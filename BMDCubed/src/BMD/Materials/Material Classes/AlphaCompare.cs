using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;

namespace BMDCubed.Materials
{
    class AlphaCompare
    {
        /// <summary> subfunction 0 </summary>
        public GXCompareType Comp0;
        /// <summary> Reference value for subfunction 0. </summary>
        public byte Reference0;
        /// <summary> Alpha combine control for subfunctions 0 and 1. </summary>
        public GXAlphaOp Operation;
        /// <summary> subfunction 1 </summary>
        public GXCompareType Comp1;
        /// <summary> Reference value for subfunction 1. </summary>
        public byte Reference1;

        public AlphaCompare()
        {
        }

        public AlphaCompare(GXCompareType com0, byte ref0, GXAlphaOp op, GXCompareType com1, byte ref2)
        {
            Comp0 = com0;
            Reference0 = ref0;
            Operation = op;
            Comp1 = com1;
            Reference1 = ref2;
        }

        public void Write(EndianBinaryWriter writer)
        {
            writer.Write((byte)Comp0);
            writer.Write(Reference0);
            writer.Write((byte)Operation);
            writer.Write((byte)Comp1);
            writer.Write(Reference1);
            // Pad entry to 8 bytes
            writer.Write((byte)0xFF);
            writer.Write((byte)0xFF);
            writer.Write((byte)0xFF);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(AlphaCompare))
                return Compare((AlphaCompare)obj);
            else
                return false;
        }

        private bool Compare(AlphaCompare obj)
        {
            if (Comp0 == obj.Comp0 && Reference0 == obj.Reference0 && Operation == obj.Operation
                && Comp1 == obj.Comp1 && Reference1 == obj.Reference1)
                return true;
            else
                return false;
        }

        public static bool operator ==(AlphaCompare left, AlphaCompare right)
        {
            if (System.Object.ReferenceEquals(left, right))
                return true;

            if (((object)left == null) || ((object)right == null))
                return false;

            if (left.Comp0 == right.Comp0 && left.Reference0 == right.Reference0 && left.Operation == right.Operation
                && left.Comp1 == right.Comp1 && left.Reference1 == right.Reference1)
                return true;
            else
                return false;
        }

        public static bool operator !=(AlphaCompare left, AlphaCompare right)
        {
            if (System.Object.ReferenceEquals(left, right))
                return false;

            if (((object)left == null) || ((object)right == null))
                return true;

            if (left.Comp0 == right.Comp0 && left.Reference0 == right.Reference0 && left.Operation == right.Operation
                && left.Comp1 == right.Comp1 && left.Reference1 == right.Reference1)
                return false;
            else
                return true;
        }
    }
}
