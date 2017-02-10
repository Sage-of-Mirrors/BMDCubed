using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using grendgine_collada;
using OpenTK;
using GameFormatReader.Common;
using System.IO;

namespace BMDCubed.src.BMD.Geometry
{
    class BatchData
    {
        public List<Batch> Batches;
        List<VertexAttributes>[] ActiveAttributesPerBatch;
        List<int> vertexAttributeOffsets;

        public BatchData(Grendgine_Collada_Mesh mesh)
        {
            Batches = new List<Batch>();

            foreach (Grendgine_Collada_Triangles tri in mesh.Triangles)
            {
                Batch batch = new Batch(tri);
                Batches.Add(batch);
            }

            ActiveAttributesPerBatch = new List<VertexAttributes>[Batches.Count];

            foreach (Batch bat in Batches)
            {
                for (int i = 0; i < Batches.Count; i++)
                {
                    if (ActiveAttributesPerBatch[i] == null)
                    {
                        ActiveAttributesPerBatch[i] = bat.ActiveAttributes;
                        bat.AttributeIndex = i;
                        break;
                    }
                    else
                    {
                        if (ActiveAttributesPerBatch[i].SequenceEqual(bat.ActiveAttributes))
                        {
                            bat.AttributeIndex = i;
                            break;
                        }
                    }
                }
            }
        }

        public void SetBoundingBoxes(List<Vector3> posList)
        {
            foreach (Batch batch in Batches)
                batch.GetBoundingBoxData(posList);
        }

        public void WriteSHP1(EndianBinaryWriter writer)
        {
            vertexAttributeOffsets = new List<int>();

            writer.Write("SHP1".ToCharArray());
            writer.Write(0); // Placeholder for chunk size
            writer.Write((short)Batches.Count);
            writer.Write((short)-1);

            writer.Write(0x2C); // Offset to batch data
            writer.Write(0); // Placeholder for offset to index table
            writer.Write(0); // Placeholder for offset to ???
            writer.Write(0); // Placeholder for offset to batch attributes
            writer.Write(0); // Placeholder for offset to matrix table
            writer.Write(0); // Placeholder for offset to packet data
            writer.Write(0); // Placeholder for offset to matrix data
            writer.Write(0); // Placeholder for offset to packet location data

            using (MemoryStream batchAttribData = new MemoryStream())
            {
                EndianBinaryWriter attribWriter = new EndianBinaryWriter(batchAttribData, Endian.Big);

                foreach (List<VertexAttributes> dat in ActiveAttributesPerBatch)
                {
                    if (dat == null)
                        break;

                    vertexAttributeOffsets.Add((int)(attribWriter.BaseStream.Length));

                    for (int i = 0; i < dat.Count; i++)
                    {
                        attribWriter.Write((int)dat[i]);
                        if (dat[i] == VertexAttributes.PositionMatrixIndex)
                            attribWriter.Write(1);
                        else
                            attribWriter.Write(3);
                    }

                    attribWriter.Write(0xFF);
                    attribWriter.Write(0);
                }

                WriteBatches(writer);
                Util.WriteOffset(writer, 0x10);

                for (int i = 0; i < Batches.Count; i++)
                    writer.Write((short)i);

                Util.WriteOffset(writer, 0x18);
                writer.Write(batchAttribData.ToArray());
            }

            Util.WriteOffset(writer, 0x1C);

            // Write chunk size
            Util.WriteOffset(writer, 4);
        }

        private void WriteBatches(EndianBinaryWriter writer)
        {
            for (int i = 0; i < Batches.Count; i++)
                Batches[i].WriteBatch(writer, vertexAttributeOffsets, i);
        }
    }
}
