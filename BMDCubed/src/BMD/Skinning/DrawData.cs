using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;
using grendgine_collada;
using OpenTK;

namespace BMDCubed.src.BMD.Skinning
{
    class DrawData
    {
        public List<Weight> AllWeights; // This contains all the weights from the skin, including duplicates
        public List<Weight> AllDrw1Weights; // This contains the full weights and the partial weights, in that order, without duplicates
        public List<Matrix4> InverseBindMatrices;

        List<Weight> fullWeightList; // List of full weights, no duplicates
        List<Weight> partialWeightList; // List of partial weights, no duplicates

        public DrawData(Grendgine_Collada_Skin skin, List<Bone> flat, List<Bone> geom)
        {
            AllDrw1Weights = new List<Weight>();
            AllWeights = new List<Weight>();
            InverseBindMatrices = new List<Matrix4>();

            fullWeightList = new List<Weight>();
            partialWeightList = new List<Weight>();

            getWeights(skin, flat, geom);

            // Sort weights between full weights (with only 1 bone)
            // and partial weights (with more than 1 bone).
            // We'll also cull duplicates.
            foreach (Weight weight in AllWeights)
            {
                if (weight.BoneWeights.Count == 1 && !fullWeightList.Contains(weight))
                    fullWeightList.Add(weight);
                else if (weight.BoneWeights.Count > 1 && !partialWeightList.Contains(weight))
                    partialWeightList.Add(weight);
            }

            // Create the full list of weights. Full weights first, then partial
            AllDrw1Weights.AddRange(fullWeightList.ToArray());
            AllDrw1Weights.AddRange(partialWeightList.ToArray());

            // We'll rip the inverse bind matrices from the hierarchy so we can
            // use them to write the EVP1 chunk later
            foreach (Bone bone in flat)
                InverseBindMatrices.Add(bone.InverseBindMatrix);
        }

        /// <summary>
        /// Populates the AllWeights list with the weights from the mesh.
        /// </summary>
        /// <param name="skin">Skin to pull weights from</param>
        /// <param name="geom">List of bones that contain geometry</param>
        /// <param name="flat">Flattened hierarchy of all bones in the mesh</param>
        private void getWeights(Grendgine_Collada_Skin skin, List<Bone> flat, List<Bone> geom)
        {
            int[] bonePairs = Grendgine_Collada_Parse_Utils.String_To_Int(skin.Vertex_Weights.V.Value_As_String.Replace('\n', ' ').Trim());
            int[] boneWeightCounts = Grendgine_Collada_Parse_Utils.String_To_Int(skin.Vertex_Weights.VCount.Value_As_String.Replace('\n', ' ').Trim());
            float[] weightData = getWeightData(skin);

            // We'll fill the main list of all the weights in the mesh, regardless of full/partial weight
            // or duplicates.
            int offset = 0;
            for (int i = 0; i < boneWeightCounts.Length; i++)
            {
                int numWeights = boneWeightCounts[i];
                Weight weight = new Weight();

                for (int j = 0; j < numWeights; j++)
                {
                    Bone bone = geom[bonePairs[offset]];
                    offset++;

                    float weightVal = weightData[bonePairs[offset]];
                    offset++;

                    weight.AddBoneWeight((short)flat.IndexOf(bone), weightVal);
                }

                AllWeights.Add(weight);
            }
        }

        /// <summary>
        /// Gets the float data representing the actual weights on a vertex.
        /// </summary>
        /// <param name="skin">Skin to pull weight data from</param>
        /// <returns>Float array containing the actual weight data</returns>
        private float[] getWeightData(Grendgine_Collada_Skin skin)
        {
            float[] floatArray = new float[32];

            // Get the name of the Source object containing the weight data
            string weightSource = "";
            for (int i = 0; i < skin.Vertex_Weights.Input.Length; i++)
            {
                if (skin.Vertex_Weights.Input[i].Semantic == Grendgine_Collada_Input_Semantic.WEIGHT)
                {
                    weightSource = skin.Vertex_Weights.Input[i].source.Remove(0, 1);
                    break;
                }
            }

            // We didn't find weight data. PANIC!!!!!
            if (weightSource == "")
                throw new FormatException("No weight data found!");

            // Run through the Source objects to find the one that has the weight data
            foreach (Grendgine_Collada_Source src in skin.Source)
            {
                if (src.ID == weightSource)
                {
                    floatArray = Grendgine_Collada_Parse_Utils.String_To_Float(src.Float_Array.Value_As_String.Replace('\n', ' ').Trim());
                }
            }

            return floatArray;
        }

