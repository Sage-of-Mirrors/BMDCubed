using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;

namespace BMDCubed.src.BMD.Skinning
{
    class DrawData
    {
        public List<Weight> FullWeightList;
        public List<Weight> PartialWeightList;
        public List<Weight> AllWeights;

        public List<Weight> AllDrw1Weights;
        List<Bone> FlatHierarchy;
        List<Bone> BonesWithGeometry;

        public DrawData(List<Weight> allWeights, List<Bone> flat, List<Bone> geom)
        {
            FullWeightList = new List<Weight>();
            PartialWeightList = new List<Weight>();
            AllDrw1Weights = new List<Weight>();
            AllWeights = allWeights;
            FlatHierarchy = flat;
            BonesWithGeometry = geom;

            foreach (Weight weight in allWeights)
            {
                if (weight.BoneWeights.Count == 1 && !FullWeightList.Contains(weight))
                {
                    FullWeightList.Add(weight);
                }
                else if (weight.BoneWeights.Count > 1 && !PartialWeightList.Contains(weight))
                {
                    PartialWeightList.Add(weight);
                }
            }

            AllDrw1Weights.AddRange(FullWeightList.ToArray());
            AllDrw1Weights.AddRange(PartialWeightList.ToArray());
        }

        public void WriteDRW1(EndianBinaryWriter writer)
        {
            writer.Write("DRW1".ToCharArray()); // FourCC, "DRW1"
            writer.Write(0); // Placeholder for chunk size
            writer.Write((short)(FullWeightList.Count + PartialWeightList.Count));
            writer.Write((ushort)0xFFFF); // Padding
            writer.Write(0x14); // Offset to bool table, it's constant
            writer.Write(0); // Placeholder for index offset

            for (int i = 0; i < FullWeightList.Count; i++)
                writer.Write((byte)0);

            for (int i = 0; i < PartialWeightList.Count; i++)
                writer.Write((byte)1);

            Util.WriteOffset(writer, 0x10);

            for (int i = 0; i < FullWeightList.Count; i++)
                writer.Write((short)FlatHierarchy.IndexOf(BonesWithGeometry[i]));

            for (int i = 0; i < PartialWeightList.Count; i++)
                writer.Write((short)i);

            Util.PadStreamWithString(writer, 32);

            Util.WriteOffset(writer, 4);
        }
    }
}
