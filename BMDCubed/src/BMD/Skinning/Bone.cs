using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using grendgine_collada;
using GameFormatReader.Common;

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

        public Bone(Grendgine_Collada_Node node)
        {
            Children = new List<Bone>();
            InverseBindMatrix = Matrix4.Identity;
            Bounds = new BoundingBox();

            Name = node.Name;
            Unknown4 = 255;

            float[] nodeTransform = node.Matrix[0].Value();
            Matrix4 transform = new Matrix4(nodeTransform[0], nodeTransform[1], nodeTransform[2], nodeTransform[3],
                                            nodeTransform[4], nodeTransform[5], nodeTransform[6], nodeTransform[7],
                                            nodeTransform[8], nodeTransform[9], nodeTransform[10], nodeTransform[11],
                                            nodeTransform[12], nodeTransform[13], nodeTransform[14], nodeTransform[15]);

            Scale = transform.ExtractScale();
            Translation = new Vector3(transform.Column3[0], transform.Column3[1], transform.Column3[2]);
            Rotation = transform.ExtractRotation();

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
                    InverseBindMatrix = mat.Value;
            }

            foreach (Bone bone in Children)
                bone.GetInverseBindMatrixRecursive(matrixList);
        }

        public void FlattenHierarchy(List<Bone> boneList)
        {
            boneList.Add(this);

            foreach (Bone bone in Children)
                bone.FlattenHierarchy(boneList);
        }

        public void WriteInvMatrix(EndianBinaryWriter writer)
        {
            // BMD stores the matrices as 3x4, so we discard the last row
            writer.Write(InverseBindMatrix.M11);
            writer.Write(InverseBindMatrix.M12);
            writer.Write(InverseBindMatrix.M13);
            writer.Write(InverseBindMatrix.M14);

            writer.Write(InverseBindMatrix.M21);
            writer.Write(InverseBindMatrix.M22);
            writer.Write(InverseBindMatrix.M23);
            writer.Write(InverseBindMatrix.M24);

            writer.Write(InverseBindMatrix.M31);
            writer.Write(InverseBindMatrix.M32);
            writer.Write(InverseBindMatrix.M33);
            writer.Write(InverseBindMatrix.M34);
        }

        public void WriteBone(EndianBinaryWriter writer)
        {
            writer.Write(Unknown1);
            writer.Write(Unknown2);
            writer.Write(Unknown3);
            writer.Write(Unknown4);

            writer.Write(Scale.X);
            writer.Write(Scale.Y);
            writer.Write(Scale.Z);

            Vector3 euler = Rotation.Xyz;
            writer.Write((short)((Util.RadsToDegrees(euler.X) * 32767.0f) / 180.0f));
            writer.Write((short)((Util.RadsToDegrees(euler.Y) * 32767.0f) / 180.0f));
            writer.Write((short)((Util.RadsToDegrees(euler.Z) * 32767.0f) / 180.0f));
            writer.Write((short)-1);

            writer.Write(Translation.X);
            writer.Write(Translation.Y);
            writer.Write(Translation.Z);

            Bounds.WriteBoundingBox(writer);
        }
    }
}
