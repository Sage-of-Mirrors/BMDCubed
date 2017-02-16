using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using grendgine_collada;
using GameFormatReader.Common;
using BMDCubed.Materials;
using BMDCubed.src.BMD.Geometry;

namespace BMDCubed.src.BMD.Skinning
{
    class Bone
    {
        // What these do are currently not known
        public byte Unknown1;
        public byte Unknown2;
        public byte Unknown3;
        public byte Unknown4;

        public Vector3 Scale;
        public Vector3 Translation;
        public Quaternion Rotation;
        public BoundingBox Bounds;

        public string Name;
        public Matrix4 InverseBindMatrix;
        public List<Bone> Children;
        public List<Material> Materials;
        public Matrix4 Transform;
        public Matrix4 transform;

        public Bone Parent;

        public Bone(Grendgine_Collada_Node node)
        {
            Children = new List<Bone>();
            InverseBindMatrix = Matrix4.Identity;
            Bounds = new BoundingBox();
            Materials = new List<Material>();

            Name = node.Name;
            Unknown4 = 255;

            float[] nodeTransform = node.Matrix[0].Value();
            
            transform = new Matrix4(nodeTransform[0], nodeTransform[4], nodeTransform[8], nodeTransform[12],
                                    nodeTransform[1], nodeTransform[5], nodeTransform[9], nodeTransform[13],
                                    nodeTransform[2], nodeTransform[6], nodeTransform[10], nodeTransform[14],
                                    nodeTransform[3], nodeTransform[7], nodeTransform[11], nodeTransform[15]);

            /*
            transform = new Matrix4(nodeTransform[0], nodeTransform[1], nodeTransform[2], nodeTransform[3],
                                    nodeTransform[4], nodeTransform[5], nodeTransform[6], nodeTransform[7],
                                    nodeTransform[8], nodeTransform[9], nodeTransform[10], nodeTransform[11],
                                    nodeTransform[12], nodeTransform[13], nodeTransform[14], nodeTransform[15]);
            */

            Scale = transform.ExtractScale();
            Rotation = transform.ExtractRotation();
            Translation = transform.ExtractTranslation();

            if (node.node == null)
                return;

            foreach (Grendgine_Collada_Node childNode in node.node)
            {
                Bone child = new Bone(childNode);
                Children.Add(child);
            }
        }

        public void GetInverseBindMatrixRecursive(Dictionary<string, Matrix4> matrixList)
        {
            foreach (KeyValuePair<string, Matrix4> mat in matrixList)
            {
                if (mat.Key == Name)
                {
                    InverseBindMatrix = mat.Value;
                    //InverseBindMatrix.Transpose();
                }
            }

            foreach (Bone bone in Children)
                bone.GetInverseBindMatrixRecursive(matrixList);
        }

        public void FlattenHierarchy(List<Bone> boneList, Bone parent)
        {
            boneList.Add(this);
            Parent = parent;

            foreach (Bone bone in Children)
                bone.FlattenHierarchy(boneList, this);
        }

        public void WriteBone(EndianBinaryWriter writer)
        {
            writer.Write(Unknown1);
            writer.Write((byte)0);
            writer.Write(Unknown3);
            writer.Write(Unknown4);

            writer.Write(Scale.X);
            writer.Write(Scale.Y);
            writer.Write(Scale.Z);

            Vector3 euler = Util.ToEulerianAngles(Rotation);
            Vector3 degrees = new Vector3(Util.RadsToDegrees(euler.X),
                                          Util.RadsToDegrees(euler.Y),
                                          Util.RadsToDegrees(euler.Z));
            Quaternion quat = new Quaternion(euler);
            writer.Write((short)((Util.RadsToDegrees(euler.X) * 32767.0f) / 180.0f));
            writer.Write((short)((Util.RadsToDegrees(euler.Y) * 32767.0f) / 180.0f));
            writer.Write((short)((Util.RadsToDegrees(euler.Z) * 32767.0f) / 180.0f));
            writer.Write((short)-1);

            writer.Write(Translation.X);
            writer.Write(Translation.Y);
            writer.Write(Translation.Z);

            Bounds.WriteBoundingBox(writer);
        }

        public void WriteScenegraphRecursive(EndianBinaryWriter writer, List<Bone> bones, List<Batch> batches, List<Material> materials)
        {
            writer.Write((short)0x10);
            writer.Write((short)bones.IndexOf(this));

            writer.Write((short)1);
            writer.Write((short)0);

            
            foreach (Material mat in Materials)
            {
                writer.Write((short)0x11);
                writer.Write((short)materials.IndexOf(mat));

                writer.Write((short)1);
                writer.Write((short)0);

                writer.Write((short)0x12);
                writer.Write((short)batches.IndexOf(mat.MatBatch));

                writer.Write((short)1);
                writer.Write((short)0);
            }

            foreach (Bone bone in Children)
                bone.WriteScenegraphRecursive(writer, bones, batches, materials);

            for (int i = 0; i < Materials.Count * 2; i++)
            {
                writer.Write((short)2);
                writer.Write((short)0);
            }

            writer.Write((short)2);
            writer.Write((short)0);
        }
    }
}
