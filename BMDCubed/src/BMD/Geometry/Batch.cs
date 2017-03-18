using BMDCubed.src.BMD.Skinning;
using GameFormatReader.Common;
using grendgine_collada;
using OpenTK;
using System;
using System.Collections.Generic;

namespace BMDCubed.src.BMD.Geometry
{
    class Batch
    {
        public class Packet
        {
            public class MatrixData
            {
                public ushort Unknown0;
                public ushort MatrixCount;
                public uint MatrixFirstIndex;

                public List<ushort> MatrixTableData;

                public MatrixData()
                {
                    MatrixTableData = new List<ushort>();
                    Unknown0 = 0;
                    MatrixCount = 0;
                    MatrixFirstIndex = 0;
                }
            }

            /// <summary> This stores the indexes for each type of attribute. These indexes get written to disk and reference existing data. </summary>
            public Dictionary<VertexAttributes, List<short>> AttributeData;

            /// <summary> This stores the indexes into the skinning information for this packet. </summary>
            public MatrixData PacketMatrixData;

            public Packet()
            {
                AttributeData = new Dictionary<VertexAttributes, List<short>>();
                PacketMatrixData = new MatrixData();
            }

            public void WritePrimitiveData(EndianBinaryWriter writer)
            {
                int numVertices = AttributeData[VertexAttributes.Position].Count;

                writer.Write((byte)0x90); // Write primitive type. For the foreseeable future, we will only support triangles, which are 0x90.
                writer.Write((ushort)numVertices); // Vertex count

                
                // For each vertex, we're going to run through the vertex attributes
                // and write the corresponding data.
                for (int i = 0; i < numVertices; i++)
                {
                    // Write each attribute for each vertex
                    foreach (var kvp in AttributeData)
                    {
                        if (kvp.Key == VertexAttributes.PositionMatrixIndex)
                            writer.Write((byte)(kvp.Value[i])); // PositionMatrixIndex needs to be 8 bits, so it's a byte
                        else
                            writer.Write(kvp.Value[i]); // All other attributes will use 16 bit short
                    }
                }

                // We pad to the nearest 32 byte boundary.
                // We need to pad with zeroes here because that
                // signals GX that there is no more packet data to be read.
                // ToDo: This may need to force a zero byte before padding. If a model falls exactly on a 32 alignment, does this break?
                Util.PadStreamWithZero(writer, 32);
            }
        }

        public List<Packet> BatchPackets;
        public string MaterialName;
        public IList<VertexAttributes> Attributes { get { return m_attributeListCopy; } }

        private Grendgine_Collada_Triangles m_colladaTriangleData;
        private DrawData m_drw1;

        // Cached for writing to disk later
        private int m_batchAttributeIndex;
        private BoundingBox m_bounds;

        // Blah hack
        private IList<VertexAttributes> m_attributeListCopy;

        public Batch(Grendgine_Collada_Triangles colladaTris, DrawData drw1)
        {
            m_colladaTriangleData = colladaTris;
            m_drw1 = drw1;
            BatchPackets = new List<Packet>();
        }

        /// <summary>
        /// This converts the raw triangle data into the data needed by the SHP1 section. This allows us to create
        /// other data (packet data, etc.) as we go and have it all get pushed into the master arrays in SHP1.
        /// </summary>
        /// <param name="batchData"></param>
        public void ConvertDataToFinalFormat(BatchData batchData)
        {
            var batchAttributes = ConvertColladaAttributes();
            m_batchAttributeIndex = batchData.GetIndexForBatchAttributes(batchAttributes);

            MaterialName = m_colladaTriangleData.Material;

            // Grab the index data from the DAE's string array
            string indexArrayString = m_colladaTriangleData.P.Value_As_String;
            indexArrayString = indexArrayString.Replace('\n', ' ').Trim();
            int[] triangleIndexes = Grendgine_Collada_Parse_Utils.String_To_Int(indexArrayString);

            if (m_drw1 != null)
                GetVertexDataWeighted(triangleIndexes, batchData.GetIndexForBatchAttributes(m_batchAttributeIndex));
            else
                GetVertexDataNotWeighted(triangleIndexes);
        }

