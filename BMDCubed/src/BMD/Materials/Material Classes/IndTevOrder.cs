using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;

namespace BMDCubed.Materials
{
    /// <summary>
    /// Configures a TEV stage during an indirect texture lookup.
    /// </summary>
    class IndTevOrder
    {
        public byte TevStageID;
        public byte IndTexFormat;
        public byte IndTexBiasSel;
        public byte IndTexMtxId;
        public byte IndTexWrapS;
        public byte IndTexWrapT;
        public bool AddPrev;
        public bool UtcLod;
        public byte AlphaSel;

        public IndTevOrder()
        {
            TevStageID = 0;
            IndTexFormat = 0;
            IndTexBiasSel = 0;
            IndTexMtxId = 0;
            IndTexWrapS = 0;
            IndTexWrapT = 0;
            AddPrev = false;
            UtcLod = false;
            AlphaSel = 0;
        }

        public void Write(EndianBinaryWriter writer)
        {
            writer.Write(TevStageID);
            writer.Write(IndTexFormat);
            writer.Write(IndTexBiasSel);
            writer.Write(IndTexMtxId);
            writer.Write(IndTexWrapS);
            writer.Write(IndTexWrapT);
            writer.Write(AddPrev);
            writer.Write(UtcLod);
            writer.Write(AlphaSel);
            // Pad entry to 12/0xC bytes
            writer.Write((byte)0xFF);
            writer.Write((byte)0xFF);
            writer.Write((byte)0xFF);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(IndTevOrder))
                return Compare((IndTevOrder)obj);
            else
                return false;
        }

        private bool Compare(IndTevOrder obj)
        {
            if ((TevStageID == obj.TevStageID) && (IndTexFormat == obj.IndTexFormat)
                && (IndTexBiasSel == obj.IndTexBiasSel) && (IndTexMtxId == obj.IndTexMtxId)
                && (IndTexWrapS == obj.IndTexWrapS) && (IndTexWrapT == obj.IndTexWrapT)
                && (AddPrev == obj.AddPrev) && (UtcLod == obj.UtcLod) && (AlphaSel == obj.AlphaSel))
                return true;
            else
                return false;
        }

        public static bool operator ==(IndTevOrder left, IndTevOrder right)
        {
            if (System.Object.ReferenceEquals(left, right))
                return true;

            if (((object)left == null) || ((object)right == null))
                return false;

            if ((left.TevStageID == right.TevStageID) && (left.IndTexFormat == right.IndTexFormat)
                && (left.IndTexBiasSel == right.IndTexBiasSel) && (left.IndTexMtxId == right.IndTexMtxId)
                && (left.IndTexWrapS == right.IndTexWrapS) && (left.IndTexWrapT == right.IndTexWrapT)
                && (left.AddPrev == right.AddPrev) && (left.UtcLod == right.UtcLod) && (left.AlphaSel == right.AlphaSel))
                return true;
            else
                return false;
        }

        public static bool operator !=(IndTevOrder left, IndTevOrder right)
        {
            if (System.Object.ReferenceEquals(left, right))
                return false;

            if (((object)left == null) || ((object)right == null))
                return true;

            if ((left.TevStageID == right.TevStageID) && (left.IndTexFormat == right.IndTexFormat)
                && (left.IndTexBiasSel == right.IndTexBiasSel) && (left.IndTexMtxId == right.IndTexMtxId)
                && (left.IndTexWrapS == right.IndTexWrapS) && (left.IndTexWrapT == right.IndTexWrapT)
                && (left.AddPrev == right.AddPrev) && (left.UtcLod == right.UtcLod) && (left.AlphaSel == right.AlphaSel))
                return false;
            else
                return true;
        }
    }
}
