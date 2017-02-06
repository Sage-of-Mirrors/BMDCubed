using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using GameFormatReader.Common;

namespace BMDCubed.src
{
    class BoundingBox
    {
        public Vector3 Minimum;
        public Vector3 Maximum;
        Vector3 Center;
        public float SphereRadius;

        public BoundingBox()
        {
            Minimum = new Vector3();
            Maximum = new Vector3();
            SphereRadius = 0.0f;
        }

        public BoundingBox(List<Vector3> positions)
        {
            #region Max and min
            float maxX = float.MinValue;

            float maxY = float.MinValue;

            float maxZ = float.MinValue;

            float minX = float.MaxValue;

            float minY = float.MaxValue;

            float minZ = float.MaxValue;

            foreach (Vector3 vec in positions)
            {
                if (vec.X > maxX)
                    maxX = vec.X;

                if (vec.Y > maxY)
                    maxY = vec.Y;

                if (vec.Z > maxZ)
                    maxZ = vec.Z;

                if (vec.X < minX)
                    minX = vec.X;

                if (vec.Y < minY)
                    minY = vec.Y;

                if (vec.Z < minZ)
                    minZ = vec.Z;
            }

            Maximum = new Vector3(maxX, maxY, maxZ);

            Minimum = new Vector3(minX, minY, minZ);
            #endregion

            #region Center
            Center.X = (Maximum.X + Minimum.X) / 2;

            Center.Y = (Maximum.Y + Minimum.Y) / 2;

            Center.Z = (Maximum.Z + Minimum.Z) / 2;
            #endregion

            //Maximum = Maximum - Center;
            //Minimum = Minimum - Center;

            #region Bounding Sphere Radius

            float radius = float.MinValue;

            foreach (Vector3 vec in positions)
            {
                Vector3 transformedVec = vec - Center;

                if (transformedVec.Length > radius)
                    radius = transformedVec.Length;
            }

            SphereRadius = ((Maximum - Minimum) / 2).Length;

            //SphereRadius = radius;

            #endregion
        }

        public void WriteBoundingBox(EndianBinaryWriter writer)
        {
            writer.Write(SphereRadius);

            writer.Write(Minimum.X);
            writer.Write(Minimum.Y);
            writer.Write(Minimum.Z);

            writer.Write(Maximum.X);
            writer.Write(Maximum.Y);
            writer.Write(Maximum.Z);
        }
    }
}
