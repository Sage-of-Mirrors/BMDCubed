using BMDCubed.src.BMD.Skinning;
using GameFormatReader.Common;
using grendgine_collada;
using OpenTK;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BMDCubed.src.BMD.Geometry
{
    class BatchData
    {
        public List<Batch> Batches;
        public int PacketCount;
        List<int> vertexAttributeOffsets;

        // Batches can share their attributes, so we store an array of each set of attributes batches use.
        // Then, in the actual batch, we can store the index to their set of attributes.
        List<List<VertexAttributes>> ActiveAttributesPerBatch;

        private EndianBinaryWriter m_batchPacketData;
        private EndianBinaryWriter m_batchPrimitiveData;
        private EndianBinaryWriter m_matrixTableData;
        private EndianBinaryWriter m_matrixDataData;

        public BatchData(Grendgine_Collada_Mesh mesh, DrawData drw1)
        {
            Batches = new List<Batch>();

            foreach (Grendgine_Collada_Triangles tri in mesh.Triangles)
            {
                // Tri count was zero, couldn't initialize batch
                if (tri.Count == 0)
                    continue;

                Batch batch = new Batch(tri, drw1);
                Batches.Add(batch);
            }

            ActiveAttributesPerBatch = new List<List<VertexAttributes>>();

            // Convert our Collada Data to the data needed for each SHP1.Batch
            foreach (var batch in Batches)
            {
                batch.ConvertDataToFinalFormat(this);
            }

            // Get final packet count
            foreach (var batch in Batches)
            {
                PacketCount += batch.BatchPackets.Count;
            }
        }

        public void SetBoundingBoxes(List<Vector3> posList)
        {
            foreach (Batch batch in Batches)
                batch.CalculateBoundingBoxData(posList);
        }

        public void WriteSHP1(EndianBinaryWriter writer)
        {
            vertexAttributeOffsets = new List<int>();
            m_batchPacketData = new EndianBinaryWriter(new MemoryStream(), Endian.Big);
            m_matrixTableData = new EndianBinaryWriter(new MemoryStream(), Endian.Big);
            m_matrixDataData = new EndianBinaryWriter(new MemoryStream(), Endian.Big);
            m_batchPrimitiveData = new EndianBinaryWriter(new MemoryStream(), Endian.Big);

            writer.Write("SHP1".ToCharArray());
            writer.Write(0); // Placeholder for chunk size
            writer.Write((short)Batches.Count); // Number of Batches in SHP1
            writer.Write((short)-1); // Padding

            writer.Write(0x2C); // Offset to batch data
            writer.Write(0); // Placeholder for offset to index remap table
            writer.Write(0); // Unknown offset. Always 0
            writer.Write(0); // Placeholder for offset to Batch Attributes
            writer.Write(0); // Placeholder for offset to the Matrix Table (stores up to 10 indexes per batch)
            writer.Write(0); // Placeholder for offset to primitive data
            writer.Write(0); // Placeholder for offset to matrix data
            writer.Write(0); // Placeholder for offset to packet location data

            // This writes the actual batch information, and initializes the BatchAttribute, MatrixTable, MatrixData, and PrimitiveData 
            WriteBatchDataToStream(writer);

            // SHP1 has an index remap table. We don't remap anything so we'll fill in a dummy table.
            Util.WriteOffset(writer, 0x10);
            for (int i = 0; i < Batches.Count; i++)
                writer.Write((short)i);

            // Next we need to write the Attributes that all Batches use.
            Util.WriteOffset(writer, 0x18); // Skips the Unknown Offset at 0x14
            WriteBatchAttributesToStream(writer);

            // Next we write the MatrixTable that all Batches contributed to
            Util.WriteOffset(writer, 0x1C);
            WriteMatrixTableToStream(writer);
            Util.PadStreamWithString(writer, 32); // Pad out to 32 byte alignment.


            // Then we write the PrimitiveData that all Batches contributed to
            Util.WriteOffset(writer, 0x20);
            WritePrimitiveDataToStream(writer);

            // Then we write the Matrix Data that all Batches contributed to
            Util.WriteOffset(writer, 0x24);
            WriteMatrixDataToStream(writer);

            // Finally we write the Packet Location
            Util.WriteOffset(writer, 0x28);
            WritePacketDataToStream(writer);

            // Pad our our SHP1 chunk to the next 32-byte alignment boundry
            Util.PadStreamWithString(writer, 32);

            // Finally go back and write the full chunk size
            Util.WriteOffset(writer, 4);


            m_batchPacketData.Dispose();
            m_batchPrimitiveData.Dispose();
            m_matrixTableData.Dispose();
            m_matrixDataData.Dispose();

            m_batchPacketData = null;
            m_batchPrimitiveData = null;
            m_matrixTableData = null;
            m_matrixDataData = null;
        }

        private void WriteBatchDataToStream(EndianBinaryWriter writer)
        {
            foreach (var batch in Batches)
                batch.WriteBatch(writer, this);
        }

        private void WriteMatrixTableToStream(EndianBinaryWriter writer)
        {
            var memoryStream = m_matrixDataData.BaseStream as MemoryStream;
            writer.Write(memoryStream.ToArray());
        }

        private void WriteMatrixDataToStream(EndianBinaryWriter writer)
        {
            var memoryStream = m_matrixTableData.BaseStream as MemoryStream;
            writer.Write(memoryStream.ToArray());
        }

        private void WriteBatchAttributesToStream(EndianBinaryWriter writer)
        {
            foreach (var attributeSet in ActiveAttributesPerBatch)
            {
                // Write each attribute in this set
                foreach (var attribute in attributeSet)
                {
                    // Write the Attribute
                    writer.Write((int)attribute);

                    // Write the Data Type
                    if (attribute == VertexAttributes.PositionMatrixIndex)
                        writer.Write((int)0x1); // Data Type is an Unsigned Byte (U8)
                    else
                        writer.Write((int)0x3); // Data Type is Unsigned Short (U16);
                }

                // Add null attribute. Tells the GPU there are no more attributes to read
                writer.Write((int)0xFF);
                writer.Write((int)0);
            }
        }

        private void WritePacketDataToStream(EndianBinaryWriter writer)
        {
            var memoryStream = m_batchPacketData.BaseStream as MemoryStream;
            writer.Write(memoryStream.ToArray());
        }

        private void WritePrimitiveDataToStream(EndianBinaryWriter writer)
        {
            var memoryStream = m_batchPrimitiveData.BaseStream as MemoryStream;
            writer.Write(memoryStream.ToArray());
        }

        internal void GetBatchAttributeOffset(int batchAttributeIndex, out ushort attributeListOffset)
        {
            ushort offset = 0;
            for (int i = 0; i < batchAttributeIndex; i++)
            {
                // We add one attribute to the count to represent the null attribute added to each set.
                offset += (ushort)(ActiveAttributesPerBatch[batchAttributeIndex].Count + 1);
            }

            attributeListOffset = (ushort)(offset * 8);
        }

        internal void WriteBatchMatrixData(List<Batch.Packet> batchPackets, out ushort firstMatrixDataIndex)
        {
            // Return the first matrix for this batch.
            firstMatrixDataIndex = (ushort)(m_matrixTableData.BaseStream.Length / 0x8);

            // Remember:
            // MatrixTable is just a header that is associated with each packet that has Unknown0, MatrixCount and FirstMatrixIndex
            // then we have the MatrixData which is the raw indexes. So we need to write both of them separately,
            // and FirstMatrixIndex is the index into the MatrixData that that packet uses first, and then MatrixCount successive ones follow.
            foreach (var packet in batchPackets)
            {
                m_matrixTableData.Write((ushort)0); // Unknown 0
                m_matrixTableData.Write((ushort)packet.PacketMatrixData.MatrixTableData.Count); // How many matrices
                m_matrixTableData.Write((uint)m_matrixDataData.BaseStream.Length / 2); // Index to the first one

                // Then write all of the actual indexes
                foreach (var matrixIndex in packet.PacketMatrixData.MatrixTableData)
                    m_matrixDataData.Write((ushort)matrixIndex);
            }
        }

        internal void WriteBatchPackets(List<Batch.Packet> batchPackets, out ushort firstPacketIndex)
        {
            // Return the first packet for this batch
            firstPacketIndex = (ushort)(m_batchPacketData.BaseStream.Length / 0x8);

            foreach (var packet in batchPackets)
            {
                long streamStart = m_batchPrimitiveData.BaseStream.Position;

                // Write the primitive data.
                packet.WritePrimitiveData(m_batchPrimitiveData);

                // Calculate the packet size
                int packetSize = (int)(m_batchPrimitiveData.BaseStream.Position - streamStart);
                int primitiveOffset = (int)streamStart;

                m_batchPacketData.Write((int)packetSize); // How much primitive data is there for this packet?
                m_batchPacketData.Write((int)primitiveOffset); // Offset into the primitive data that this packet has.
            }
        }

        /// <summary>
        /// Given the input array of attributes, find its matching index in the <see cref="ActiveAttributesPerBatch"/> array.
        /// If no match is found, the array of attributes is added and the new index is returned.
        /// </summary>
        /// <param name="batchAttributes"></param>
        /// <returns></returns>
        public int GetIndexForBatchAttributes(VertexAttributes[] batchAttributes)
        {
            for (int i = 0; i < ActiveAttributesPerBatch.Count; i++)
            {
                if (ActiveAttributesPerBatch[i].SequenceEqual(batchAttributes))
                    return i;
            }

            // If we made it this far, there's no attributes which match yet.
            ActiveAttributesPerBatch.Add(new List<VertexAttributes>(batchAttributes));
            return ActiveAttributesPerBatch.Count - 1;
        }

        public List<VertexAttributes> GetIndexForBatchAttributes(int index)
        {
            return ActiveAttributesPerBatch[index];
        }
    }
}
