using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using grendgine_collada;
using OpenTK;
using GameFormatReader.Common;

namespace BMDCubed.src.BMD.Skinning
{
    class SkeletonData
    {
        /// <summary>
        /// The root of the skeleton's hierarchy.
        /// Also doubles as the root of the model's scenegraph.
        /// </summary>
        public Bone SkeletonRoot;
        /// <summary>
        /// The skeleton hierarchy, flattened into a list.
        /// </summary>
        public List<Bone> FlatHierarchy;
        /// <summary>
        /// Contains the bones with geometry assigned to them.
        /// Derived from boneNameList.
        /// </summary>
        public List<Bone> BonesWithGeometry;

        List<string> boneNameList;
        List<Matrix4> inverseBindMatrices;
        List<float> weights;

        List<int> vertexBoneIndexPairs;
        List<int> vertexWeightCounts;

        List<Weight> VertexWeights;
        List<Weight> MultiBoneWeights;

        public DrawData drw1Data;

        public SkeletonData(Grendgine_Collada scene)
        {
            FlatHierarchy = new List<Bone>();
            BonesWithGeometry = new List<Bone>();

            SkeletonRoot = new Bone(GetSkeletonFromVisualScene(scene));

            Grendgine_Collada_Skin skin = GetSkinningInfo(scene);

            // Get bone names and inverse bind matrices
            if (skin.Joints != null)
            {
                foreach (Grendgine_Collada_Input_Unshared input in skin.Joints.Input)
                {
                    if (input.Semantic == Grendgine_Collada_Input_Semantic.JOINT)
                    {
                        GetJointNamesFromSkin(input.source.Remove(0, 1), skin);
                    }
                    else if (input.Semantic == Grendgine_Collada_Input_Semantic.INV_BIND_MATRIX)
                    {
                        GetInvBindMatricesFromSkin(input.source.Remove(0, 1), skin);
                    }
                }
            }

            Dictionary<string, Matrix4> matrixDict = new Dictionary<string, Matrix4>();

            for (int i = 0; i < boneNameList.Count; i++)
            {
                matrixDict.Add(boneNameList[i], inverseBindMatrices[i]);
            }

            //Assign inverse bind matrices to the bones that need one
            SkeletonRoot.GetInverseBindMatrixRecursive(matrixDict);

            // Flatten hierarchy for easy access later
            SkeletonRoot.FlattenHierarchy(FlatHierarchy);

            // We'll make another list that contains just the bones with geometry from boneNameList, in order,
            // so that the vertex weights and bone assignments can be used correctly.
            MakeBoneGeometryList();
        }

        private Grendgine_Collada_Node GetSkeletonFromVisualScene(Grendgine_Collada scene)
        {
            // We need to get the hierarchy of the bone structure.
            // DAE stores it in the visual scene, but sometimes it's hidden under layers of
            // other nodes. To get around that, we're going to check every node in the visual scene
            // for if A) it's a "JOINT" node and B) it has children, ie the node property isn't null.
            // The check will be recursive, working on every node in the hierarchy until we find what
            // *should* be the skeleton's root.

            Grendgine_Collada_Node root = null;

            foreach (Grendgine_Collada_Visual_Scene visScene in scene.Library_Visual_Scene.Visual_Scene)
            {
                foreach (Grendgine_Collada_Node node in visScene.Node)
                {
                    root = GetSkeletonFromNodeRecursive(node);

                    if (root != null)
                        break;
                }
            }

            return root;
        }

        private Grendgine_Collada_Node GetSkeletonFromNodeRecursive(Grendgine_Collada_Node node)
        {
            Grendgine_Collada_Node skelRoot = null;

            if (node.Type == Grendgine_Collada_Node_Type.JOINT && node.node != null)
            {
                return node;
            }
            else if (node.node != null)
            {
                foreach (Grendgine_Collada_Node child in node.node)
                {
                    skelRoot = GetSkeletonFromNodeRecursive(child);

                    if (skelRoot != null)
                        break;
                }
            }

            return skelRoot;
        }

        private Grendgine_Collada_Skin GetSkinningInfo(Grendgine_Collada scene)
        {
            Grendgine_Collada_Skin skinning = null;

            foreach (Grendgine_Collada_Controller con in scene.Library_Controllers.Controller)
            {
                if (con.Skin != null)
                    skinning = con.Skin;
            }

            return skinning;
        }

