using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMDCubed.src.BMD.Skinning
{
    class Weight
    {
        public List<short> BoneIndexes;
        public List<float> BoneWeights;

        public Weight()
        {
            BoneIndexes = new List<short>();
            BoneWeights = new List<float>();
        }

        public void AddBoneWeight(short index, float weight)
        {
            BoneIndexes.Add(index);
            BoneWeights.Add(weight);
        }

        public override string ToString()
        {
            return "Bone Count: " + BoneIndexes.Count;
        }
    }
}
