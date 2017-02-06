using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using grendgine_collada;
using GameFormatReader.Common;
using BMDCubed.src.BMD.Skinning;
using BMDCubed.src.BMD.Geometry;
using System.IO;

namespace BMDCubed.src
{
    class BMDManager
    {
        public Skeleton Skeleton;
        public GeometryManager Geometry;

        public BMDManager(Grendgine_Collada scene)
        {
            Skeleton = new Skeleton(scene);
            Geometry = new GeometryManager(scene);
        }

        public void WriteBMD(EndianBinaryWriter writer)
        {
            using (MemoryStream vtx1 = new MemoryStream())
            {
                EndianBinaryWriter vtx1Writer = new EndianBinaryWriter(vtx1, Endian.Big);
                Geometry.VertexData.WriteVTX1(vtx1Writer);
                writer.Write(vtx1.ToArray());
            }

            using (MemoryStream evp1 = new MemoryStream())
            {
                EndianBinaryWriter evp1Writer = new EndianBinaryWriter(evp1, Endian.Big);
                Skeleton.WriteEVP1(evp1Writer);
                writer.Write(evp1.ToArray());
            }

            using (MemoryStream drw1 = new MemoryStream())
            {
                EndianBinaryWriter drw1Writer = new EndianBinaryWriter(drw1, Endian.Big);
                Skeleton.WriteDRW1(drw1Writer);
                writer.Write(drw1.ToArray());
            }

            using (MemoryStream jnt1 = new MemoryStream())
            {
                EndianBinaryWriter jnt1Writer = new EndianBinaryWriter(jnt1, Endian.Big);
                Skeleton.WriteJNT1(jnt1Writer);
                writer.Write(jnt1.ToArray());
            }
        }
    }
}
