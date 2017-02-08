using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using GameFormatReader.Common;

namespace BMDCubed.Materials
{
    class TexMatrix
    {
        public byte Projection;
        public byte Type;
        public float CenterS;
        public float CenterT;
        public float Unknown0;
        public float ScaleS;
        public float ScaleT;
        public float Rotation;
        public float TranslateS;
        public float TranslateT;
        public Matrix4 PreMatrix;

        public TexMatrix()
        {
            Projection = 1;
            CenterS = .5f;
            CenterT = .5f;

            ScaleS = 1.0f;
            ScaleT = 1.0f;

            Unknown0 = .5f;

            PreMatrix = Matrix4.Identity;
        }

        public void Write(EndianBinaryWriter writer)
        {
            writer.Write(Projection);
            writer.Write(Type);

            // Padding
            writer.Write((short)-1);

            writer.Write(CenterS);
            writer.Write(CenterT);
            writer.Write(Unknown0);
            writer.Write(ScaleS);
            writer.Write(ScaleT);
            writer.Write((short)(Rotation));

            // Padding
            writer.Write((short)-1);

            writer.Write(TranslateS);
            writer.Write(TranslateT);

            writer.Write(PreMatrix.M11);
            writer.Write(PreMatrix.M12);
            writer.Write(PreMatrix.M13);
            writer.Write(PreMatrix.M14);
            writer.Write(PreMatrix.M21);
            writer.Write(PreMatrix.M22);
            writer.Write(PreMatrix.M23);
            writer.Write(PreMatrix.M24);
            writer.Write(PreMatrix.M31);
            writer.Write(PreMatrix.M32);
            writer.Write(PreMatrix.M33);
            writer.Write(PreMatrix.M34);
            writer.Write(PreMatrix.M41);
            writer.Write(PreMatrix.M42);
            writer.Write(PreMatrix.M43);
            writer.Write(PreMatrix.M44);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(TexMatrix))
                return Compare((TexMatrix)obj);
            else
                return false;
        }

        private bool Compare(TexMatrix obj)
        {
            if ((Projection == obj.Projection) && (Type == obj.Type) && (CenterS == obj.CenterS) && (CenterT == obj.CenterT)
                && (Unknown0 == obj.Unknown0) && (ScaleS == obj.ScaleS) && (ScaleT == obj.ScaleT) && (Rotation == obj.Rotation)
                && (TranslateS == obj.TranslateS) && (TranslateT == obj.TranslateT) && (PreMatrix == obj.PreMatrix))
                return true;
            else
                return false;
        }

        public static bool operator ==(TexMatrix left, TexMatrix right)
        {
            if (System.Object.ReferenceEquals(left, right))
                return true;

            if (((object)left == null) || ((object)right == null))
                return false;

            if ((left.Projection == right.Projection) && (left.Type == right.Type) && (left.CenterS == right.CenterS) && (left.CenterT == right.CenterT)
                && (left.Unknown0 == right.Unknown0) && (left.ScaleS == right.ScaleS) && (left.ScaleT == right.ScaleT) && (left.Rotation == right.Rotation)
                && (left.TranslateS == right.TranslateS) && (left.TranslateT == right.TranslateT) && (left.PreMatrix == right.PreMatrix))
                return true;
            else
                return false;
        }

        public static bool operator !=(TexMatrix left, TexMatrix right)
        {
            if (System.Object.ReferenceEquals(left, right))
                return false;

            if (((object)left == null) || ((object)right == null))
                return true;

            if ((left.Projection == right.Projection) && (left.Type == right.Type) && (left.CenterS == right.CenterS) && (left.CenterT == right.CenterT)
                && (left.Unknown0 == right.Unknown0) && (left.ScaleS == right.ScaleS) && (left.ScaleT == right.ScaleT) && (left.Rotation == right.Rotation)
                && (left.TranslateS == right.TranslateS) && (left.TranslateT == right.TranslateT) && (left.PreMatrix == right.PreMatrix))
                return false;
            else
                return true;
        }
    }
}
