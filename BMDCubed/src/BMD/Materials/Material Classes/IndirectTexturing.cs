using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;

namespace BMDCubed.Materials
{
    class IndirectTexturing
    {
        /// <summary>
        /// Determines if an indirect texture lookup is to take place
        /// </summary>
        public bool HasLookup;
        /// <summary>
        /// The number of indirect texturing stages to use
        /// </summary>
        public byte IndTexStageNum;
        /// <summary>
        /// Unknown value 1. Related to TevOrders.
        /// </summary>
        public byte Unknown1;
        /// <summary>
        /// Unknown value 2. Related to TevOrders.
        /// </summary>
        public byte Unknown2;
        /// <summary>
        /// The dynamic 2x3 matrices to use when transforming the texture coordinates
        /// </summary>
        public IndTexMatrix[] Matrices;
        /// <summary>
        /// U and V scales to use when transforming the texture coordinates
        /// </summary>
        public IndTexScale[] Scales;
        /// <summary>
        /// Instructions for setting up the specified TEV stage for lookup operations
        /// </summary>
        public IndTevOrder[] TevOrders;

        public IndirectTexturing()
        {
            HasLookup = false;
            IndTexStageNum = 0;
            Unknown1 = 0xFF;
            Unknown2 = 0xFF;
            Matrices = new IndTexMatrix[3] { new IndTexMatrix(), new IndTexMatrix(), new IndTexMatrix() };
            Scales = new IndTexScale[4] { new IndTexScale(), new IndTexScale(), new IndTexScale(), new IndTexScale() };
            TevOrders = new IndTevOrder[16] 
            {
                new IndTevOrder(), new IndTevOrder(), new IndTevOrder(), new IndTevOrder(), new IndTevOrder(),
                new IndTevOrder(), new IndTevOrder(), new IndTevOrder(), new IndTevOrder(), new IndTevOrder(),
                new IndTevOrder(), new IndTevOrder(), new IndTevOrder(), new IndTevOrder(), new IndTevOrder(),
                new IndTevOrder()
            };
        }

        public void Write(EndianBinaryWriter writer)
        {
            writer.Write(HasLookup);
            writer.Write(IndTexStageNum);
            writer.Write((short)-1);
            writer.Write(Unknown1);
            writer.Write(Unknown2);
            // Pad header to 20/0x14 bytes
            for (int i = 0; i < 7; i++)
                writer.Write((short)-1);
            // Output matrices
            for (int mat = 0; mat < 3; mat++)
                Matrices[mat].Write(writer);
            // Output scales
            for (int scale = 0; scale < 4; scale++)
                Scales[scale].Write(writer);
            // Output TEV orders
            for (int order = 0; order < 16; order++)
                TevOrders[order].Write(writer);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(IndirectTexturing))
                return Compare((IndirectTexturing)obj);
            else
                return false;
        }

        private bool Compare(IndirectTexturing obj)
        {
            if ((HasLookup == obj.HasLookup) && (IndTexStageNum == obj.IndTexStageNum)
                && (Unknown1 == obj.Unknown1) && (Unknown2 == obj.Unknown2) && (Matrices == obj.Matrices)
                && (Scales == obj.Scales) && (TevOrders == obj.TevOrders))
                return true;
            else
                return false;
        }

        public static bool operator == (IndirectTexturing left, IndirectTexturing right)
        {
            if (System.Object.ReferenceEquals(left, right))
                return true;

            if (((object)left == null) || ((object)right == null))
                return false;

            if ((left.HasLookup == right.HasLookup) && (left.IndTexStageNum == right.IndTexStageNum)
                && (left.Unknown1 == right.Unknown1) && (left.Unknown2 == right.Unknown2) && (left.Matrices == right.Matrices)
                && (left.Scales == right.Scales) && (left.TevOrders == right.TevOrders))
                return true;
            else
                return false;
        }

        public static bool operator !=(IndirectTexturing left, IndirectTexturing right)
        {
            if (System.Object.ReferenceEquals(left, right))
                return false;

            if (((object)left == null) || ((object)right == null))
                return true;

            if ((left.HasLookup == right.HasLookup) && (left.IndTexStageNum == right.IndTexStageNum)
                && (left.Unknown1 == right.Unknown1) && (left.Unknown2 == right.Unknown2) && (left.Matrices == right.Matrices)
                && (left.Scales == right.Scales) && (left.TevOrders == right.TevOrders))
                return false;
            else
                return true;
        }
    }
}
