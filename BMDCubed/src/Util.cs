using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;

namespace BMDCubed.src
{
    static class Util
    {
        const string PaddingString = "This is padding data to align to";

        static public void PadStreamWithString(EndianBinaryWriter writer, int padValue)
        {
            // Pad up to a 32 byte alignment
            // Formula: (x + (n-1)) & ~(n-1)
            long nextAligned = (writer.BaseStream.Length + (padValue - 1)) & ~(padValue - 1);

            long delta = nextAligned - writer.BaseStream.Length;
            writer.BaseStream.Position = writer.BaseStream.Length;
            for (int i = 0; i < delta; i++)
            {
                writer.Write(PaddingString[i]);
            }
        }

        static public void PadStreamWithZero(EndianBinaryWriter writer, int padValue)
        {
            // Pad up to a 32 byte alignment
            // Formula: (x + (n-1)) & ~(n-1)
            long nextAligned = (writer.BaseStream.Length + (padValue - 1)) & ~(padValue - 1);

            long delta = nextAligned - writer.BaseStream.Length;
            writer.BaseStream.Position = writer.BaseStream.Length;
            for (int i = 0; i < delta; i++)
            {
                writer.Write(0);
            }
        }

        static public void WriteOffset(EndianBinaryWriter writer, int offset)
        {
            writer.BaseStream.Seek(offset, System.IO.SeekOrigin.Begin);
            writer.Write((int)writer.BaseStream.Length);
            writer.BaseStream.Seek(0, System.IO.SeekOrigin.End);
        }

        static public float RadsToDegrees(float rads)
        {
            return (float)(rads * (180 / Math.PI));
        }

        static public ushort HashName(string name)
        {
            ushort hash = 0;
            foreach (char c in name)
            {
                hash *= 3;
                hash += (ushort)c;
            }

            return hash;
        }
    }
}
