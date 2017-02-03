using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using grendgine_collada;
using GameFormatReader.Common;
using BMDCubed.src.BMD.Skinning;
using BMDCubed.src.BMD.Geometry;

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
            Skeleton.WriteEVP1(writer);
        }
    }
}
