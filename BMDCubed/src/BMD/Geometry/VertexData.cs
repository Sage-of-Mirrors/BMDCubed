using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using grendgine_collada;
using GameFormatReader.Common;
using OpenTK;
using System.IO;
using BMDCubed.src.BMD.Skinning;

namespace BMDCubed.src.BMD.Geometry
{
    public enum VertexAttributes
    {
        PositionMatrixIndex, Tex0MatrixIndex, Tex1MatrixIndex, Tex2MatrixIndex, Tex3MatrixIndex,
        Tex4MatrixIndex, Tex5MatrixIndex, Tex6MatrixIndex, Tex7MatrixIndex,
        Position, Normal, Color0, Color1, Tex0, Tex1, Tex2, Tex3, Tex4, Tex5, Tex6, Tex7,
        PositionMatrixArray, NormalMatrixArray, TextureMatrixArray, LitMatrixArray, NormalBinormalTangent,
        MaxAttr, NullAttr = 0xFF,
    }

    public enum DataTypes
    {
        U8, S8, U16, S16, F32
    }

    public enum ColorDataTypes
    {
        RGB565, RGB8, RGBX8, RGBA4, RGBA6, RGBA8
    }

    class VertexData
    {
        public List<Vertex> Vertices;
        public List<Vector3> Positions;
        public List<Vector3> Normals;

        public List<Vector2>[] UVData;
        public List<Vector3>[] ColorData;

        DataTypes PositionType;
        DataTypes NormalType;
        DataTypes UVType;
        ColorDataTypes ColorType;

        int PositionFractionalBitVal;
        int NormalFractionalBitVal;
        int UVFractionalBitVal;

        Dictionary<VertexAttributes, string> activeAttributes;

        public VertexData(Grendgine_Collada_Mesh mesh, Matrix4 bindShape, DataTypes position = DataTypes.F32, DataTypes normal = DataTypes.F32,
            DataTypes uv = DataTypes.F32, ColorDataTypes color = ColorDataTypes.RGB8)
        {
            Vertices = new List<Vertex>();

            Positions = new List<Vector3>();
            Normals = new List<Vector3>();
            UVData = new List<Vector2>[8];
            ColorData = new List<Vector3>[2];

            PositionType = position;
            NormalType = normal;
            UVType = uv;
            ColorType = color;

            activeAttributes = new Dictionary<VertexAttributes, string>();
            GetActiveVertAttributes(mesh);
            FillAttributeLists(mesh);

            PositionFractionalBitVal = GetFractionalVec3(Positions);
            NormalFractionalBitVal = GetFractionalVec3(Normals);
            UVFractionalBitVal = 15;

            StringBuilder bld = new StringBuilder();

            foreach (Vector3 vec in Positions)
                bld.AppendLine(vec.ToString());
            File.WriteAllText(@"D:\vertex.txt", bld.ToString());

            for (int i = 0; i < Positions.Count; i++)
            {
                Positions[i] = Vector3.TransformPosition(Positions[i], bindShape);
            }

            foreach (List<Vector2> list2 in UVData)
            {
                if (list2 != null)
                {
                    for (int i = 0; i < list2.Count; i++)
                    {
                        Vector2 vec = list2[i];
                        vec.X = 1 - vec.X;
                        vec.Y = 1 - vec.Y;
                        list2[i] = vec;
                    }
                }
            }
        }

        public void TransformPositions(List<Weight> weights, List<Bone> bones)
        {
            for (int i = 0; i < Positions.Count; i++)
            {
                Weight weight = weights[i];
                Matrix4 cumMat = Matrix4.Identity;

                for (int b = 0; b < weight.BoneIndexes.Count; b++)
                {
                    Matrix4 invBind = bones[weight.BoneIndexes[b]].InverseBindMatrix;
                    cumMat *= Matrix4.Mult(invBind, weight.BoneWeights[b]);
                }

                Positions[i] = Vector3.TransformPosition(Positions[i], cumMat);
            }
        }

