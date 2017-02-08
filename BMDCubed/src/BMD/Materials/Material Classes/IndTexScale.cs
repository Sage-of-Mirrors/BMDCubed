using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;

namespace BMDCubed.Materials
{
    /// <summary>
    /// Defines S (U) and T (V) scale values to transform source texture coordinates during an indirect texture lookup.
    /// </summary>
    class IndTexScale
    {
        /// <summary>
        /// Scale value for the source texture coordinates' S (U) component
        /// </summary>
        public byte ScaleS;
        /// <summary>
        /// Scale value for the source texture coordinates' T (V) component
        /// </summary>
        public byte ScaleT;

        public IndTexScale()
        {
            ScaleS = 0;
            ScaleT = 0;
        }

        public void Write(EndianBinaryWriter writer)
        {
            writer.Write(ScaleS);
            writer.Write(ScaleT);
            // Pad entry to 4 bytes
            writer.Write((byte)0xFF);
            writer.Write((byte)0xFF);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(IndTexScale))
                return Compare((IndTexScale)obj);
            else
                return false;
        }

        private bool Compare(IndTexScale obj)
        {
            if ((ScaleS == obj.ScaleS) && (ScaleT == obj.ScaleT))
                return true;
            else
                return false;
        }

        public static bool operator ==(IndTexScale left, IndTexScale right)
        {
            if (System.Object.ReferenceEquals(left, right))
                return true;

            if (((object)left == null) || ((object)right == null))
                return false;

            if((left.ScaleS == right.ScaleS) && (left.ScaleT == right.ScaleT))
                return true;
            else
                return false;
        }

        public static bool operator !=(IndTexScale left, IndTexScale right)
        {
            if (System.Object.ReferenceEquals(left, right))
                return false;

            if (((object)left == null) || ((object)right == null))
                return true;

            if ((left.ScaleS == right.ScaleS) && (left.ScaleT == right.ScaleT))
                return false;
            else
                return true;
        }
    }
}
