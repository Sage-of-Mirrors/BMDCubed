using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;

namespace BMDCubed.Materials
{
    class TevOrder
    {
        public GXTexCoordSlot TexCoordId;
        public byte TexMap;
        public GXColorChannelId ChannelId;

        public TevOrder()
        {
        }

        public TevOrder(GXTexCoordSlot slot, byte texMap, GXColorChannelId chanID)
        {
            TexCoordId = slot;
            TexMap = texMap;
            ChannelId = chanID;
        }

        public void Write(EndianBinaryWriter writer)
        {
            writer.Write((byte)TexCoordId);
            writer.Write(TexMap);
            writer.Write((byte)ChannelId);

            // Pad entry to 4 bytes
            writer.Write((byte)0xFF);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(TevOrder))
                return Compare((TevOrder)obj);
            else
                return false;
        }

        private bool Compare(TevOrder obj)
        {
            if ((TexCoordId == obj.TexCoordId) && (TexMap == obj.TexMap) && (ChannelId == obj.ChannelId))
                return true;
            else
                return false;
        }

        public static bool operator ==(TevOrder left, TevOrder right)
        {
            if (System.Object.ReferenceEquals(left, right))
                return true;

            if (((object)left == null) || ((object)right == null))
                return false;

            if ((left.TexCoordId == right.TexCoordId) && (left.TexMap == right.TexMap) && (left.ChannelId == right.ChannelId))
                return true;
            else
                return false;
        }

        public static bool operator !=(TevOrder left, TevOrder right)
        {
            if (System.Object.ReferenceEquals(left, right))
                return false;

            if (((object)left == null) || ((object)right == null))
                return true;

            if ((left.TexCoordId == right.TexCoordId) && (left.TexMap == right.TexMap) && (left.ChannelId == right.ChannelId))
                return false;
            else
                return true;
        }
    }
}