        public void WriteVTX1(EndianBinaryWriter writer)
        {
            writer.Write("VTX1".ToCharArray()); // "VTX1" header
            writer.Write((int)0); // Placeholder for chunk size
            writer.Write((int)0x40); // Offset to attribute data

            // Placeholder offsets to data banks
            for (int i = 0; i < 13; i++)
                writer.Write((int)0);

            // Vertex attributes
            foreach (KeyValuePair<VertexAttributes, string> val in activeAttributes)
                WriteAttribute(writer, val.Key);
            WriteAttribute(writer, VertexAttributes.NullAttr);

            Util.PadStreamWithString(writer, 32);

            foreach (KeyValuePair<VertexAttributes, string> val in activeAttributes)
            {
                WriteDataBankOffset(writer, val.Key);
                WriteDataBank(writer, val.Key);
                Util.PadStreamWithString(writer, 32);
            }

            // Write chunk size
            writer.Seek(4, System.IO.SeekOrigin.Begin);
            writer.Write((int)writer.BaseStream.Length);
            writer.Seek(0, System.IO.SeekOrigin.End);
        }

        private void WriteAttribute(EndianBinaryWriter writer, VertexAttributes attribute)
        {
            writer.Write((int)attribute);

            // Component count. Normals use 0, all others use 1
            if (attribute == VertexAttributes.Normal)
                writer.Write((int)0);
            else
                writer.Write((int)1);

            switch (attribute)
            {
                case VertexAttributes.Position:
                    writer.Write((int)PositionType);
                    if (PositionType == DataTypes.S16)
                        writer.Write((byte)PositionFractionalBitVal);
                    else
                        writer.Write((byte)0);
                    break;
                case VertexAttributes.Normal:
                    writer.Write((int)NormalType);
                    if (NormalType == DataTypes.S16)
                        writer.Write((byte)NormalFractionalBitVal);
                    else
                        writer.Write((byte)0);
                    break;
                case VertexAttributes.Tex0:
                case VertexAttributes.Tex1:
                case VertexAttributes.Tex2:
                case VertexAttributes.Tex3:
                case VertexAttributes.Tex4:
                case VertexAttributes.Tex5:
                case VertexAttributes.Tex6:
                case VertexAttributes.Tex7:
                    writer.Write((int)UVType);
                    if (UVType == DataTypes.S16)
                        writer.Write((byte)UVFractionalBitVal);
                    else
                        writer.Write((byte)0);
                    break;
                case VertexAttributes.Color0:
                case VertexAttributes.Color1:
                    writer.Write((int)ColorType);
                    writer.Write((byte)0);
                    break;
                case VertexAttributes.NullAttr:
                    writer.Write((int)0);
                    writer.Write((byte)0);
                    break;
            }

            writer.Write((byte)0xFF); // Padding part 1
            writer.Write((ushort)0xFFFF); // Padding part 2
        }

        private void WriteDataBank(EndianBinaryWriter writer, VertexAttributes attribute)
        {
            switch (attribute)
            {
                case VertexAttributes.Position:
                    WriteVec3(writer, Positions, PositionType, PositionFractionalBitVal);
                    break;
                case VertexAttributes.Normal:
                    WriteVec3(writer, Normals, NormalType, NormalFractionalBitVal);
                    break;
                case VertexAttributes.Tex0:
                case VertexAttributes.Tex1:
                case VertexAttributes.Tex2:
                case VertexAttributes.Tex3:
                case VertexAttributes.Tex4:
                case VertexAttributes.Tex5:
                case VertexAttributes.Tex6:
                case VertexAttributes.Tex7:
                    int uvListIndex = (int)attribute - 13;
                    WriteVec2(writer, UVData[uvListIndex], UVType, UVFractionalBitVal);
                    break;
                case VertexAttributes.Color0:
                case VertexAttributes.Color1:
                    int colorListIndex = (int)attribute - 11;
                    WriteColor(writer, ColorData[colorListIndex], ColorType);
                    break;
            }
        }

