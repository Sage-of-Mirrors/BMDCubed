using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;
using OpenTK;

namespace BMDCubed.Materials
{
    class NBTScale
    {
        public byte Unknown1;
        public Vector3 Scale;

        public NBTScale()
        {
            Scale = new Vector3(1, 1, 1);
        }

        public NBTScale(byte unk1, Vector3 scale)
        {
            Unknown1 = unk1;
            Scale = scale;
        }

        public void Write(EndianBinaryWriter writer)
        {
            writer.Write(Unknown1);
            writer.Write((byte)0xFF);
            writer.Write((byte)0xFF);
            writer.Write((byte)0xFF);
            writer.Write(Scale.X);
            writer.Write(Scale.Y);
            writer.Write(Scale.Z);
        }
    }
}