        private void GetVertexDataWeighted(int[] triangleArray, List<VertexAttributes> attributes)
        {
            Packet curPacket = null;

            // Collada gives us one index per attribute, for a total of (3 * numAttributes) per triangle.
            // We need to get a copy of the attributes array without the PositionMatrixIndex however as
            // Collada indexes don't know anything about that one.
            var attribCopy = new List<VertexAttributes>(attributes);
            attribCopy.Remove(VertexAttributes.PositionMatrixIndex);

            for (int colladaIndex = 0; colladaIndex < triangleArray.Length;)
            {
                // If we don't have enough possible spots for our PMI's we'll just start a new packet at the end of this triangle.
                if (curPacket == null /*|| curPacket.PacketMatrixData.MatrixCount >= 7*/)
                {
                    curPacket = new Packet();
                    BatchPackets.Add(curPacket);

                    // Initialize an array to hold the indexes of each attribute within our Packet's data.
                    foreach (var attribute in attributes)
                        curPacket.AttributeData[attribute] = new List<short>();
                }

                for(int tri = 0; tri < 3; tri++)
                {
                    for (int attribIndex = 0; attribIndex < attribCopy.Count; attribIndex++)
                    {
                        // Assign the index into the AttributeData for this attribute on this packet
                        curPacket.AttributeData[attribCopy[attribIndex]].Add((short)triangleArray[colladaIndex + attribIndex]);

                        // We only care about the Position attribute while finding skinning information
                        if (attribCopy[attribIndex] != VertexAttributes.Position)
                            continue;

                        int vertPosIndex = triangleArray[colladaIndex + attribIndex];

                        Weight vertexWeight = m_drw1.AllWeights[vertPosIndex];
                        ushort vertexWeightIndex = (ushort)m_drw1.AllDrw1Weights.IndexOf(vertexWeight);

                        if (!curPacket.PacketMatrixData.MatrixTableData.Contains(vertexWeightIndex))
                        {
                            curPacket.PacketMatrixData.MatrixTableData.Add(vertexWeightIndex);
                        }

                        int positionMatrixIndex = curPacket.PacketMatrixData.MatrixTableData.IndexOf(vertexWeightIndex);
                        curPacket.AttributeData[VertexAttributes.PositionMatrixIndex].Add((short)(positionMatrixIndex *3));
                        /*if (!curPacket.WeightIndexes.Contains(drw1.AllDrw1Weights.IndexOf(drw1.AllWeights[positionIndex])))
                        {
                            curPacket.WeightIndexes.Add(drw1.AllDrw1Weights.IndexOf(drw1.AllWeights[positionIndex]));
                            matrixPosIndex = (curPacket.WeightIndexes.Count - 1) * 3;
                        }
                        else
                            matrixPosIndex = curPacket.WeightIndexes.IndexOf(drw1.AllDrw1Weights.IndexOf(drw1.AllWeights[positionIndex])) * 3;*/
                    }

                    colladaIndex += attribCopy.Count;
                }
            }

            // Reverse Triangle Winding Order
            int vertexCount = curPacket.AttributeData[VertexAttributes.Position].Count;
            for (int i = 0; i < vertexCount; i += 3)
            {
                SwapVertexes(curPacket, i, i+2, attributes);
            }
        }

        private void GetVertexDataNotWeighted(int[] indexArray)
        {
            throw new NotImplementedException();

            /*Packet curPacket = BatchPackets[BatchPackets.Count - 1];

            // This will get the vertex indexes into VertIndexes.
            // This version does not use the PositionMatrixIndex attribute.
            for (int i = 0; i < indexArray.Length; i += ActiveAttributes.Count)
            {
                for (int attrib = 0; attrib < ActiveAttributes.Count; attrib++)
                {
                    if (ActiveAttributes[attrib] == VertexAttributes.Position)
                    {
                        int positionIndex = indexArray[i + attrib];
                        curPacket.PositionIndex.Add(positionIndex);
                    }

                    curPacket.VertIndexes.Add((short)indexArray[i + attrib]);
                }
            }

            foreach (VertexAttributes attrib in ActiveAttributes)
                curPacket.AttributeData.Add(attrib, new List<short>());

            // The triangles from the DAE have the wrong winding order. We need to swap the first
            // and last vertexes of each triangle to flip them around.
            // If we don't do that, the mesh will render inside-out!
            // We add 3 * ActiveAttributes.Count so that we can get the correct indexes of each
            // vertex triplet.
            for (int i = 0; i < curPacket.VertIndexes.Count; i += 3 * ActiveAttributes.Count)
            {
                SwapVertexes(curPacket, i, i + (2 * ActiveAttributes.Count));
            }

            // We'll separate the indexes by attribute type. This will allow us to 
            // sort the attributes in ActiveAttributes independently of the indexes'
            // order. With that, we can give GX the attribute indexes in the order
            // that it expects.
            int runningIndex = 0;
            for (int i = 0; i < numVerts; i++)
            {
                foreach (VertexAttributes attrib in ActiveAttributes)
                {
                    curPacket.AttributeData[attrib].Add(curPacket.VertIndexes[runningIndex++]);
                }
            }

            ActiveAttributes.Sort();

            curPacket.WeightIndexes.Add(0);*/
        }