        private void WriteVec3(EndianBinaryWriter writer, List<Vector3> list, DataTypes type, int fractionalVal)
        {
            // For each vector in the list...
            foreach (Vector3 vec in list)
            {
                // For each component of the vector...
                for (int i = 0; i < 3; i++)
                {
                    switch (type)
                    {
                        case DataTypes.F32:
                            writer.Write(vec[i]);
                            break;
                        case DataTypes.S16:
                            short s16Value = (short)(vec[i] * (1 << fractionalVal));
                            writer.Write(s16Value);
                            break;
                    }
                }
            }
        }

        private void WriteVec2(EndianBinaryWriter writer, List<Vector2> list, DataTypes type, int fractionalVal)
        {
            foreach (Vector2 vec in list)
            {
                for (int i = 0; i < 2; i++)
                {
                    switch (type)
                    {
                        case DataTypes.F32:
                            writer.Write(vec[i]);
                            break;
                        case DataTypes.S16:
                            short s16Value = (short)(vec[i] * (1 << fractionalVal));
                            writer.Write(s16Value);
                            break;
                    }
                }
            }
        }

        private void WriteColor(EndianBinaryWriter writer, List<Vector3> list, ColorDataTypes type)
        {
            foreach (Vector3 vec in list)
            {
                for (int i = 0; i < 3; i++)
                {
                    switch (type)
                    {
                        case ColorDataTypes.RGB8:
                            float clampedVal = (float)Math.Max(0.0, Math.Min(1.0, vec[i]));
                            byte colorByte = (byte)Math.Floor(clampedVal == 1.0 ? 255 : clampedVal * 256.0);
                            writer.Write(colorByte);
                            break;
                    }
                }

                if ((int)type > 2)
                {
                    switch (type)
                    {
                        case ColorDataTypes.RGBA8:
                            writer.Write((byte)0xFF);
                            break;
                    }
                }
            }
        }

        private void WriteDataBankOffset(EndianBinaryWriter writer, VertexAttributes attribute)
        {
            int bankOffset = 0;

            #region Offset switch
            switch (attribute)
            {
                case VertexAttributes.Position:
                    bankOffset = 0;
                    break;
                case VertexAttributes.Normal:
                    bankOffset = 1;
                    break;
                case VertexAttributes.Color0:
                    bankOffset = 3;
                    break;
                case VertexAttributes.Color1:
                    bankOffset = 4;
                    break;
                case VertexAttributes.Tex0:
                    bankOffset = 5;
                    break;
                case VertexAttributes.Tex1:
                    bankOffset = 6;
                    break;
                case VertexAttributes.Tex2:
                    bankOffset = 7;
                    break;
                case VertexAttributes.Tex3:
                    bankOffset = 8;
                    break;
                case VertexAttributes.Tex4:
                    bankOffset = 9;
                    break;
                case VertexAttributes.Tex5:
                    bankOffset = 10;
                    break;
                case VertexAttributes.Tex6:
                    bankOffset = 11;
                    break;
                case VertexAttributes.Tex7:
                    bankOffset = 12;
                    break;
            }
            #endregion

            writer.Seek(0xC + (4 * bankOffset), System.IO.SeekOrigin.Begin);
            writer.Write((int)writer.BaseStream.Length);
            writer.Seek(0, System.IO.SeekOrigin.End);
        }

