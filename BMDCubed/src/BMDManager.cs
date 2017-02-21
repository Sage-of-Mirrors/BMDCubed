using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using grendgine_collada;
using GameFormatReader.Common;
using BMDCubed.src.BMD.Skinning;
using BMDCubed.src.BMD.Geometry;
using BMDCubed.Materials;
using System.IO;

namespace BMDCubed.src
{
    class BMDManager
    {
        public SkinningManager Skeleton;
        public GeometryManager Geometry;
        public MaterialManager Materials;

        public BMDManager(Grendgine_Collada scene)
        {
            Skeleton = new SkinningManager(scene);
            Geometry = new GeometryManager(scene, Skeleton.Drw1Data, Skeleton.SkelData.BindShapeMatrix);
            Geometry.VertexData.TransformPositions(Skeleton.Drw1Data, Skeleton.SkelData.FlatHierarchy);
            Skeleton.SkelData.AssignBoneBoundingBoxes(Geometry.VertexData.Positions, Skeleton.Drw1Data);
            Materials = new MaterialManager(scene, Geometry.BatchData.Batches);
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
            using (MemoryStream inf1 = new MemoryStream())
            {
                EndianBinaryWriter inf1Writer = new EndianBinaryWriter(inf1, Endian.Big);
                WriteINF1(inf1Writer);
                writer.Write(inf1.ToArray());
            }

            // Write VTX1
            using (MemoryStream vtx1 = new MemoryStream())
            {
                EndianBinaryWriter vtx1Writer = new EndianBinaryWriter(vtx1, Endian.Big);
                Geometry.VertexData.WriteVTX1(vtx1Writer);
                writer.Write(vtx1.ToArray());
            }

            // Write EVP1
            if (Skeleton.Drw1Data != null)
                WriteEVP1(writer);
            else
                WriteEVP1Stub(writer);

            // Write DRW1
            if (Skeleton.Drw1Data != null)
                WriteDRW1(writer);
            else
                WriteDRW1Stub(writer);

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
            using (MemoryStream mat3 = new MemoryStream())
            {
                EndianBinaryWriter mat3Writer = new EndianBinaryWriter(mat3, Endian.Big);
                Materials.WriteMAT3(mat3Writer);
                writer.Write(mat3.ToArray());
            }

            // Write TEX1
            using (MemoryStream tex1 = new MemoryStream())
            {
                EndianBinaryWriter tex1Writer = new EndianBinaryWriter(tex1, Endian.Big);
                Materials.WriteTEX1(tex1Writer, Materials.TextureList);
                writer.Write(tex1.ToArray());
            }

            // Write file size
            Util.WriteOffset(writer, 8);
        }

        public void WriteINF1(EndianBinaryWriter writer)
        {
            // Header
            writer.Write("INF1".ToCharArray()); // FourCC, "INF1"
            writer.Write(0); // Placeholder for chunk size
            writer.Write((short)1); // Unknown. It's 1?
            writer.Write((ushort)0xFFFF); // Padding
            writer.Write(Geometry.BatchData.Batches.Count); // Number of packets
            writer.Write(Geometry.VertexData.Positions.Count); // Number of vertexes
            writer.Write(0x18); // Offset to hierarchy data. Always 0x18

            Skeleton.SkelData.SkeletonRoot.Materials = Materials.Materials;
            Skeleton.SkelData.SkeletonRoot.WriteScenegraphRecursive(writer, Skeleton.SkelData.FlatHierarchy, Geometry.BatchData.Batches, Materials.Materials);

            writer.Write((short)0);
            writer.Write((short)0);

            Util.PadStreamWithString(writer, 32);

            // Write chunk size size
            Util.WriteOffset(writer, 4);
        }

        private void WriteEVP1(EndianBinaryWriter writer)
        {
            using (MemoryStream evp1 = new MemoryStream())
            {
                EndianBinaryWriter evp1Writer = new EndianBinaryWriter(evp1, Endian.Big);
                Skeleton.Drw1Data.WriteEVP1(evp1Writer);
                writer.Write(evp1.ToArray());
            }
        }

        private void WriteEVP1Stub(EndianBinaryWriter writer)
        {
            using (MemoryStream evp1 = new MemoryStream())
            {
                EndianBinaryWriter evp1Writer = new EndianBinaryWriter(evp1, Endian.Big);

                evp1Writer.Write("EVP1".ToCharArray()); // FourCC, "EVP1"
                evp1Writer.Write(32); // Size. The stub is always 32 bytes
                evp1Writer.Write((short)0); // No EVP1 entries, so this is 0
                evp1Writer.Write((short)-1); // Padding

                // These 0s fill the stub
                evp1Writer.Write(0);
                evp1Writer.Write(0);
                evp1Writer.Write(0);
                evp1Writer.Write(0);

                Util.PadStreamWithString(evp1Writer, 32);

                writer.Write(evp1.ToArray());
            }
        }

        private void WriteDRW1(EndianBinaryWriter writer)
        {
            using (MemoryStream drw1 = new MemoryStream())
            {
                EndianBinaryWriter drw1Writer = new EndianBinaryWriter(drw1, Endian.Big);
                Skeleton.Drw1Data.WriteDRW1(drw1Writer);
                writer.Write(drw1.ToArray());
            }
        }

        private void WriteDRW1Stub(EndianBinaryWriter writer)
        {
            using (MemoryStream drw1 = new MemoryStream())
            {
                EndianBinaryWriter drw1Writer = new EndianBinaryWriter(drw1, Endian.Big);

                drw1Writer.Write("DRW1".ToCharArray()); // FourCC, "DRW1"
                drw1Writer.Write(32); // Size. The stub is always 32 bytes
                drw1Writer.Write((short)1); // The only entry is a reference to the root bone
                drw1Writer.Write((short)-1); // Padding

                drw1Writer.Write(0x14); // Offset to bool array
                drw1Writer.Write(0x16); // Offset to index data

                drw1Writer.Write((byte)0); // Only bool in the array, false.
                drw1Writer.Write((byte)0x54); // Bool array is padded to 2 bytes. I won't bother using the PadStreamWithString function here

                drw1Writer.Write((short)0); // Index of root bone

                Util.PadStreamWithString(drw1Writer, 32);

                writer.Write(drw1.ToArray());
            }
        }
    }
}
