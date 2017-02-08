using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using GameFormatReader.Common;

namespace BMDCubed.Materials
{
    /// <summary>
    /// Defines a matrix used to transform source texture coordinates during an indirect texture lookup.
    /// </summary>
    class IndTexMatrix
    {
        /// <summary>
        /// The floats that make up the matrix
        /// </summary>
        public Matrix3 Matrix;
        /// <summary>
        /// The exponent (of 2) to multiply the matrix by
        /// </summary>
        public byte Exponent;

        public IndTexMatrix()
        {
            Matrix = new Matrix3(.5f, 0f, 0f, 0f, .5f, 0f, 0f, 0f, 0f);
            Exponent = 1;
        }

        public void Write(EndianBinaryWriter writer)
        {
            // Write matrix floats
            writer.Write(Matrix.M11);
            writer.Write(Matrix.M12);
            writer.Write(Matrix.M13);
            writer.Write(Matrix.M21);
            writer.Write(Matrix.M22);
            writer.Write(Matrix.M23);
            // Write exponent
            writer.Write(Exponent);
            // Pad exponent to 4 bytes
            writer.Write((byte)0xFF);
            writer.Write((byte)0xFF);
            writer.Write((byte)0xFF);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(IndTexMatrix))
                return Compare((IndTexMatrix)obj);
            else
                return false;
        }

        private bool Compare(IndTexMatrix obj)
        {
            if ((Matrix == obj.Matrix) && (Exponent == obj.Exponent))
                return true;
            else
                return false;
        }

        public static bool operator == (IndTexMatrix left, IndTexMatrix right)
        {
            if (System.Object.ReferenceEquals(left, right))
                return true;

            if (((object)left == null) || ((object)right == null))
                return false;

            if ((left.Matrix == right.Matrix) && (left.Exponent == right.Exponent))
                return true;
            else
                return false;
        }

        public static bool operator != (IndTexMatrix left, IndTexMatrix right)
        {
            if (System.Object.ReferenceEquals(left, right))
                return false;

            if (((object)left == null) || ((object)right == null))
                return true;

            if ((left.Matrix == right.Matrix) && (left.Exponent == right.Exponent))
                return false;
            else
                return true;
        }
    }
}
