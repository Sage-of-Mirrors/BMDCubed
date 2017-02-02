using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using grendgine_collada;
using GameFormatReader.Common;
using BMDCubed.src.BMD.Skinning;

namespace BMDCubed.src
{
    class BMDManager
    {
        public Skeleton Skeleton;

        public BMDManager(Grendgine_Collada scene)
        {
            Skeleton = new Skeleton(scene);
        }

        public void WriteBMD(EndianBinaryWriter writer)
        {

        }
    }
}
