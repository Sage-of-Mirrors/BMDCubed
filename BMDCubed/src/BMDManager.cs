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
        public SkinningManager Skeleton;
        public GeometryManager Geometry;

        public BMDManager(Grendgine_Collada scene)
        {
            //Skeleton = new SkeletonData(scene);
            Skeleton = new SkinningManager(scene);
            Geometry = new GeometryManager(scene);
            Skeleton.SkelData.AssignBoneBoundingBoxes(Geometry.VertexData.Positions, Skeleton.Drw1Data.AllWeights);
        }

        public void WriteBMD(EndianBinaryWriter writer)
        {
            // Write the header
            writer.Write("J3D2bmd3".ToCharArray()); // Magic, "J3D2bmd3"
            writer.Write(0); // Placeholder for file size
            writer.Write(8); // Number of chunks. BMD has 8

            // BMD has a chunk not counted in the chunk count above, called "SVR3"
            // No idea what it does. It's just the FourCC and 3 ints of -1.
            writer.Write("SVR3".ToCharArray());
            writer.Write(-1);
            writer.Write(-1);
            writer.Write(-1);

            // Write INF1

            // Write VTX1
            using (MemoryStream vtx1 = new MemoryStream())
            {
                EndianBinaryWriter vtx1Writer = new EndianBinaryWriter(vtx1, Endian.Big);
                Geometry.VertexData.WriteVTX1(vtx1Writer);
                writer.Write(vtx1.ToArray());
            }

            // Write EVP1
            using (MemoryStream evp1 = new MemoryStream())
            {
                EndianBinaryWriter evp1Writer = new EndianBinaryWriter(evp1, Endian.Big);
                Skeleton.Drw1Data.WriteEVP1(evp1Writer);
                writer.Write(evp1.ToArray());
            }

            // Write DRW1
            using (MemoryStream drw1 = new MemoryStream())
            {
                EndianBinaryWriter drw1Writer = new EndianBinaryWriter(drw1, Endian.Big);
                Skeleton.Drw1Data.WriteDRW1(drw1Writer);
                writer.Write(drw1.ToArray());
            }

            // Write JNT1
            using (MemoryStream jnt1 = new MemoryStream())
            {
                EndianBinaryWriter jnt1Writer = new EndianBinaryWriter(jnt1, Endian.Big);
                Skeleton.SkelData.WriteJNT1(jnt1Writer);
                writer.Write(jnt1.ToArray());
            }

            // Write SHP1
            using (MemoryStream shp1 = new MemoryStream())
            {
                EndianBinaryWriter shp1Writer = new EndianBinaryWriter(shp1, Endian.Big);
                Geometry.BatchData.WriteSHP1(shp1Writer);
                writer.Write(shp1.ToArray());
            }

            // Write MAT3

            // Write TEX1

            // Write file size
            Util.WriteOffset(writer, 8);
        }
    }
}
