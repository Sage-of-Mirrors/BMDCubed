using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using grendgine_collada;
using OpenTK;

namespace BMDCubed.src.BMD.Geometry
{
    public enum VertexAttributes
    {
        PositionMatrixIndex, Tex0MatrixIndex, Tex1MatrixIndex, Tex2MatrixIndex, Tex3MatrixIndex,
        Tex4MatrixIndex, Tex5MatrixIndex, Tex6MatrixIndex, Tex7MatrixIndex,
        Position, Normal, Color0, Color1, Tex0, Tex1, Tex2, Tex3, Tex4, Tex5, Tex6, Tex7,
        PositionMatrixArray, NormalMatrixArray, TextureMatrixArray, LitMatrixArray, NormalBinormalTangent,
        MaxAttr, NullAttr = 0xFF,
    }

    class GeometryManager
    {
        public List<Vertex> Vertices;
        public List<Vector3> Positions;
        public List<Vector3> Normals;

        public List<Vector3> Color0;
        public List<Vector3> Color1;

        public List<Vector2> UV0;
        public List<Vector2> UV1;
        public List<Vector2> UV2;
        public List<Vector2> UV3;
        public List<Vector2> UV4;
        public List<Vector2> UV5;
        public List<Vector2> UV6;
        public List<Vector2> UV7;

        Dictionary<VertexAttributes, string> activeAttributes;

        public GeometryManager(Grendgine_Collada scene)
        {
            Vertices = new List<Vertex>();
            Positions = new List<Vector3>();
            Normals = new List<Vector3>();

            Color0 = new List<Vector3>();
            Color1 = new List<Vector3>();

            UV0 = new List<Vector2>();
            UV1 = new List<Vector2>();
            UV2 = new List<Vector2>();
            UV3 = new List<Vector2>();
            UV4 = new List<Vector2>();
            UV5 = new List<Vector2>();
            UV6 = new List<Vector2>();
            UV7 = new List<Vector2>();

            activeAttributes = new Dictionary<VertexAttributes, string>();

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
            GetActiveVertAttributes(geom.Mesh);
            FillAttributeLists(geom.Mesh);

        }

        private void GetActiveVertAttributes(Grendgine_Collada_Mesh mesh)
        {
            foreach (Grendgine_Collada_Triangles tri in mesh.Triangles)
            {
                foreach (Grendgine_Collada_Input_Shared input in tri.Input)
                {
                    switch (input.Semantic)
                    {
                        case Grendgine_Collada_Input_Semantic.VERTEX:
                            if (!activeAttributes.Keys.Contains(VertexAttributes.Position))
                                activeAttributes.Add(VertexAttributes.Position, input.source.Remove(0, 1));
                            break;
                        case Grendgine_Collada_Input_Semantic.NORMAL:
                            if (!activeAttributes.Keys.Contains(VertexAttributes.Normal))
                                activeAttributes.Add(VertexAttributes.Normal, input.source.Remove(0, 1));
                            break;
                        case Grendgine_Collada_Input_Semantic.COLOR:
                            if (input.Set >= 2)
                            {
                                Console.WriteLine(string.Format("BMD only supports two Color attributes. Skipping color attribute [0].", input.Set));
                                break;
                            }
                            if (!activeAttributes.Keys.Contains(VertexAttributes.Color0 + input.Set))
                                activeAttributes.Add(VertexAttributes.Color0 + input.Set, input.source.Remove(0, 1));
                            break;
                        case Grendgine_Collada_Input_Semantic.TEXCOORD:
                            if (input.Set >= 9)
                            {
                                Console.Write(string.Format("BMD only supports 8 UV attributes. Skipping UV attribute [0].", input.Set));
                                break;
                            }
                            if (!activeAttributes.Keys.Contains(VertexAttributes.Tex0 + input.Set))
                                activeAttributes.Add(VertexAttributes.Tex0 + input.Set, input.source.Remove(0, 1));
                            break;
                    }
                }
            }
        }

        private void FillAttributeLists(Grendgine_Collada_Mesh mesh)
        {
            foreach (KeyValuePair<VertexAttributes, string> val in activeAttributes)
            {
                if (val.Key == VertexAttributes.Position)
                {
                    // This is a hack. It needs to be fixed because it is a hack.
                    // Should probably get the vertex positions directly from the vertex
                    // class in the mesh.
                    GetVertexPositions(mesh.Source.First(x => x.ID.Contains("POSITION")));
                }
                else if (val.Key == VertexAttributes.Normal)
                {
                    GetVertexNormals(mesh.Source.First(x => x.ID == val.Value));
                }
                else if (val.Key >= VertexAttributes.Tex0 && val.Key <= VertexAttributes.Tex7)
                {
                    GetVertexUVs(mesh.Source.First(x => x.ID == val.Value), val.Key);
                }
                else if (val.Key == VertexAttributes.Color0 || val.Key == VertexAttributes.Color1)
                {
                    GetVertexColors(mesh.Source.First(x => x.ID == val.Value), val.Key);
                }
            }
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

        private void GetVertexUVs(Grendgine_Collada_Source src, VertexAttributes uvAttrib)
        {
            string blah = src.Float_Array.Value_As_String.Replace('\n', ' ').Trim();
            float[] vertUV = Grendgine_Collada_Parse_Utils.String_To_Float(blah);

            List<Vector2> uvList = null;

            switch (uvAttrib)
            {
                case VertexAttributes.Tex0:
                    uvList = UV0;
                    break;
                case VertexAttributes.Tex1:
                    uvList = UV1;
                    break;
                case VertexAttributes.Tex2:
                    uvList = UV2;
                    break;
                case VertexAttributes.Tex3:
                    uvList = UV3;
                    break;
                case VertexAttributes.Tex4:
                    uvList = UV4;
                    break;
                case VertexAttributes.Tex5:
                    uvList = UV5;
                    break;
                case VertexAttributes.Tex6:
                    uvList = UV6;
                    break;
                case VertexAttributes.Tex7:
                    uvList = UV7;
                    break;
            }

            for (int i = 0; i < vertUV.Length; i += 2)
            {
                Vector2 uv = new Vector2(vertUV[i], vertUV[i + 1]);
                uvList.Add(uv);
            }
        }

        private void GetVertexColors(Grendgine_Collada_Source src, VertexAttributes colorAtr)
        {
            string blah = src.Float_Array.Value_As_String.Replace('\n', ' ').Trim();
            float[] vertCol = Grendgine_Collada_Parse_Utils.String_To_Float(blah);

            List<Vector3> colorList = Color0;

            if (colorAtr == VertexAttributes.Color1)
                colorList = Color1;

            for (int i = 0; i < vertCol.Length; i += 3)
            {
                Vector3 col = new Vector3(vertCol[i], vertCol[i + 1], vertCol[i + 2]);
                colorList.Add(col);
            }
        }
    }
}
