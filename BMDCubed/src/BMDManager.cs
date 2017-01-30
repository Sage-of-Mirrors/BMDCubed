using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using grendgine_collada;
using GameFormatReader.Common;
using BMDCubed.src.BMD;

namespace BMDCubed.src
{
    class BMDManager
    {
        public Bone SkeletonRoot;

        public BMDManager(Grendgine_Collada scene)
        {
            Grendgine_Collada_Node skeleton = GetSkeletonFromVisualScene(scene);
            SkeletonRoot = new Bone(skeleton);

        }

        public void WriteBMD(EndianBinaryWriter writer)
        {

        }

        private Grendgine_Collada_Node GetSkeletonFromVisualScene(Grendgine_Collada scene)
        {
            // We need to get the hierarchy of the bone structure.
            // DAE stores it in the visual scene, but sometimes it's hidden under layers of
            // other nodes. To get around that, we're going to check every node in the visual scene
            // for if A) it's a "JOINT" node and B) it has children, ie the node property isn't null.
            // The check will be recursive, working on every node in the hierarchy until we find what
            // *should* be the skeleton's root.

            Grendgine_Collada_Node root = null;

            foreach(Grendgine_Collada_Visual_Scene visScene in scene.Library_Visual_Scene.Visual_Scene)
            {
                foreach (Grendgine_Collada_Node node in visScene.Node)
                {
                    root = GetSkeletonFromNodeRecursive(node);

                    if (root != null)
                        break;
                }
            }

            return root;
        }

        private Grendgine_Collada_Node GetSkeletonFromNodeRecursive(Grendgine_Collada_Node node)
        {
            Grendgine_Collada_Node skelRoot = null;

            if (node.Type == Grendgine_Collada_Node_Type.JOINT && node.node != null)
            {
                return node;
            }
            else if (node.node != null)
            {
                foreach (Grendgine_Collada_Node child in node.node)
                {
                    skelRoot = GetSkeletonFromNodeRecursive(child);

                    if (skelRoot != null)
                        break;
                }
            }

            return skelRoot;
        }
    }
}
