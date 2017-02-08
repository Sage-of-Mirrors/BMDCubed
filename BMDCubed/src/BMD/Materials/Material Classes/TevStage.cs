using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;

namespace BMDCubed.Materials
{
    class TevStage
    {
        public byte Unknown0; // Always 0xFF
        public GXCombineColorInput[] ColorIn; // 4
        public GXTevOp ColorOp;
        public GXTevBias ColorBias;
        public GXTevScale ColorScale;
        public bool ColorClamp;
        public byte ColorRegId;
        public GXCombineAlphaInput[] AlphaIn; // 4
        public GXTevOp AlphaOp;
        public GXTevBias AlphaBias;
        public GXTevScale AlphaScale;
        public bool AlphaClamp;
        public byte AlphaRegId;
        public byte Unknown1; // Always 0xFF

        public TevStage()
        {
            Unknown0 = 0xFF;
            Unknown1 = 0xFF;
            ColorIn = new GXCombineColorInput[4];
            AlphaIn = new GXCombineAlphaInput[4];
        }

        public TevStage(GXCombineColorInput[] colInput, GXTevOp colOp, GXTevBias colBias, GXTevScale colScale, bool colClamp, byte colRegID,
            GXCombineAlphaInput[] alphIn, GXTevOp alphOp, GXTevBias alphBias, GXTevScale alphScale, bool alphClamp, byte alphRegID)
        {
            ColorIn = colInput;
            ColorOp = colOp;
            ColorBias = colBias;
            ColorScale = colScale;
            ColorClamp = colClamp;
            ColorRegId = colRegID;

            AlphaIn = alphIn;
            AlphaOp = alphOp;
            AlphaBias = alphBias;
            AlphaScale = alphScale;
            AlphaClamp = alphClamp;
            AlphaRegId = alphRegID;

            Unknown0 = 0xFF;
            Unknown1 = 0xFF;
        }

        public void Write(EndianBinaryWriter writer)
        {
            writer.Write(Unknown0);
            for (int i = 0; i < 4; i++)
                writer.Write((byte)ColorIn[i]);
            writer.Write((byte)ColorOp);
            writer.Write((byte)ColorBias);
            writer.Write((byte)ColorScale);
            writer.Write(ColorClamp);
            writer.Write(ColorRegId);
            for (int i = 0; i < 4; i++)
                writer.Write((byte)AlphaIn[i]);
            writer.Write((byte)AlphaOp);
            writer.Write((byte)AlphaBias);
            writer.Write((byte)AlphaScale);
            writer.Write(AlphaClamp);
            writer.Write(AlphaRegId);
            writer.Write(Unknown1);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(TevStage))
                return Compare((TevStage)obj);
            else
                return false;
        }

        private bool Compare(TevStage obj)
        {
            if (Unknown0 == obj.Unknown0 && ColorIn == obj.ColorIn && ColorOp == obj.ColorOp && ColorBias == obj.ColorBias
                && ColorScale == obj.ColorScale && ColorRegId == obj.ColorRegId && AlphaIn == obj.AlphaIn && AlphaOp == obj.AlphaOp
                && AlphaBias == obj.AlphaBias && AlphaClamp == obj.AlphaClamp && AlphaRegId == obj.AlphaRegId && Unknown1 == obj.Unknown1)
                return true;
            else
                return false;
        }

        public static bool operator ==(TevStage left, TevStage right)
        {
            if (System.Object.ReferenceEquals(left, right))
                return true;

            if (((object)left == null) || ((object)right == null))
                return false;

            if (left.Unknown0 == right.Unknown0 && left.ColorIn == right.ColorIn && left.ColorOp == right.ColorOp && left.ColorBias == right.ColorBias
                && left.ColorScale == right.ColorScale && left.ColorRegId == right.ColorRegId && left.AlphaIn == right.AlphaIn && left.AlphaOp == right.AlphaOp
                && left.AlphaBias == right.AlphaBias && left.AlphaClamp == right.AlphaClamp && left.AlphaRegId == right.AlphaRegId && left.Unknown1 == right.Unknown1)
                return true;
            else
                return false;
        }

        public static bool operator !=(TevStage left, TevStage right)
        {
            if (System.Object.ReferenceEquals(left, right))
                return false;

            if (((object)left == null) || ((object)right == null))
                return true;

            if (left.Unknown0 == right.Unknown0 && left.ColorIn == right.ColorIn && left.ColorOp == right.ColorOp && left.ColorBias == right.ColorBias
                && left.ColorScale == right.ColorScale && left.ColorRegId == right.ColorRegId && left.AlphaIn == right.AlphaIn && left.AlphaOp == right.AlphaOp
                && left.AlphaBias == right.AlphaBias && left.AlphaClamp == right.AlphaClamp && left.AlphaRegId == right.AlphaRegId && left.Unknown1 == right.Unknown1)
                return false;
            else
                return true;
        }
    }
}