        /// <summary>
        /// Outputs an EVP1 chunk to the specified stream.
        /// </summary>
        /// <param name="writer">Stream to write EVP1 to</param>
        public void WriteEVP1(EndianBinaryWriter writer)
        {
            // Header
            writer.Write("EVP1".ToCharArray()); // FourCC, "EVP1"
            writer.Write((int)0); // Placeholder for size
            writer.Write((short)partialWeightList.Count); // Number of weights
            writer.Write((short)-1); // Padding for header

            writer.Write((int)0x1C); // Offset to index count section, always 0x1C
            writer.Write((int)0); // Placeholder for index data offset
            writer.Write((int)0); // Placeholder for weight data offset
            writer.Write((int)0); // Placeholder for inverse bind matrix offset

            // Write index count table
            for (int i = 0; i < partialWeightList.Count; i++)
                writer.Write((byte)(partialWeightList[i].BoneIndexes.Count));

            // Write offset to bone index table
            Util.WriteOffset(writer, 0x10);

            // Write bone index table
            for (int i = 0; i < partialWeightList.Count; i++)
            {
                for (int j = 0; j < partialWeightList[i].BoneIndexes.Count; j++)
                    writer.Write((ushort)(partialWeightList[i].BoneIndexes[j]));
            }

            Util.PadStreamWithString(writer, 32);

            // Write offset to weight table
            Util.WriteOffset(writer, 0x14);

            // Write weight table
            foreach (Weight weight in partialWeightList)
            {
                for (int i = 0; i < weight.BoneWeights.Count; i++)
                    writer.Write(weight.BoneWeights[i]);
            }

            Util.PadStreamWithString(writer, 32);

            // Write offset to inverse matrix table
            Util.WriteOffset(writer, 0x18);

            // Write inverse bind matrix table
            foreach (Matrix4 mat in InverseBindMatrices)
            {
                Vector3 trans = mat.ExtractTranslation();
                Vector3 scale = mat.ExtractScale();
                Quaternion rot = mat.ExtractRotation();

                Matrix3x4 test = Matrix3x4.CreateScale(scale) *
                                 Matrix3x4.CreateFromQuaternion(rot) *
                                 Matrix3x4.CreateTranslation(trans);
                //Matrix3x4 mat3 = Matrix3x4.Mult(InverseBindMatrices, ident);


                // BMD stores the matrices as 3x4, so we discard the last row
                /*
                writer.Write(test.M11);
                writer.Write(test.M12);
                writer.Write(test.M13);
                writer.Write(test.M14);

                writer.Write(test.M21);
                writer.Write(test.M22);
                writer.Write(test.M23);
                writer.Write(test.M24);

                writer.Write(test.M31);
                writer.Write(test.M32);
                writer.Write(test.M33);
                writer.Write(test.M34);
                */

                
                Vector4 Row1 = mat.Column0;
                Vector4 Row2 = mat.Column1;
                Vector4 Row3 = mat.Column2;

                writer.Write(Row1.X);
                writer.Write(Row1.Y);
                writer.Write(Row1.Z);
                writer.Write(Row1.W);

                writer.Write(Row2.X);
                writer.Write(Row2.Y);
                writer.Write(Row2.Z);
                writer.Write(Row2.W);

                writer.Write(Row3.X);
                writer.Write(Row3.Y);
                writer.Write(Row3.Z);
                writer.Write(Row3.W);
                
            }

            Util.PadStreamWithString(writer, 32);

            // Write chunk size
            Util.WriteOffset(writer, 4);
        }

        /// <summary>
        /// Outputs a DRW1 section to the specified stream.
        /// </summary>
        /// <param name="writer">Stream to write DRW1 to</param>
        public void WriteDRW1(EndianBinaryWriter writer)
        {
            // Header
            writer.Write("DRW1".ToCharArray()); // FourCC, "DRW1"
            writer.Write(0); // Placeholder for chunk size
            writer.Write((short)(AllDrw1Weights.Count)); // Number of elements
            writer.Write((ushort)0xFFFF); // Padding
            writer.Write(0x14); // Offset to bool table, it's constant
            writer.Write(0); // Placeholder for index table offset

            // Write false bools for full weights
            for (int i = 0; i < fullWeightList.Count; i++)
                writer.Write((byte)0);

            // Write true bools for partial weights
            for (int i = 0; i < partialWeightList.Count * 2; i++)
                writer.Write((byte)1);

            // Write index table offset
            Util.WriteOffset(writer, 0x10);

            // Write bone indexes for full weights
            for (int i = 0; i < fullWeightList.Count; i++)
                writer.Write((short)fullWeightList[i].BoneIndexes[0]);

            // Write EVP1 indexes for partial weights
            for (int i = 0; i < partialWeightList.Count; i++)
                writer.Write((short)i);

            // Write EVP1 indexes for partial weights
            for (int i = 0; i < partialWeightList.Count; i++)
                writer.Write((short)i);

            Util.PadStreamWithString(writer, 32);

            // Write chunk size
            Util.WriteOffset(writer, 4);
        }
    }
}
