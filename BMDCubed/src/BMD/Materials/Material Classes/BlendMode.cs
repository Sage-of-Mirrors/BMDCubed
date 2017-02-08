using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;

namespace BMDCubed.Materials
{
    class BlendMode
    {
        /// <summary> Blending Type </summary>
        public GXBlendMode Type;

        /// <summary> Blending Control </summary>
        public GXBlendModeControl SourceFact;

        /// <summary> Blending Control </summary>
        public GXBlendModeControl DestinationFact;

        /// <summary> What operation is used to blend them when <see cref="Type"/> is set to <see cref="GXBlendMode.Logic"/>. </summary>
        public GXLogicOp Operation; // Seems to be logic operators such as clear, and, copy, equiv, inv, invand, etc.

        public BlendMode()
        {
        }

        public BlendMode(GXBlendMode type, GXBlendModeControl srcFact, GXBlendModeControl destFact, GXLogicOp op)
        {
            Type = type;
            SourceFact = srcFact;
            DestinationFact = destFact;
            Operation = op;
        }

        public void Write(EndianBinaryWriter writer)
        {
            writer.Write((byte)Type);
            writer.Write((byte)SourceFact);
            writer.Write((byte)DestinationFact);
            writer.Write((byte)Operation);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(BlendMode))
                return Compare((BlendMode)obj);
            else
                return false;
        }

        private bool Compare(BlendMode obj)
        {
            if (Type == obj.Type && SourceFact == obj.SourceFact && DestinationFact == obj.DestinationFact
                && Operation == obj.Operation)
                return true;
            else
                return false;
        }

        public static bool operator ==(BlendMode left, BlendMode right)
        {
            if (System.Object.ReferenceEquals(left, right))
                return true;

            if (((object)left == null) || ((object)right == null))
                return false;

            if (left.Type == right.Type && left.SourceFact == right.SourceFact && left.DestinationFact == right.DestinationFact
                && left.Operation == right.Operation)
                return true;
            else
                return false;
        }

        public static bool operator !=(BlendMode left, BlendMode right)
        {
            if (System.Object.ReferenceEquals(left, right))
                return false;

            if (((object)left == null) || ((object)right == null))
                return true;

            if (left.Type == right.Type && left.SourceFact == right.SourceFact && left.DestinationFact == right.DestinationFact
                && left.Operation == right.Operation)
                return false;
            else
                return true;
        }
    }
}
