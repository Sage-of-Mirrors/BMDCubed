using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;

namespace BMDCubed.Materials
{
    class ChannelControl
    {
        public bool Enable;
        public GXColorSrc MaterialSrcColor;
        public GXLightId LitMask;
        public GXDiffuseFn DiffuseFunction;
        public GXAttenuationFn AttenuationFunction;
        public GXColorSrc AmbientSrcColor;

        public ChannelControl()
        {
            Enable = false;
            MaterialSrcColor = GXColorSrc.Register;
            LitMask = GXLightId.None;
            DiffuseFunction = GXDiffuseFn.None;
            AttenuationFunction = GXAttenuationFn.None;
            AmbientSrcColor = GXColorSrc.Register;
        }

        public ChannelControl(bool enable, GXColorSrc matSrc, GXLightId lit, GXDiffuseFn dif, GXAttenuationFn att, GXColorSrc amb)
        {
            Enable = enable;
            MaterialSrcColor = matSrc;
            LitMask = lit;
            DiffuseFunction = dif;
            AttenuationFunction = att;
            AmbientSrcColor = amb;
        }

        public void Write(EndianBinaryWriter writer)
        {
            writer.Write(Enable);
            writer.Write((byte)MaterialSrcColor);
            writer.Write((byte)LitMask);
            writer.Write((byte)DiffuseFunction);
            writer.Write((byte)AttenuationFunction);
            writer.Write((byte)AmbientSrcColor);

            // Pad entry to 8 bytes
            writer.Write((short)-1);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(ChannelControl))
                return Compare(obj as ChannelControl);
            else
                return false;
        }

        private bool Compare(ChannelControl obj)
        {
            if ((Enable == obj.Enable) && (MaterialSrcColor == obj.MaterialSrcColor)
                && (LitMask == obj.LitMask) && (DiffuseFunction == obj.DiffuseFunction)
                && (AttenuationFunction == obj.AttenuationFunction) && (AmbientSrcColor == obj.AmbientSrcColor))
                return true;
            else
                return false;
        }

        public static bool operator ==(ChannelControl left, ChannelControl right)
        {
            if (System.Object.ReferenceEquals(left, right))
                return true;

            if (((object)left == null) || ((object)right == null))
                return false;

            if ((left.Enable == right.Enable) && (left.MaterialSrcColor == right.MaterialSrcColor)
                && (left.LitMask == right.LitMask) && (left.DiffuseFunction == right.DiffuseFunction)
                && (left.AttenuationFunction == right.AttenuationFunction) && (left.AmbientSrcColor == right.AmbientSrcColor))
                return true;
            else
                return false;
        }

        public static bool operator !=(ChannelControl left, ChannelControl right)
        {
            if (System.Object.ReferenceEquals(left, right))
                return false;

            if (((object)left == null) || ((object)right == null))
                return true;

            if ((left.Enable == right.Enable) && (left.MaterialSrcColor == right.MaterialSrcColor)
                && (left.LitMask == right.LitMask) && (left.DiffuseFunction == right.DiffuseFunction)
                && (left.AttenuationFunction == right.AttenuationFunction) && (left.AmbientSrcColor == right.AmbientSrcColor))
                return false;
            else
                return true;
        }
    }
}
