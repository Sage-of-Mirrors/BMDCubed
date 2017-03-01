using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;
using BMDCubed.src.BMD.Skinning;

namespace BMDCubed.src.BMD.Geometry
{
    class Packet
    {
        List<int> VertexIndexes;
        List<Weight> ActiveWeights;
        List<int> ActiveMatrixes;
        public Dictionary<VertexAttributes, List<int>> AttributeData;

        int triCount;
        int vertexCount;

        int matrixCount;

        public Packet(List<VertexAttributes> attributeList)
        {
            VertexIndexes = new List<int>();
            ActiveWeights = new List<Weight>();
            AttributeData = new Dictionary<VertexAttributes, List<int>>();
            ActiveMatrixes = new List<int>();

            for (int i = 0; i < attributeList.Count; i++)
                AttributeData.Add(attributeList[i], new List<int>());
        }

        public void AddTriVertexes(Triangle tri)
        {
            for (int i = 0; i < 3; i++)
            {
                int attribIndex = 0;
                foreach (VertexAttributes attribute in AttributeData.Keys)
                {
                    AttributeData[attribute].Add(tri.VertexIndexes[i][attribIndex++]);
                }

                if (!ActiveWeights.Contains(tri.Weights[i]))
                    ActiveWeights.Add(tri.Weights[i]);
            }

            for (int i = 0; i < tri.MatrixList.Count; i++)
            {
                if (!ActiveMatrixes.Contains(tri.MatrixList[i]))
                    ActiveMatrixes.Add(tri.MatrixList[i]);
            }

            triCount++;
            vertexCount += 3;
        }

        public bool CanAddTriToPacket(Triangle tri)
        {
            // We're going to see how many matrices we need to add to add this triangle to this packet.
            // If adding this triangle would cause the ActiveWeight list to go over 10 weights, we will
            // return false in order to make a new packet and insert this triangle into it.

            int newMatrixCount = 0;

            // If we need to add new weights to the packet, we need
            // to check if the number of matrixes will go over 10.
            // We'll calculate the number of matrixes that need to be added
            // and then compare it to the current number of matrixes.
            for (int i = 0; i < tri.MatrixList.Count; i++)
            {
                if (!ActiveMatrixes.Contains(tri.MatrixList[i]))
                    newMatrixCount++;
            }

            // If the current number of matrices + the number of matrices to be add is less than or
            // equal to 10, we'll update the current matrix count and return true.
            if (matrixCount + newMatrixCount <= 10)
            {
                matrixCount += newMatrixCount;
                return true;
            }
            // Otherwise, we need to create a new packet to put this triangle in.
            else
                return false;
        }

        public void WriteMatrixIndexes(EndianBinaryWriter writer)
        {
            for (int i = 0; i < AttributeData[VertexAttributes.PositionMatrixIndex].Count; i++)
                writer.Write((short)AttributeData[VertexAttributes.PositionMatrixIndex][i]);
        }

        public void WriteMatrixEntryData(EndianBinaryWriter writer, int matrixIndexCount)
        {
            writer.Write((short)1);
            writer.Write((ushort)AttributeData[VertexAttributes.PositionMatrixIndex].Count);
            writer.Write(matrixIndexCount);
        }

        public void WritePacket(EndianBinaryWriter writer, List<VertexAttributes> WriteOrder)
        {
            writer.Write((byte)0x90); // Triangle primitive type. This tool only supports triangles.
            writer.Write((short)vertexCount);

            // For each vertex...
            for (int i = 0; i < vertexCount; i++)
            {
                // For each attribute that makes up a vertex here...
                foreach (VertexAttributes attrib in WriteOrder)
                {
                    // Position matrix index needs to be a byte
                    if (attrib == VertexAttributes.PositionMatrixIndex)
                        writer.Write((byte)AttributeData[attrib][i]);
                    // All other attributes will be a ushort
                    else
                        writer.Write((ushort)AttributeData[attrib][i]);
                }
            }

            Util.PadStreamWithZero(writer, 32); // Primitives must be padded to 32 bytes
        }
    }
}