        private void SwapVertexes(Packet packet, int vert1, int vert2, IList<VertexAttributes> attributes)
        {
            int[] vertData1 = new int[attributes.Count]; // Data for vertex 1
            int[] vertData2 = new int[attributes.Count]; // Data for vertex 2

            for (int i = 0; i < attributes.Count; i++)
            {
                // Store data from the two vertexes
                vertData1[i] = packet.AttributeData[attributes[i]][vert1];
                vertData2[i] = packet.AttributeData[attributes[i]][vert2];
            }

            for (int i = 0; i < attributes.Count; i++)
            {
                // Swap the two vertexes index-by-index
                packet.AttributeData[attributes[i]][vert1] = (short)vertData2[i];
                packet.AttributeData[attributes[i]][vert2] = (short)vertData1[i];
            }
        }

        public void CalculateBoundingBoxData(List<Vector3> posList)
        {
            List<Vector3> listForBounds = new List<Vector3>();

            foreach (var packet in BatchPackets)
            {
                foreach (var posIndex in packet.AttributeData[VertexAttributes.Position])
                    listForBounds.Add(posList[posIndex]);
            }

            m_bounds = new BoundingBox(listForBounds);
        }

        public void WriteBatch(EndianBinaryWriter writer, BatchData shp1)
        {
            ushort attributeListOffset; // Offset from start of Attribute List (in bytes) to this Batch's attributes
            ushort firstMatrixDataIndex; // First Matrix Data this Batch uses
            ushort firstPacketIndex; // First Packet this Batch uses

            shp1.GetBatchAttributeOffset(m_batchAttributeIndex, out attributeListOffset);
            shp1.WriteBatchMatrixData(BatchPackets, out firstMatrixDataIndex);
            shp1.WriteBatchPackets(BatchPackets, out firstPacketIndex);

            writer.Write((byte)3);                      // Write Matrix Type. 0 is basic, 1 is ???, 2 is ???, 3 is multi-matrix.
            writer.Write((byte)0xFF);                   // Write padding
            writer.Write((short)BatchPackets.Count);    // Write packet count.
            writer.Write((short)attributeListOffset);   // Offset in Bytes to first Attribute
            writer.Write((short)firstMatrixDataIndex);  // First Matrix Index
            writer.Write((short)firstPacketIndex);      // First Packet Index

            writer.Write((ushort)0xFFFF);                  // Padding
            writer.Write(m_bounds.SphereRadius);        // Bounding Sphere Diameter
            writer.Write(m_bounds.Minimum.X);           // Bounding Box Mins
            writer.Write(m_bounds.Minimum.Y);
            writer.Write(m_bounds.Minimum.Z);
            writer.Write(m_bounds.Maximum.X);           // Bounding Box Max
            writer.Write(m_bounds.Maximum.Y);
            writer.Write(m_bounds.Maximum.Z);
        }

        /// <summary>
        /// This parses the Collada data and converts the Collada attributes into J3D <see cref="VertexAttributes"/>.
        /// </summary>
        /// <returns></returns>
        private VertexAttributes[] ConvertColladaAttributes()
        {
            var attributes = new List<VertexAttributes>();

            int uvIndex = 0;
            int colorIndex = 0;
            foreach (Grendgine_Collada_Input_Shared input in m_colladaTriangleData.Input)
            {
                switch (input.Semantic)
                {
                    case Grendgine_Collada_Input_Semantic.VERTEX:
                    case Grendgine_Collada_Input_Semantic.POSITION:
                        attributes.Add(VertexAttributes.Position);
                        break;
                    case Grendgine_Collada_Input_Semantic.NORMAL:
                        attributes.Add(VertexAttributes.Normal);
                        break;
                    case Grendgine_Collada_Input_Semantic.COLOR:
                        attributes.Add(VertexAttributes.Color0 + colorIndex++);
                        break;
                    case Grendgine_Collada_Input_Semantic.TEXCOORD:
                        attributes.Add(VertexAttributes.Tex0 + uvIndex++);
                        break;
                    default:
                        throw new FormatException(string.Format("Found unknown DAE semantic {0}!", input.Semantic));
                }
            }

            // If this model has skinning data then we'll insert a PositionMatrixIndex attribute.
            if (m_drw1 != null)
                attributes.Add(VertexAttributes.PositionMatrixIndex);

            // Sort the Attributes in ascending order to make GX happy
            attributes.Sort();
            m_attributeListCopy = attributes;

            return attributes.ToArray();
        }
    }
}
