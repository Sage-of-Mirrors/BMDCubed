using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;

namespace BMDCubed.Materials
{
    class TexCoordGen
    {
        public GXTexGenType Type;
        public GXTexGenSrc Source;
        public GXTexMatrix TexMatrixSource;

        public TexCoordGen()
        {
            Type = GXTexGenType.Matrix2x4;
            Source = GXTexGenSrc.Tex0;
            TexMatrixSource = GXTexMatrix.TexMtx0;
        }

        public TexCoordGen(GXTexGenType type, GXTexGenSrc src, GXTexMatrix matrix)
        {
            Type = type;
            Source = src;
            TexMatrixSource = matrix;
        }

        public void Write(EndianBinaryWriter writer)
        {
            writer.Write((byte)Type);
            writer.Write((byte)Source);
            writer.Write((byte)TexMatrixSource);

            // Pad entry to 4 bytes
            writer.Write((byte)0xFF);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(TexCoordGen))
                return Compare((TexCoordGen)obj);
            else
                return false;
        }

        private bool Compare(TexCoordGen obj)
        {
            if ((Type == obj.Type) && (Source == obj.Source) && (TexMatrixSource == obj.TexMatrixSource))
                return true;
            else 
                return false;
        }

        public static bool operator ==(TexCoordGen left, TexCoordGen right)
        {
            if (System.Object.ReferenceEquals(left, right))
                return true;

            if (((object)left == null) || ((object)right == null))
                return false;

            if ((left.Type == right.Type) && (left.Source == right.Source) && (left.TexMatrixSource == right.TexMatrixSource))
                return true;

            else
                return false;
        }

        public static bool operator !=(TexCoordGen left, TexCoordGen right)
        {
            if (System.Object.ReferenceEquals(left, right))
                return false;

            if (((object)left == null) || ((object)right == null))
                return true;

            if ((left.Type == right.Type) && (left.Source == right.Source) && (left.TexMatrixSource == right.TexMatrixSource))
                return false;
            else
                return true;
        }
    }
}
