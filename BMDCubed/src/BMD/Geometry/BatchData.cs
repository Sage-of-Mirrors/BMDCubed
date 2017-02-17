using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using grendgine_collada;
using OpenTK;
using GameFormatReader.Common;
using System.IO;
using BMDCubed.src.BMD.Skinning;

namespace BMDCubed.src.BMD.Geometry
{
    class BatchData
    {
        public List<Batch> Batches;
        List<VertexAttributes>[] ActiveAttributesPerBatch;
        List<int> vertexAttributeOffsets;

        public BatchData(Grendgine_Collada_Mesh mesh, DrawData drw1)
        {
            Batches = new List<Batch>();

            foreach (Grendgine_Collada_Triangles tri in mesh.Triangles)
            {
                Batch batch = new Batch(tri, drw1);

                // Tri count was zero, couldn't initialize batch
                if (batch.ActiveAttributes == null)
                    continue;
                else
                    Batches.Add(batch);
            }

            ActiveAttributesPerBatch = new List<VertexAttributes>[Batches.Count];

            foreach (Batch bat in Batches)
            {
                for (int i = 0; i < Batches.Count; i++)
                {
                    if (ActiveAttributesPerBatch[i] == null)
                    {
                        ActiveAttributesPerBatch[i] = bat.ActiveAttributes;
                        bat.AttributeIndex = i;
                        break;
                    }
                    else
                    {
                        if (ActiveAttributesPerBatch[i].SequenceEqual(bat.ActiveAttributes))
                        {
                            bat.AttributeIndex = i;
                            break;
                        }
                    }
                }
            }
        }

        public void SetBoundingBoxes(List<Vector3> posList)
        {
            foreach (Batch batch in Batches)
                batch.GetBoundingBoxData(posList);
        }

        public void WriteSHP1(EndianBinaryWriter writer)
        {
            vertexAttributeOffsets = new List<int>();

            writer.Write("SHP1".ToCharArray());
            writer.Write(0); // Placeholder for chunk size
            writer.Write((short)Batches.Count);
            writer.Write((short)-1);

            writer.Write(0x2C); // Offset to batch data
            writer.Write(0); // Placeholder for offset to index table
            writer.Write(0); // Placeholder for offset to ???
            writer.Write(0); // Placeholder for offset to batch attributes
            writer.Write(0); // Placeholder for offset to matrix table
            writer.Write(0); // Placeholder for offset to packet data
            writer.Write(0); // Placeholder for offset to matrix data
            writer.Write(0); // Placeholder for offset to packet location data

            // Batch data is found *before* the attribute data. Unfortunately, we need the attribute data
            // first so we can get the offset of the attribute set that each batch uses.
            // So we're going to write the attribute data to a memory stream, then use that to
            // write the batch data to the output stream.
            // Then we'll copy the attribute data to the output stream.
            using (MemoryStream batchAttribData = new MemoryStream())
            {
                EndianBinaryWriter attribWriter = new EndianBinaryWriter(batchAttribData, Endian.Big);

                // Write attribute data to memorystream
                foreach (List<VertexAttributes> dat in ActiveAttributesPerBatch)
                {
                    if (dat == null)
                        break;

                    vertexAttributeOffsets.Add((int)(attribWriter.BaseStream.Length));

                    for (int i = 0; i < dat.Count; i++)
                    {
                        attribWriter.Write((int)dat[i]);

                        if (dat[i] == VertexAttributes.PositionMatrixIndex)
                            attribWriter.Write(1);
                        else
                            attribWriter.Write(3);
                    }

                    // Add null attribute. Tells the GPU there are no more attributes to read
                    attribWriter.Write(0xFF);
                    attribWriter.Write(0);
                }

                // Write batch data
                WriteBatches(writer);

                // Write index array offset. Don't know what this does, really
                Util.WriteOffset(writer, 0x10);

                // Write index array offset
                for (int i = 0; i < Batches.Count; i++)
                    writer.Write((short)i);

                Util.PadStreamWithString(writer, 32);

                // Write attribute data offset
                Util.WriteOffset(writer, 0x18);

                // Write attribute data
                writer.Write(batchAttribData.ToArray());

                //Util.PadStreamWithString(writer, 32);
            }

            // Write matrix indexes offset
            Util.WriteOffset(writer, 0x1C);

            // Write matrix indexes
            foreach (Batch bat in Batches)
            {
                bat.WriteMatrixIndexes(writer);
            }

            Util.PadStreamWithString(writer, 32);

            // Write packet offset
            Util.WriteOffset(writer, 0x20);

            // We're going to keep track of the packets' length and offset relative to the
            // start of the packet data so we can write the packet info data later on.
            List<int> packetOffsets = new List<int>();
            List<int> packetSizes = new List<int>();
            packetOffsets.Add(0);

            int basePos = (int)writer.BaseStream.Length; // Offset of first packet
            int lastPos = basePos; // This will hold the offset of the last packet so we can calculate size

            // Write packet data
            foreach (Batch bat in Batches)
            {
                bat.WritePacket(writer);
                packetOffsets.Add((int)writer.BaseStream.Length - basePos);
                packetSizes.Add((int)writer.BaseStream.Length - lastPos);

                lastPos = (int)writer.BaseStream.Length;
            }

            // Write matrix info offset
            Util.WriteOffset(writer, 0x24);

            int matrixIndexCount = 0;

            // Write matrix info
            foreach (Batch bat in Batches)
            {
                writer.Write((short)1);
                writer.Write((ushort)bat.WeightIndexes.Count);
                writer.Write(matrixIndexCount);

                matrixIndexCount += bat.WeightIndexes.Count;
            }

            // Write packet info offset
            Util.WriteOffset(writer, 0x28);

            // Write packet info
            for (int i = 0; i < Batches.Count; i++)
            {
                writer.Write(packetSizes[i]);
                writer.Write(packetOffsets[i]);
            }

            Util.PadStreamWithString(writer, 32);

            // Write chunk size
            Util.WriteOffset(writer, 4);
        }

        private void WriteBatches(EndianBinaryWriter writer)
        {
            for (int i = 0; i < Batches.Count; i++)
                Batches[i].WriteBatch(writer, vertexAttributeOffsets, i);
        }
    }
}
