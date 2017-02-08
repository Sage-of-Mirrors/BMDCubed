using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;

namespace BMDCubed.Materials
{
    class Fog
    {
        public byte Type;
        public bool Enable;
        public ushort Center;
        public float StartZ;
        public float EndZ;
        public float NearZ;
        public float FarZ;
        public Color Color;
        public ushort[] Table;

        public Fog()
        {
            Table = new ushort[10];
        }

        public Fog(byte type, bool enable, ushort center, float startz, float endz, float nearz, float farz, Color col)
        {
            Type = type;
            Enable = enable;
            Center = center;
            StartZ = startz;
            EndZ = endz;
            NearZ = nearz;
            FarZ = farz;
            Color = col;

            Table = new ushort[10];
        }

        public void Write(EndianBinaryWriter writer)
        {
            writer.Write(Type);
            writer.Write(Enable);
            writer.Write(Center);
            writer.Write(StartZ);
            writer.Write(EndZ);
            writer.Write(NearZ);
            writer.Write(FarZ);
            writer.Write((byte)(Color.R * 255.0f));
            writer.Write((byte)(Color.G * 255.0f));
            writer.Write((byte)(Color.B * 255.0f));
            writer.Write((byte)(Color.A * 255.0f));
            for (int i = 0; i < 10; i++)
                writer.Write(Table[i]);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(Fog))
                return Compare((Fog)obj);
            else
                return false;
        }

        private bool Compare(Fog obj)
        {
            if (Type == obj.Type && Enable == obj.Enable && Center == obj.Center && StartZ == obj.StartZ
                && EndZ == obj.EndZ && NearZ == obj.NearZ && FarZ == obj.FarZ && Color == obj.Color && Table == obj.Table)
                return true;
            else
                return false;
        }

        public static bool operator ==(Fog left, Fog right)
        {
            if (System.Object.ReferenceEquals(left, right))
                return true;

            if (((object)left == null) || ((object)right == null))
                return false;

            if (left.Type == right.Type && left.Enable == right.Enable && left.Center == right.Center && left.StartZ == right.StartZ
                && left.EndZ == right.EndZ && left.NearZ == right.NearZ && left.FarZ == right.FarZ && left.Color == right.Color && left.Table == right.Table)
                return true;
            else
                return false;
        }

        public static bool operator !=(Fog left, Fog right)
        {
            if (System.Object.ReferenceEquals(left, right))
                return false;

            if (((object)left == null) || ((object)right == null))
                return true;

            if (left.Type == right.Type && left.Enable == right.Enable && left.Center == right.Center && left.StartZ == right.StartZ
                && left.EndZ == right.EndZ && left.NearZ == right.NearZ && left.FarZ == right.FarZ && left.Color == right.Color && left.Table == right.Table)
                return false;
            else
                return true;
        }
    }
}
