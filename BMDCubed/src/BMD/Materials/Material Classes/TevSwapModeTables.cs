using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;

namespace BMDCubed.Materials
{
    class TevSwapModeTable
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public TevSwapModeTable()
        {

        }

        public TevSwapModeTable(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public void Write(EndianBinaryWriter writer)
        {
            writer.Write(R);
            writer.Write(G);
            writer.Write(B);
            writer.Write(A);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(TevSwapModeTable))
                return Compare((TevSwapModeTable)obj);
            else
                return false;
        }

        private bool Compare(TevSwapModeTable obj)
        {
            if (R == obj.R && G == obj.G && B == obj.B && A == obj.A)
                return true;
            else
                return false;
        }

        public static bool operator ==(TevSwapModeTable left, TevSwapModeTable right)
        {
            if (System.Object.ReferenceEquals(left, right))
                return true;

            if (((object)left == null) || ((object)right == null))
                return false;

            if (left.R == right.R && left.G == right.G && left.B == right.B && left.A == right.A)
                return true;
            else
                return false;
        }

        public static bool operator !=(TevSwapModeTable left, TevSwapModeTable right)
        {
            if (System.Object.ReferenceEquals(left, right))
                return false;

            if (((object)left == null) || ((object)right == null))
                return true;

            if (left.R == right.R && left.G == right.G && left.B == right.B && left.A == right.A)
                return false;
            else
                return true;
        }
    }
}
