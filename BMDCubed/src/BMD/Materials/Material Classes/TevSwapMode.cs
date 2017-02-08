using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;

namespace BMDCubed.Materials
{
    class TevSwapMode
    {
        public byte RasSel;
        public byte TexSel;

        public TevSwapMode()
        {
        }

        public TevSwapMode(byte ras, byte tex)
        {
            RasSel = ras;
            TexSel = tex;
        }

        public void Write(EndianBinaryWriter writer)
        {
            writer.Write(RasSel);
            writer.Write(TexSel);
            // Pad to 4 bytes
            writer.Write((short)-1);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(TevSwapMode))
                return Compare((TevSwapMode)obj);
            else
                return false;
        }

        private bool Compare(TevSwapMode obj)
        {
            if (RasSel == obj.RasSel && TexSel == obj.TexSel)
                return true;
            else
                return false;
        }

        public static bool operator ==(TevSwapMode left, TevSwapMode right)
        {
            if (System.Object.ReferenceEquals(left, right))
                return true;

            if (((object)left == null) || ((object)right == null))
                return false;

            if (left.RasSel == right.RasSel && left.TexSel == right.TexSel)
                return true;
            else
                return false;
        }

        public static bool operator !=(TevSwapMode left, TevSwapMode right)
        {
            if (System.Object.ReferenceEquals(left, right))
                return false;

            if (((object)left == null) || ((object)right == null))
                return true;

            if (left.RasSel == right.RasSel && left.TexSel == right.TexSel)
                return false;
            else
                return true;
        }
    }
}
