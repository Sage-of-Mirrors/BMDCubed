using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using grendgine_collada;
using OpenTK;
using GameFormatReader.Common;
using BMDCubed.src.BMD.Skinning;

namespace BMDCubed.src.BMD.Geometry
{
    class GeometryManager
    {
        public VertexData VertexData;
        public BatchData BatchData;

        /// <summary>
        /// Creates an instance of GeometryManager and loads the geometry data from it.
        /// </summary>
        /// <param name="scene">Source of geometry data</param>
        /// <param name="position">Datatype to use for position data</param>
        /// <param name="normal">Datatype to use for normal data</param>
        /// <param name="uv">Datatype to use for UV data</param>
        /// <param name="color">Datatype to use for color data</param>
        public GeometryManager(Grendgine_Collada scene, DrawData drw1, Matrix4 bindShape, DataTypes position = DataTypes.F32, DataTypes normal = DataTypes.F32, 
            DataTypes uv = DataTypes.F32, ColorDataTypes color = ColorDataTypes.RGB8)
        {
            if (scene.Library_Geometries == null)
            {
                throw new FormatException("Mesh has no geometry!");
            }

            foreach (Grendgine_Collada_Geometry geom in scene.Library_Geometries.Geometry)
            {
                VertexData = new VertexData(geom.Mesh, bindShape, position, normal, uv, color);
                BatchData = new BatchData(geom.Mesh, drw1);
                BatchData.SetBoundingBoxes(VertexData.Positions);
            }
        }
    }
}
