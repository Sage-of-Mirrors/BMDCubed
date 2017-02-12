using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using grendgine_collada;
using OpenTK;
using GameFormatReader.Common;
using BMDCubed.src.BMD.Skinning;

namespace BMDCubed.src.BMD.Geometry
{
    class Batch
    {
        public List<VertexAttributes> ActiveAttributes;
        public int AttributeIndex = 0;

        List<short> VertIndexes;

        List<int> PositionIndex;
        public List<int> WeightIndexes;
        List<int> VertexWeightIndexes;
        int numTris;
        int numVerts;
        public string MaterialName;
        BoundingBox Bounds;

        public Batch(Grendgine_Collada_Triangles tri, DrawData drw1)
        {
            if (tri.Count == 0)
                return;

            ActiveAttributes = new List<VertexAttributes>();
            VertIndexes = new List<short>();
            PositionIndex = new List<int>();
            WeightIndexes = new List<int>();
            MaterialName = tri.Material;
            numTris = tri.Count;

            int uvIndex = 0;
            int colorIndex = 0;
            foreach (Grendgine_Collada_Input_Shared input in tri.Input)
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
                }
            }

            numVerts = numTris * 3;

            string indexArrayString = tri.P.Value_As_String;
            indexArrayString = indexArrayString.Replace('\n', ' ').Trim();
            int[] indexArray = Grendgine_Collada_Parse_Utils.String_To_Int(indexArrayString);

            for (int i = 0; i < indexArray.Length; i += ActiveAttributes.Count)
            {
                for (int attrib = 0; attrib < ActiveAttributes.Count; attrib++)
                {
                    if (ActiveAttributes[attrib] == VertexAttributes.Position)
                    {
                        int positionIndex = indexArray[i + attrib];
                        PositionIndex.Add(positionIndex);
                        VertIndexes.Add((short)positionIndex);

                        if (drw1.AllWeights[positionIndex].BoneIndexes.Count > 1)
                        {

                        }

                        WeightIndexes.Add(drw1.AllDrw1Weights.IndexOf(drw1.AllWeights[positionIndex]));
                    }
                    else
                    {
                        VertIndexes.Add((short)indexArray[i + attrib]);
                    }
                }

                VertIndexes.Add((short)((WeightIndexes.Count - 1) * 3));
            }

            ActiveAttributes.Add(VertexAttributes.PositionMatrixIndex);
        }

        public void GetBoundingBoxData(List<Vector3> posList)
        {
            List<Vector3> listForBounds = new List<Vector3>();

            foreach (int inte in PositionIndex)
            {
                listForBounds.Add(posList[inte]);
            }

            Bounds = new BoundingBox(listForBounds);
        }

        public void WriteBatch(EndianBinaryWriter writer, List<int> attributeOffsets, int thisIndex)
        {
            writer.Write((byte)3);
            writer.Write((byte)0xFF);
            writer.Write((short)1);

            writer.Write((short)(attributeOffsets[AttributeIndex]));
            writer.Write((short)thisIndex);
            writer.Write((short)thisIndex);

            writer.Write((short)-1);

            Bounds.WriteBoundingBox(writer);
        }

        public void WriteMatrixIndexes(EndianBinaryWriter writer)
        {
            for (int i = 0; i < WeightIndexes.Count; i++)
                writer.Write((ushort)WeightIndexes[i]);
        }

        public void WritePacket(EndianBinaryWriter writer)
        {
            writer.Write((byte)0x90);
            writer.Write((ushort)numVerts);

            for (int i = 0; i < VertIndexes.Count; i++)
                writer.Write((ushort)VertIndexes[i]);

            Util.PadStreamWithZero(writer, 8);
        }
    }
}