        private void GetActiveVertAttributes(Grendgine_Collada_Mesh mesh)
        {
            foreach (Grendgine_Collada_Triangles tri in mesh.Triangles)
            {
                foreach (Grendgine_Collada_Input_Shared input in tri.Input)
                {
                    switch (input.Semantic)
                    {
                        case Grendgine_Collada_Input_Semantic.VERTEX:
                            if (!activeAttributes.Keys.Contains(VertexAttributes.Position))
                                activeAttributes.Add(VertexAttributes.Position, input.source.Remove(0, 1));
                            break;
                        case Grendgine_Collada_Input_Semantic.NORMAL:
                            if (!activeAttributes.Keys.Contains(VertexAttributes.Normal))
                                activeAttributes.Add(VertexAttributes.Normal, input.source.Remove(0, 1));
                            break;
                        case Grendgine_Collada_Input_Semantic.COLOR:
                            if (input.Set > 2)
                            {
                                Console.WriteLine(string.Format("BMD only supports two Color attributes. Skipping color attribute [0].", input.Set));
                                break;
                            }
                            if (!activeAttributes.Keys.Contains(VertexAttributes.Color0 + input.Set))
                                activeAttributes.Add(VertexAttributes.Color0 + input.Set, input.source.Remove(0, 1));
                            break;
                        case Grendgine_Collada_Input_Semantic.TEXCOORD:
                            if (input.Set > 8)
                            {
                                Console.Write(string.Format("BMD only supports 8 UV attributes. Skipping UV attribute [0].", input.Set));
                                break;
                            }
                            if (!activeAttributes.Keys.Contains(VertexAttributes.Tex0 + input.Set))
                                activeAttributes.Add(VertexAttributes.Tex0 + input.Set, input.source.Remove(0, 1));
                            break;
                    }
                }
            }
        }

        private void FillAttributeLists(Grendgine_Collada_Mesh mesh)
        {
            foreach (KeyValuePair<VertexAttributes, string> val in activeAttributes)
            {
                if (val.Key == VertexAttributes.Position)
                {
                    // This is a hack. It needs to be fixed because it is a hack.
                    // Should probably get the vertex positions directly from the vertex
                    // class in the mesh.
                    Positions = GetVertexData<Vector3>(mesh.Source.First(x => x.ID.Contains("POSITION")));
                }
                else if (val.Key == VertexAttributes.Normal)
                {
                    Normals = GetVertexData<Vector3>(mesh.Source.First(x => x.ID.Contains(val.Value)));
                }
                else if (val.Key >= VertexAttributes.Tex0 && val.Key <= VertexAttributes.Tex7)
                {
                    int uvIndex = (int)val.Key - 13; // 13 is the value of the Tex0 attribute
                    UVData[uvIndex] = GetVertexData<Vector2>(mesh.Source.First(x => x.ID.Contains(val.Value)));
                }
                else if (val.Key == VertexAttributes.Color0 || val.Key == VertexAttributes.Color1)
                {
                    int colorIndex = (int)val.Key - 11; // 11 is the value of the Color0 attribute
                    ColorData[colorIndex] = GetVertexData<Vector3>(mesh.Source.First(x => x.ID.Contains(val.Value)));
                }
            }
        }

        private int GetFractionalVec3(List<Vector3> list)
        {
            int frac = 0;
            float highest = float.MinValue;

            foreach (Vector3 vec in list)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (vec[i] > highest)
                        highest = vec[i];
                }
            }

            if (highest < 2048.0f)
                frac = 4;
            else if (highest >= 2048.0f && highest < 4069.0f)
                frac = 3;
            else if (highest >= 4069.0f && highest < 8192.0f)
                frac = 2;
            else if (highest >= 8192.0f)
                frac = 1;

            return frac;
        }

        private List<T> GetVertexData<T>(Grendgine_Collada_Source src) where T : new()
        {
            Grendgine_Collada_Accessor srcInfo = src.Technique_Common.Accessor;

            int componentCount = srcInfo.Param.Length;
            int dataCount = (int)srcInfo.Count;

            List<T> values = new List<T>(dataCount);
            int globalCompOffset = 0;

            List<float> componentList = new List<float>();
            string listString = src.Float_Array.Value_As_String;
            listString = listString.Replace('\n', ' ').Trim();

            componentList.AddRange(Grendgine_Collada_Parse_Utils.String_To_Float(listString));

            for (int i = 0; i < dataCount; i++)
            {
                dynamic value = new T();

                for (int comp = 0; comp < componentCount; comp++)
                {
                    value[comp] = componentList[globalCompOffset++];
                }

                values.Add(value);
            }

            return values;
        }
    }
}
