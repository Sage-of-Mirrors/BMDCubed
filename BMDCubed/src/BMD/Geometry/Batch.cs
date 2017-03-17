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
            public List<short> VertIndexes;
            public List<int> PositionIndex;
            public List<int> WeightIndexes;
            public Dictionary<VertexAttributes, List<short>> AttributeData;

            public Packet()
            {
                VertIndexes = new List<short>();
                PositionIndex = new List<int>();
                WeightIndexes = new List<int>();

                AttributeData = new Dictionary<VertexAttributes, List<short>>();
            }

            public void WritePacket(EndianBinaryWriter writer)
            {
                writer.Write((byte)0x90); // Write primitive type. For the foreseeable future, we will only support triangles, which are 0x90.
                writer.Write((ushort)PositionIndex.Count); // Vertex count

                // For each vertex, we're going to run through the vertex attributes
                // and write the corresponding data.
                for (int i = 0; i < PositionIndex.Count; i++)
                {
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
                Util.PadStreamWithZero(writer, 32);
            }
        }

        public List<VertexAttributes> ActiveAttributes;
        public int AttributeIndex = 0;
        public List<Packet> BatchPackets;

        int numTris;
        int numVerts;

        public string MaterialName;
        BoundingBox Bounds;

        public Batch(Grendgine_Collada_Triangles colladaTris, DrawData drw1)
        {
            if (colladaTris.Count == 0)
                return;

            ActiveAttributes = new List<VertexAttributes>();
            BatchPackets = new List<Packet>();

            MaterialName = colladaTris.Material;
            numTris = colladaTris.Count;

            // This will parse the vertex attributes in the batch.
            int uvIndex = 0;
            int colorIndex = 0;
            foreach (Grendgine_Collada_Input_Shared input in colladaTris.Input)
            {
                switch (input.Semantic)
                {
                    case Grendgine_Collada_Input_Semantic.VERTEX:
                    case Grendgine_Collada_Input_Semantic.POSITION:
                        ActiveAttributes.Add(VertexAttributes.Position);
                        break;
                    case Grendgine_Collada_Input_Semantic.NORMAL:
                        ActiveAttributes.Add(VertexAttributes.Normal);
                        break;
                    case Grendgine_Collada_Input_Semantic.COLOR:
                        ActiveAttributes.Add(VertexAttributes.Color0 + colorIndex++);
                        break;
                    case Grendgine_Collada_Input_Semantic.TEXCOORD:
                        ActiveAttributes.Add(VertexAttributes.Tex0 + uvIndex++);
                        break;
                    default:
                        throw new FormatException(string.Format("Found unknown DAE semantic {0}!", input.Semantic));
                }
            }

            numVerts = numTris * 3;

            // Grab the index data from the DAE's string array
            string indexArrayString = colladaTris.P.Value_As_String;
            indexArrayString = indexArrayString.Replace('\n', ' ').Trim();
            int[] indexArray = Grendgine_Collada_Parse_Utils.String_To_Int(indexArrayString);

            // Create at least one packet.
            BatchPackets.Add(new Packet());

            if (drw1 != null)
                GetVertexDataWeighted(indexArray, drw1);
            else
                GetVertexDataNotWeighted(indexArray);
        }

        private void GetVertexDataWeighted(int[] indexArray, DrawData drw1)
        {
            Packet curPacket = BatchPackets[BatchPackets.Count - 1];

            // Load the data into VertIndexes. This version will add MatrixPositionIndex to the
            // list of attributes for weighted meshes.
            for (int i = 0; i < indexArray.Length; i += ActiveAttributes.Count)
            {
                int matrixPosIndex = 0;

                for (int attrib = 0; attrib < ActiveAttributes.Count; attrib++)
                {
                    if (ActiveAttributes[attrib] == VertexAttributes.Position)
                    {
                        int positionIndex = indexArray[i + attrib];
                        curPacket.PositionIndex.Add(positionIndex);

                        if (!curPacket.WeightIndexes.Contains(drw1.AllDrw1Weights.IndexOf(drw1.AllWeights[positionIndex])))
                        {
                            curPacket.WeightIndexes.Add(drw1.AllDrw1Weights.IndexOf(drw1.AllWeights[positionIndex]));
                            matrixPosIndex = (curPacket.WeightIndexes.Count - 1) * 3;
                        }
                        else
                            matrixPosIndex = curPacket.WeightIndexes.IndexOf(drw1.AllDrw1Weights.IndexOf(drw1.AllWeights[positionIndex])) * 3;

                        curPacket.VertIndexes.Add((short)((matrixPosIndex)));
                        curPacket.VertIndexes.Add((short)positionIndex);
                    }
                    else
                    {
                        curPacket.VertIndexes.Add((short)indexArray[i + attrib]);
                    }
                }
            }

            // Now that we're done parsing the data that was in the file, we can
            // add Position Matrix Index to the start of the attribute data.
            // This is the index used to start the chain that gives the
            // vertexes skinning data.
            ActiveAttributes.Insert(0, VertexAttributes.PositionMatrixIndex);

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
        }

        private void GetVertexDataNotWeighted(int[] indexArray)
        {
            Packet curPacket = BatchPackets[BatchPackets.Count - 1];

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

            curPacket.WeightIndexes.Add(0);
        }

        private void SwapVertexes(Packet packet, int vert1, int vert2)
        {
            int[] vertData1 = new int[ActiveAttributes.Count]; // Data for vertex 1
            int[] vertData2 = new int[ActiveAttributes.Count]; // Data for vertex 2

            for (int i = 0; i < ActiveAttributes.Count; i++)
            {
                // Store data from the two vertexes
                vertData1[i] = packet.VertIndexes[vert1 + i];
                vertData2[i] = packet.VertIndexes[vert2 + i];
            }

            for (int i = 0; i < ActiveAttributes.Count; i++)
            {
                // Swap the two vertexes index-by-index
                packet.VertIndexes[vert1 + i] = (short)vertData2[i];
                packet.VertIndexes[vert2 + i] = (short)vertData1[i];
            }
        }

        public void GetBoundingBoxData(List<Vector3> posList)
        {
            List<Vector3> listForBounds = new List<Vector3>();

            foreach(var packet in BatchPackets)
            {
                foreach (int inte in packet.PositionIndex)
                {
                    listForBounds.Add(posList[inte]);
                }
            }

            Bounds = new BoundingBox(listForBounds);
        }

        public void WriteBatch(EndianBinaryWriter writer, List<int> attributeOffsets, int thisIndex)
        {
            writer.Write((byte)3); // Write matrix type. 0 is basic, 1 is ???, 2 is ???, 3 is multi-matrix.
            writer.Write((byte)0xFF); // Write padding
            writer.Write((short)BatchPackets.Count); // Write packet count.

            writer.Write((short)(attributeOffsets[AttributeIndex])); // Write the offset to the attributes in this batch.
            writer.Write((short)thisIndex); // Matrix Data Offset
            writer.Write((short)thisIndex); // Packet Data Offset

            writer.Write((short)-1); // Padding

            Bounds.WriteBoundingBox(writer); // Write bounding box.
        }

        public void WritePackets(EndianBinaryWriter writer)
        {
            foreach (var packet in BatchPackets)
                packet.WritePacket(writer);
        }

        public void WriteMatrixIndexes(EndianBinaryWriter writer)
        {
            foreach(var packet in BatchPackets)
            {
                for (int i = 0; i < packet.WeightIndexes.Count; i++)
                    writer.Write((ushort)packet.WeightIndexes[i]);
            }
        }
    }
}
