using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using grendgine_collada;
using OpenTK;

namespace BMDCubed.src.BMD.Geometry
{
    class GeometryManager
    {
        public List<Vertex> Vertices;
        public List<Vector3> Positions;
        public List<Vector3> Normals;

        public GeometryManager(Grendgine_Collada scene)
        {
            Vertices = new List<Vertex>();
            Positions = new List<Vector3>();
            Normals = new List<Vector3>();

            if (scene.Library_Geometries == null)
            {
                throw new FormatException("Mesh has no geometry!");
            }

            foreach (Grendgine_Collada_Geometry geom in scene.Library_Geometries.Geometry)
            {
                ParseGeometry(geom);
            }
        }

        private void ParseGeometry(Grendgine_Collada_Geometry geom)
        {
            Grendgine_Collada_Vertices vertexes = geom.Mesh.Vertices;

            foreach (Grendgine_Collada_Source src in geom.Mesh.Source)
            {
                if (src.ID.Contains("POSITION"))
                {
                    GetVertexPositions(src);
                }
                else if (src.ID.Contains("Normal"))
                {
                    GetVertexNormals(src);
                }
            }

            /*
            foreach (Grendgine_Collada_Input_Unshared input in vertexes.Input)
            {
                if (input.Semantic == Grendgine_Collada_Input_Semantic.POSITION)
                {
                    string source = input.source.Remove(0, 1);

                    foreach (Grendgine_Collada_Source src in geom.Mesh.Source)
                    {
                        if (src.ID == source)
                        {
                            string blah = src.Float_Array.Value_As_String.Replace('\n', ' ').Trim();
                            float[] vertPos = Grendgine_Collada_Parse_Utils.String_To_Float(blah);

                            for (int i = 0; i < vertPos.Length; i+=3)
                            {
                                Vector3 pos = new Vector3(vertPos[i], vertPos[i + 1], vertPos[i + 2]);
                                Positions.Add(pos);
                            }
                        }
                    }
                }
            }*/
        }

        private void GetVertexPositions(Grendgine_Collada_Source src)
        {
            string blah = src.Float_Array.Value_As_String.Replace('\n', ' ').Trim();
            float[] vertPos = Grendgine_Collada_Parse_Utils.String_To_Float(blah);

            for (int i = 0; i < vertPos.Length; i += 3)
            {
                Vector3 pos = new Vector3(vertPos[i], vertPos[i + 1], vertPos[i + 2]);
                Positions.Add(pos);
            }
        }

        private void GetVertexNormals(Grendgine_Collada_Source src)
        {
            string blah = src.Float_Array.Value_As_String.Replace('\n', ' ').Trim();
            float[] vertNorm = Grendgine_Collada_Parse_Utils.String_To_Float(blah);

            for (int i = 0; i < vertNorm.Length; i += 3)
            {
                Vector3 pos = new Vector3(vertNorm[i], vertNorm[i + 1], vertNorm[i + 2]);
                Normals.Add(pos);
            }
        }
    }
}