        private void GetJointNamesFromSkin(string source, Grendgine_Collada_Skin skin)
        {
            foreach (Grendgine_Collada_Source src in skin.Source)
            {
                if (src.ID == source)
                {
                    boneNameList.AddRange(src.Name_Array.Value());

                    for (int i = 0; i < boneNameList.Count; i++)
                    {
                        if (boneNameList[i] == "\n")
                        {
                            boneNameList.RemoveAt(i);
                            i--;
                        }
                        else if (boneNameList[i].Contains("\n"))
                        {
                            string[] tempEndLine = boneNameList[i].Split('\n');

                            boneNameList.RemoveAt(i);

                            for (int j = 0; j < tempEndLine.Length; j++)
                            {
                                boneNameList.Insert(i, tempEndLine[j]);
                                i++;
                            }

                            i--;
                        }
                    }
                }
            }
        }

        private void GetInvBindMatricesFromSkin(string source, Grendgine_Collada_Skin skin)
        {
            foreach (Grendgine_Collada_Source src in skin.Source)
            {
                if (src.ID == source)
                {
                    string test = src.Float_Array.Value_As_String.Replace('\n', ' ').Trim();
                    
                    float[] matrixSrc = grendgine_collada.Grendgine_Collada_Parse_Utils.String_To_Float(test);

                    for (int i = 0; i < matrixSrc.Length; i += 16)
                    {
                        Matrix4 invBind = new Matrix4(matrixSrc[i + 0], matrixSrc[i + 1], matrixSrc[i + 2], matrixSrc[i + 3],
                                                      matrixSrc[i + 4], matrixSrc[i + 5], matrixSrc[i + 6], matrixSrc[i + 7],
                                                      matrixSrc[i + 8], matrixSrc[i + 9], matrixSrc[i + 10], matrixSrc[i + 11],
                                                      matrixSrc[i + 12], matrixSrc[i + 13], matrixSrc[i + 14], matrixSrc[i + 15]);

                        inverseBindMatrices.Add(invBind);
                    }
                }
            }
        }

        private void WriteBoneStringTable(EndianBinaryWriter writer)
        {
            List<char> stringBank = new List<char>();

            writer.Write((short)FlatHierarchy.Count);
            writer.Write((short)-1);

            foreach (Bone bone in FlatHierarchy)
            {
                writer.Write(Util.HashName(bone.Name));
                writer.Write((short)(4 + (FlatHierarchy.Count * 4) + stringBank.Count));

                stringBank.AddRange(bone.Name.ToCharArray());
                stringBank.Add('\0');
            }

            writer.Write(stringBank.ToArray());
        }

        private void MakeBoneGeometryList()
        {
            foreach (string st in boneNameList)
            {
                foreach (Bone bone in FlatHierarchy)
                {
                    if (bone.Name == st)
                    {
                        BonesWithGeometry.Add(bone);
                        continue;
                    }
                }
            }
        }

        public void WriteJNT1(EndianBinaryWriter writer)
        {
            writer.Write("JNT1".ToCharArray());
            writer.Write(0); // Placeholder for chunk size
            writer.Write((short)FlatHierarchy.Count); // Number of joints
            writer.Write((short)-1); // Padding
            writer.Write(0x18); // Offset to bone data. Always 0x18
            writer.Write(0);
            writer.Write(0);

            // Write bone data
            foreach (Bone bone in FlatHierarchy)
                bone.WriteBone(writer);

            Util.WriteOffset(writer, 0x10); // Write index offset

            // Write index data
            for (int i = 0; i < FlatHierarchy.Count; i++)
                writer.Write((short)i);

            //Util.PadStreamWithString(writer, 32);
            Util.WriteOffset(writer, 0x14); // Write string table offset

            // Write string table
            WriteBoneStringTable(writer);

            Util.PadStreamWithString(writer, 32);
            Util.WriteOffset(writer, 4);
        }

        public void AssignBoneBoundingBoxes(List<Vector3> verts)
        {
            List<Vector3>[] boneVerts = new List<Vector3>[FlatHierarchy.Count];

            for (int i = 0; i < VertexWeights.Count; i++)
            {
                foreach (int inte in VertexWeights[i].BoneIndexes)
                {
                    if (boneVerts[inte] == null)
                        boneVerts[inte] = new List<Vector3>();

                    boneVerts[inte].Add(verts[i]);
                }
            }

            for (int i = 0; i < FlatHierarchy.Count; i++)
            {
                if (boneVerts[i] == null)
                    continue;

                FlatHierarchy[i].Bounds = new BoundingBox(boneVerts[i]);
            }
        }
    }
}
