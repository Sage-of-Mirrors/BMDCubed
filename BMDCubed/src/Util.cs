using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;
using OpenTK;

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
                writer.Write((byte)0);
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

        public static Vector3 ToEulerAngles(this Quaternion q)
        {
            // Store the Euler angles in radians
            Vector3 pitchYawRoll = new Vector3();

            double sqw = q.W * q.W;
            double sqx = q.X * q.X;
            double sqy = q.Y * q.Y;
            double sqz = q.Z * q.Z;

            // If quaternion is normalised the unit is one, otherwise it is the correction factor
            double unit = sqx + sqy + sqz + sqw;
            double test = q.X * q.Y + q.Z * q.W;

            if (test > 0.4999f * unit)                              // 0.4999f OR 0.5f - EPSILON
            {
                // Singularity at north pole
                pitchYawRoll.Y = 2f * (float)Math.Atan2(q.X, q.W);  // Yaw
                pitchYawRoll.X = (float)Math.PI * 0.5f;                         // Pitch
                pitchYawRoll.Z = 0f;                                // Roll
                return pitchYawRoll;
            }
            else if (test < -0.4999f * unit)                        // -0.4999f OR -0.5f + EPSILON
            {
                // Singularity at south pole
                pitchYawRoll.Y = -2f * (float)Math.Atan2(q.X, q.W); // Yaw
                pitchYawRoll.X = -(float)Math.PI * 0.5f;                        // Pitch
                pitchYawRoll.Z = 0f;                                // Roll
                return pitchYawRoll;
            }
            else
            {
                pitchYawRoll.Y = (float)Math.Atan2(2f * q.Y * q.W - 2f * q.X * q.Z, sqx - sqy - sqz + sqw);       // Yaw
                pitchYawRoll.X = (float)Math.Asin(2f * test / unit);                                             // Pitch
                pitchYawRoll.Z = (float)Math.Atan2(2f * q.X * q.W - 2f * q.Y * q.Z, -sqx + sqy - sqz + sqw);      // Roll
            }

            return pitchYawRoll;
        }

        public static Vector3 ToEulerianAngles(Quaternion q)
        {
            Vector3 vec = new Vector3();

            float ysqr = q.Y * q.Y;

            float t0 = 2.0f * (q.W * q.X + q.Y * q.Z);
            float t1 = 1.0f - 2.0f * (q.X * q.X + ysqr);

            vec.X = (float)Math.Atan2(t0, t1);

            float t2 = 2.0f * (q.W * q.Y - q.Z * q.X);
            t2 = t2 > 1.0f ? 1.0f : t2;
            t2 = t2 < -1.0f ? -1.0f : t2;

            vec.Y = (float)Math.Asin(t2);

            float t3 = 2.0f * (q.W * q.Z + q.X * q.Y);
            float t4 = 1.0f - 2.0f * (ysqr + q.Z * q.Z);

            vec.Z = (float)Math.Atan2(t3, t4);

            return vec;
        }
    }
}
