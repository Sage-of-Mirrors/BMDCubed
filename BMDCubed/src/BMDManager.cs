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
                Geometry.WriteVTX1(vtx1Writer);
                writer.Write(vtx1.ToArray());
            }

            using (MemoryStream evp1 = new MemoryStream())
            {
                EndianBinaryWriter evp1Writer = new EndianBinaryWriter(evp1, Endian.Big);
                Skeleton.WriteEVP1(evp1Writer);
                writer.Write(evp1.ToArray());
            }
            //Geometry.WriteVTX1(writer);
            //Skeleton.WriteEVP1(writer);
        }
    }
}
