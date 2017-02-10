using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;
using BMDCubed.src.BMD.Geometry;
using BMDCubed.src;
using grendgine_collada;

namespace BMDCubed.Materials
{
    class MaterialManager
    {
        const string padString = "This is padding data to align.p";
        /// <summary>
        /// A list of all the unique indirect texturing entries within the model.
        /// </summary>
        private List<IndirectTexturing> m_indirectTexBlock;
        /// <summary>
        /// A list of all the unique cull modes used by the materials.
        /// </summary>
        private List<GXCullMode> m_cullModeBlock;
        /// <summary>
        /// A list of all the unique material colors used by the materials.
        /// </summary>
        private List<Color?> m_materialColorBlock;
        /// <summary>
        /// A list of all the unique channel controls used by the materials.
        /// </summary>
        private List<ChannelControl> m_channelControlBlock;
        /// <summary>
        /// A list of all the unique material colors used by the materials.
        /// </summary>
        private List<Color?> m_ambientColorBlock;
        /// <summary>
        /// A list of all the unique material colors used by the materials.
        /// </summary>
        private List<Color?> m_lightingColorBlock;
        /// <summary>
        /// A list of all the unique texture coordinate 1 generation info entries used by the materials.
        /// </summary>
        private List<TexCoordGen> m_texCoord1GenBlock;
        /// <summary>
        /// A list of all the unique texture coordinate 2 generation info entries used by the materials.
        /// </summary>
        private List<TexCoordGen> m_texCoord2GenBlock;
        /// <summary>
        /// A list of all the unique texture matrix 1 entries used by the materials.
        /// </summary>
        private List<TexMatrix> m_texMatrix1Block;
        /// <summary>
        /// A list of all the unique texture matrix 2 entries used by the materials.
        /// </summary>
        private List<TexMatrix> m_texMatrix2Block;
        /// <summary>
        /// A list of all the unique textures used by the materials. Mainly used for getting indexes.
        /// </summary>
        public List<BinaryTextureImage> TextureList;
        /// <summary>
        /// A list of all the unique tev order entries used by the materials.
        /// </summary>
        private List<TevOrder> m_tevOrderBlock;
        /// <summary>
        /// A list of all the unique TEV colors used by the materials.
        /// </summary>
        private List<Color?> m_tevColorBlock;
        /// <summary>
        /// A list of all the unique TEV konst colors used by the materials.
        /// </summary>
        private List<Color?> m_tevKonstColorBlock;
        /// <summary>
        /// A list of all the unique TEV stage configs used by the materials.
        /// </summary>
        private List<TevStage> m_tevStageBlock;
        /// <summary>
        /// A list of all the unique TEV swap modes used by the materials.
        /// </summary>
        private List<TevSwapMode> m_swapModeBlock;
        /// <summary>
        /// A list of all the unique TEV swap mode tables used by the materials.
        /// </summary>
        private List<TevSwapModeTable> m_swapTableBlock;
        /// <summary>
        /// A list of all the unique fog settings used by the materials.
        /// </summary>
        private List<Fog> m_fogBlock;
        /// <summary>
        /// A list of all the unique alpha compare settings used by the materials.
        /// </summary>
        private List<AlphaCompare> m_alphaCompBlock;
        /// <summary>
        ///  A list of all the unique blend modes used by the materials.
        /// </summary>
        private List<BlendMode> m_blendModeBlock;
        // A list of all the Zmodes used by the materials.
        private List<ZMode> m_zModeBlock;
        // A list of all the unique ZCompLocs used by the materials.
        private List<bool> m_zCompLocBlock;
        // A list of all the unique Dithers used by the materials.
        private List<bool> m_ditherBlock;
        /// <summary>
        /// A list of all the materials within the model.
        /// </summary>
        public List<Material> Materials;

        private List<byte> NumColorChannelsBlock;
        private List<byte> NumTexGensBlock;
        private List<byte> NumTevSTagesBlock;

        public MaterialManager()
        {
            m_indirectTexBlock = new List<IndirectTexturing>();
            m_cullModeBlock = new List<GXCullMode>();
            m_materialColorBlock = new List<Color?>();
            m_channelControlBlock = new List<ChannelControl>();
            m_ambientColorBlock = new List<Color?>();
            m_lightingColorBlock = new List<Color?>();
            m_texCoord1GenBlock = new List<TexCoordGen>();
            m_texCoord2GenBlock = new List<TexCoordGen>();
            m_texMatrix1Block = new List<TexMatrix>();
            m_texMatrix2Block = new List<TexMatrix>();
            TextureList = new List<BinaryTextureImage>();
            m_tevOrderBlock = new List<TevOrder>();
            m_tevColorBlock = new List<Color?>();
            m_tevKonstColorBlock = new List<Color?>();
            m_tevStageBlock = new List<TevStage>();
            m_swapModeBlock = new List<TevSwapMode>();
            m_swapTableBlock = new List<TevSwapModeTable>();
            m_fogBlock = new List<Fog>();
            m_alphaCompBlock = new List<AlphaCompare>();
            m_blendModeBlock = new List<BlendMode>();
            m_zModeBlock = new List<ZMode>();
            m_zCompLocBlock = new List<bool>();
            m_ditherBlock = new List<bool>();

            Materials = new List<Material>();
        }

        public MaterialManager(Grendgine_Collada scene, List<Batch> batches)
        {
            #region List initialization
            m_indirectTexBlock = new List<IndirectTexturing>();
            m_cullModeBlock = new List<GXCullMode>();
            m_materialColorBlock = new List<Color?>();
            m_channelControlBlock = new List<ChannelControl>();
            m_ambientColorBlock = new List<Color?>();
            m_lightingColorBlock = new List<Color?>();
            m_texCoord1GenBlock = new List<TexCoordGen>();
            m_texCoord2GenBlock = new List<TexCoordGen>();
            m_texMatrix1Block = new List<TexMatrix>();
            m_texMatrix2Block = new List<TexMatrix>();
            TextureList = new List<BinaryTextureImage>();
            m_tevOrderBlock = new List<TevOrder>();
            m_tevColorBlock = new List<Color?>();
            m_tevKonstColorBlock = new List<Color?>();
            m_tevStageBlock = new List<TevStage>();
            m_swapModeBlock = new List<TevSwapMode>();
            m_swapTableBlock = new List<TevSwapModeTable>();
            m_fogBlock = new List<Fog>();
            m_alphaCompBlock = new List<AlphaCompare>();
            m_blendModeBlock = new List<BlendMode>();
            m_zModeBlock = new List<ZMode>();
            m_zCompLocBlock = new List<bool>();
            m_ditherBlock = new List<bool>();
            NumColorChannelsBlock = new List<byte>();
            NumTexGensBlock = new List<byte>();
            NumTevSTagesBlock = new List<byte>();

            Materials = new List<Material>();
            #endregion

            Grendgine_Collada_Material[] GrendgineMaterials = scene.Library_Materials.Material;
            Grendgine_Collada_Effect[] Effects = scene.Library_Effects.Effect;
            Grendgine_Collada_Image[] Images = scene.Library_Images.Image;

            foreach (Batch bat in batches)
            {
                Grendgine_Collada_Material batMat = GrendgineMaterials.First(x => x.Name == bat.MaterialName);
                Grendgine_Collada_Effect batEffect = Effects.First(x => x.ID == batMat.Instance_Effect.URL.Remove(0,1));
                Grendgine_Collada_Phong phong = batEffect.Profile_COMMON[0].Technique.Phong;

                Material mat = new Material(phong, bat, "", Images);
                Materials.Add(mat);
            }
        }
        /// <summary>
        /// Writes the MAT3 section to the specified stream.
        /// </summary>
        /// <param name="writer">Stream to write to.</param>
        public void WriteMAT3(EndianBinaryWriter writer)
        {
            PopulateBlockLists();

            // Write header
            WriteHeader(writer);

            // Write materials
            foreach (Material mat in Materials)
                WriteMaterialIndexes(writer, mat);

            // Write index offset
            WriteOffset(writer, 0x10);

            // Write material indexes
            for (int i = 0; i < Materials.Count; i++)
                writer.Write((short)i);

            // Write string table offset
            WriteOffset(writer, 0x14);

            // Write string table
            WriteStringTable(writer);

            Pad32(writer);

            // Write indirect texturing offset
            WriteOffset(writer, 0x18);

            // Write indirect texturing block
            foreach (IndirectTexturing ind in m_indirectTexBlock)
                ind.Write(writer);

            if (m_cullModeBlock.Count != 0)
            {
                // Write cull mode offset
                WriteOffset(writer, 0x1C);

                // Write cull mode block
                foreach (GXCullMode cull in m_cullModeBlock)
                    writer.Write((int)cull);
            }


            //Pad32(writer);

            if (m_materialColorBlock.Count != 0)
            {
                // Write material colors offset
                WriteOffset(writer, 0x20);

                // Write material color block
                foreach (Color col in m_materialColorBlock)
                    WriteColor(writer, col);
            }

            //Pad32(writer);

            // Write number of channel controls offset
            WriteOffset(writer, 0x24);

            foreach (byte by in NumColorChannelsBlock)
                writer.Write(by);

            Pad32(writer);

            if (m_channelControlBlock.Count != 0)
            {
                // Write channel control data offset
                WriteOffset(writer, 0x28);

                // Write channel control block
                foreach (ChannelControl chan in m_channelControlBlock)
                    chan.Write(writer);
            }

            //Pad32(writer);

            if (m_ambientColorBlock.Count != 0)
            {
                // Write ambient colors offset
                WriteOffset(writer, 0x2C);

                //Write ambient color block
                foreach (Color col in m_ambientColorBlock)
                    WriteColor(writer, col);
            }

            //Pad32(writer);

            // Write lighting colors offset
            // Writing this here because for whatever reason the game prefers to have an offset rather than 0
            WriteOffset(writer, 0x30);

            if (m_lightingColorBlock.Count != 0)
            {
                // Write lighting color block
                foreach (Color col in m_lightingColorBlock)
                    WriteColor(writer, col);
            }

            //Pad32(writer);

            // Write numTexCoord1Gen offset
            WriteOffset(writer, 0x34);

            foreach (byte by in NumTexGensBlock)
                writer.Write(by);

            Pad32(writer);

            // Write tex coord 1 gen block
            if (m_texCoord1GenBlock.Count != 0)
            {
                // Write TexCoord1Gen offset
                WriteOffset(writer, 0x38);

                // Write tex coord gens 1 block
                foreach (TexCoordGen gen in m_texCoord1GenBlock)
                    gen.Write(writer);

                //Pad32(writer);
            }

            // Write tex coord 2 gen block
            if (m_texCoord2GenBlock.Count != 0)
            {
                // Write numTexCoord2Gen offset
                //WriteOffset(writer, 0x3C);

                // Write numTexCoord2Gen

                // Write TexCoord2Gen offset
                WriteOffset(writer, 0x3C);

                // Write tex coord gens 2 block
                foreach (TexCoordGen gen in m_texCoord2GenBlock)
                    gen.Write(writer);

                //Pad32(writer);
            }

            if (m_texMatrix1Block.Count != 0)
            {
                // Write tex matrix 1 offset
                WriteOffset(writer, 0x40);

                // Write tex matrix 1 block
                foreach (TexMatrix mat in m_texMatrix1Block)
                    mat.Write(writer);

               // Pad32(writer);
            }

            if (m_texMatrix2Block.Count != 0)
            {
                // Write tex matrix 2 offset
                WriteOffset(writer, 0x44);

                // Write tex matrix 2 block
                foreach (TexMatrix mat in m_texMatrix2Block)
                    mat.Write(writer);

                //Pad32(writer);
            }

            if (TextureList.Count != 0)
            {
                // Write texture index offset
                WriteOffset(writer, 0x48);

                // Write texture index block
                for (int i = 0; i < TextureList.Count; i++)
                    writer.Write((short)i);
            }

            Pad32(writer);

            if (m_tevOrderBlock.Count != 0)
            {
                // Write tev order offset
                WriteOffset(writer, 0x4C);

                // Write tev order block
                foreach (TevOrder order in m_tevOrderBlock)
                    order.Write(writer);
            }

            //Pad32(writer);

            if (m_tevColorBlock.Count != 0)
            {
                // Write tev color offset
                WriteOffset(writer, 0x50);

                // Write tev color block
                foreach (Color col in m_tevColorBlock)
                    WriteShortColor(writer, col);
            }

            //Pad32(writer);

            if (m_tevKonstColorBlock.Count != 0)
            {
                // Write tev konst color offset
                WriteOffset(writer, 0x54);

                // Write konst color block
                foreach (Color col in m_tevKonstColorBlock)
                    WriteColor(writer, col);
            }

            //Pad32(writer);
            
            // Write num tev stage offset in header here
            WriteOffset(writer, 0x58);

            foreach (byte by in NumTevSTagesBlock)
                writer.Write(by);

            Pad32(writer);

            if (m_tevStageBlock.Count != 0)
            {
                // Write tev stage offset
                WriteOffset(writer, 0x5C);

                // Write tev stage block
                foreach (TevStage stage in m_tevStageBlock)
                    stage.Write(writer);
            }

            //Pad32(writer);

            if (m_swapModeBlock.Count != 0)
            {
                // Write swap mode offset
                WriteOffset(writer, 0x60);

                // Write tev swap mode block
                foreach (TevSwapMode mode in m_swapModeBlock)
                    mode.Write(writer);
            }

            //Pad32(writer);

            if (m_swapTableBlock.Count != 0)
            {
                // Write swap table offset
                WriteOffset(writer, 0x64);

                // Write tev swap table block
                foreach (TevSwapModeTable table in m_swapTableBlock)
                    table.Write(writer);
            }

            //Pad32(writer);

            if (m_fogBlock.Count != 0)
            {
                // Write fog offset
                WriteOffset(writer, 0x68);

                // Write fog block
                foreach (Fog fg in m_fogBlock)
                    fg.Write(writer);
            }

            //Pad32(writer);

            if (m_alphaCompBlock.Count != 0)
            {
                // Write alpha compare offset
                WriteOffset(writer, 0x6C);

                // Write alpha compare block
                foreach (AlphaCompare alph in m_alphaCompBlock)
                    alph.Write(writer);
            }

            //Pad32(writer);

            if (m_blendModeBlock.Count != 0)
            {
                // Write blend mode offset
                WriteOffset(writer, 0x70);

                // Write blend mode block
                foreach (BlendMode mode in m_blendModeBlock)
                    mode.Write(writer);
            }

            //Pad32(writer);

            if (m_zModeBlock.Count != 0)
            {
                // Write z mode offset
                WriteOffset(writer, 0x74);

                // Write zmode block
                foreach (ZMode mode in m_zModeBlock)
                    mode.Write(writer);
            }

            //Pad32(writer);

            if (m_zCompLocBlock.Count != 0)
            {
                // Write z comp loc offset
                WriteOffset(writer, 0x78);

                // Write zcomploc block
                foreach (bool bol in m_zCompLocBlock)
                    writer.Write(bol);

                for (int i = 0; i < 4 - m_zCompLocBlock.Count; i++)
                {
                    writer.Write(padString[i]);
                }
            }

            if (m_ditherBlock.Count != 0)
            {
                // Write dither offset
                WriteOffset(writer, 0x7C);

                // Write dither block
                foreach (bool bol in m_ditherBlock)
                    writer.Write(bol);

                for (int i = 0; i < 4 - m_ditherBlock.Count; i++)
                {
                    writer.Write(padString[i]);
                }
            }

            WriteOffset(writer, 0x80);

            NBTScale test = new NBTScale();

            test.Write(writer);

            Pad32(writer);

            // Write section size
            WriteOffset(writer, 0x4);
        }
        /// <summary>
        /// Pulls all of the unique fields from each material into lists for easy indexing and output.
        /// </summary>
        private void PopulateBlockLists()
        {
            foreach (Material mat in Materials)
            {
                m_indirectTexBlock.Add(mat.IndTexEntry);

                if (!m_cullModeBlock.Contains(mat.CullMode))
                    m_cullModeBlock.Add(mat.CullMode);

                if (!NumColorChannelsBlock.Contains(mat.ColorChannelControlsCount))
                    NumColorChannelsBlock.Add(mat.ColorChannelControlsCount);

                if (!NumTexGensBlock.Contains(mat.NumTexGensCount))
                    NumTexGensBlock.Add(mat.NumTexGensCount);

                if (!NumTevSTagesBlock.Contains(mat.NumTevStagesCount))
                    NumTevSTagesBlock.Add(mat.NumTevStagesCount);

                // Material colors
                for (int i = 0; i < 2; i++)
                {
                    if (!m_materialColorBlock.Contains(mat.MaterialColors[i]))
                        m_materialColorBlock.Add(mat.MaterialColors[i]);
                }
                // Channel controls
                for (int i = 0; i < 4; i++)
                {
                    if (!m_channelControlBlock.Contains(mat.ChannelControls[i]))
                        m_channelControlBlock.Add(mat.ChannelControls[i]);
                }
                // Ambient colors
                for (int i = 0; i < 2; i++)
                {
                    if (!m_ambientColorBlock.Contains(mat.AmbientColors[i]))
                        m_ambientColorBlock.Add(mat.AmbientColors[i]);
                }
                // Lighting colors
                for (int lit = 0; lit < 8; lit++)
                {
                    if ((mat.LightingColors[lit] != null) && (!m_lightingColorBlock.Contains(mat.LightingColors[lit])))
                        m_lightingColorBlock.Add(mat.LightingColors[lit]);
                }
                // Tex coord gens 1
                for (int gen = 0; gen < 8; gen++)
                {
                    if ((mat.TexCoord1Gens[gen] != null) && (!m_texCoord1GenBlock.Contains(mat.TexCoord1Gens[gen])))
                        m_texCoord1GenBlock.Add(mat.TexCoord1Gens[gen]);
                }
                // Tex coord gens 2
                for (int gen = 0; gen < 8; gen++)
                {
                    if ((mat.TexCoord2Gens[gen] != null) && (!m_texCoord2GenBlock.Contains(mat.TexCoord2Gens[gen])))
                        m_texCoord2GenBlock.Add(mat.TexCoord2Gens[gen]);
                }
                // Texture matrices 1
                for (int tex = 0; tex < 10; tex++)
                {
                    if ((mat.TexMatrix1[tex] != null) && (!m_texMatrix1Block.Contains(mat.TexMatrix1[tex])))
                        m_texMatrix1Block.Add(mat.TexMatrix1[tex]);
                }
                // Texture matrices 2
                for (int tex = 0; tex < 20; tex++)
                {
                    if ((mat.TexMatrix2[tex] != null) && (!m_texMatrix2Block.Contains(mat.TexMatrix2[tex])))
                        m_texMatrix2Block.Add(mat.TexMatrix2[tex]);
                }
                // Textures
                for (int text = 0; text < 8; text++)
                {
                    if ((mat.Textures[text] != null) && (!TextureList.Contains(mat.Textures[text])))
                        TextureList.Add(mat.Textures[text]);
                }
                // Tev orders
                for (int i = 0; i < 16; i++)
                {
                    if ((mat.TevOrders[i] != null) && (!m_tevOrderBlock.Contains(mat.TevOrders[i])))
                        m_tevOrderBlock.Add(mat.TevOrders[i]);
                }
                // Tev colors
                for (int i = 0; i < 4; i++)
                {
                    if (!m_tevColorBlock.Contains(mat.TevColors[i]))
                        m_tevColorBlock.Add(mat.TevColors[i]);
                }
                // Tev konst colors
                for (int i = 0; i < 4; i++)
                {
                    if (!m_tevKonstColorBlock.Contains(mat.KonstColors[i]))
                        m_tevKonstColorBlock.Add(mat.KonstColors[i]);
                }
                // Tev stages
                for (int i = 0; i < 16; i++)
                {
                    if ((mat.TevStages[i] != null) && (!m_tevStageBlock.Contains(mat.TevStages[i])))
                        m_tevStageBlock.Add(mat.TevStages[i]);
                }
                // Tev swap modes
                for (int i = 0; i < 16; i++)
                {
                    if ((mat.SwapModes[i] != null) && (!m_swapModeBlock.Contains(mat.SwapModes[i])))
                        m_swapModeBlock.Add(mat.SwapModes[i]);
                }
                // Tev swap tables
                for (int i = 0; i < 4; i++)
                {
                    if ((mat.SwapTables[i] != null) && (!m_swapTableBlock.Contains(mat.SwapTables[i])))
                        m_swapTableBlock.Add(mat.SwapTables[i]);
                }
                // Fog
                if (!m_fogBlock.Contains(mat.FogInfo))
                    m_fogBlock.Add(mat.FogInfo);
                // Alpha compare
                if (!m_alphaCompBlock.Contains(mat.AlphCompare))
                    m_alphaCompBlock.Add(mat.AlphCompare);
                // Blend mode
                if (!m_blendModeBlock.Contains(mat.BMode))
                    m_blendModeBlock.Add(mat.BMode);
                // Z mode
                if (!m_zModeBlock.Contains(mat.ZMode))
                    m_zModeBlock.Add(mat.ZMode);
                // Z comp loc
                m_zCompLocBlock.Add(false);
                m_zCompLocBlock.Add(true);
                // Dither
                m_ditherBlock.Add(false);
                m_ditherBlock.Add(true);
            }
        }
        /// <summary>
        /// Outputs the specified color to the specified stream.
        /// </summary>
        private void WriteColor(EndianBinaryWriter writer, Color col)
        {
            writer.Write((byte)(col.R * 255f));
            writer.Write((byte)(col.G * 255f));
            writer.Write((byte)(col.B * 255f));
            writer.Write((byte)(col.A * 255f));
        }
        private void WriteShortColor(EndianBinaryWriter writer, Color col)
        {
            writer.Write((short)(col.R * 255f));
            writer.Write((short)(col.G * 255f));
            writer.Write((short)(col.B * 255f));
            writer.Write((short)(col.A * 255f));
        }
        /// <summary>
        /// Outputs the specified material to the stream.
        /// </summary>
        private void WriteMaterialIndexes(EndianBinaryWriter writer, Material mat)
        {
            // Flag
            writer.Write(mat.Flag);
            // Cull mode
            writer.Write((byte)m_cullModeBlock.IndexOf(mat.CullMode));
            // NumChannelControls
            writer.Write((byte)NumColorChannelsBlock.IndexOf(mat.ColorChannelControlsCount));
            // NumTexGens
            writer.Write((byte)NumTexGensBlock.IndexOf(mat.NumTexGensCount));
            // NumTevStages
            writer.Write((byte)NumTevSTagesBlock.IndexOf(mat.NumTevStagesCount));
            // ZCompLoc
            writer.Write((byte)m_zCompLocBlock.IndexOf(mat.ZCompLoc));
            // ZMode
            writer.Write((byte)m_zModeBlock.IndexOf(mat.ZMode));
            // Dither
            writer.Write((byte)m_ditherBlock.IndexOf(mat.Dither));
            // Material color
            for (int i = 0; i < 2; i++)
            {
                writer.Write((short)m_materialColorBlock.IndexOf(mat.MaterialColors[0]));
            }
            // Channel controls
            for (int i = 0; i < 4; i++)
            {
                writer.Write((short)m_channelControlBlock.IndexOf(mat.ChannelControls[i]));
            }
            // Ambient color
            for (int i = 0; i < 2; i++)
            {
                writer.Write((short)m_ambientColorBlock.IndexOf(mat.AmbientColors[i]));
            }
            // Lighting color
            for (int i = 0; i < 8; i++)
            {
                if (mat.LightingColors[i] == null)
                    writer.Write((short)-1);
                else
                    writer.Write((short)m_lightingColorBlock.IndexOf(mat.LightingColors[i]));
            }
            // Tex coord gens 1
            for (int i = 0; i < 8; i++)
            {
                if (mat.TexCoord1Gens[i] == null)
                    writer.Write((short)-1);
                else
                    writer.Write((short)m_texCoord1GenBlock.IndexOf(mat.TexCoord1Gens[i]));
            }
            // Tex coord gens 2
            for (int i = 0; i < 8; i++)
            {
                if (mat.TexCoord2Gens[i] == null)
                    writer.Write((short)-1);
                else
                    writer.Write((short)m_texCoord2GenBlock.IndexOf(mat.TexCoord2Gens[i]));
            }
            // Tex matrix 1
            for (int i = 0; i < 10; i++)
            {
                if (mat.TexMatrix1[i] == null)
                    writer.Write((short)-1);
                else
                    writer.Write((short)m_texMatrix1Block.IndexOf(mat.TexMatrix1[i]));
            }
            // Tex matrix 2
            for (int i = 0; i < 20; i++)
            {
                if (mat.TexMatrix2[i] == null)
                    writer.Write((short)-1);
                else
                    writer.Write((short)m_texMatrix2Block.IndexOf(mat.TexMatrix2[i]));
            }
            // Texture indices
            for (int i = 0; i < 8; i++)
            {
                if (mat.Textures[i] == null)
                    writer.Write((short)-1);
                else
                    writer.Write((short)TextureList.IndexOf(mat.Textures[i]));
            }
            // TevKonstColors
            for (int i = 0; i < 4; i++)
            {
                if (mat.KonstColors[i] == null)
                    writer.Write((short)-1);
                else
                    writer.Write((short)m_tevKonstColorBlock.IndexOf(mat.KonstColors[i]));
            }
            // Konst Color Sels
            for (int i = 0; i < 16; i++)
            {
                writer.Write((byte)mat.ColorSels[i]);
            }
            // Konst Alpha Sels
            for (int i = 0; i < 16; i++)
            {
                writer.Write((byte)mat.AlphaSels[i]);
            }
            // Tev order info
            for (int i = 0; i < 16; i++)
            {
                if (mat.TevOrders[i] == null)
                    writer.Write((short)-1);
                else
                    writer.Write((short)m_tevOrderBlock.IndexOf(mat.TevOrders[i]));
            }
            // Tev Color
            for (int i = 0; i < 4; i++)
            {
                if (mat.TevColors[i] == null)
                    writer.Write((short)-1);
                else
                    writer.Write((short)m_tevColorBlock.IndexOf(mat.TevColors[i]));
            }
            // Tev Stage Info
            for (int i = 0; i < 16; i++)
            {
                if (mat.TevStages[i] == null)
                    writer.Write((short)-1);
                else
                    writer.Write((short)m_tevStageBlock.IndexOf(mat.TevStages[i]));
            }
            // TevSwapModes
            for (int i = 0; i < 16; i++)
            {
                if (mat.SwapModes[i] == null)
                    writer.Write((short)-1);
                else
                    writer.Write((short)m_swapModeBlock.IndexOf(mat.SwapModes[i]));
            }
            // TevSwapModeTables
            for (int i = 0; i < 4; i++)
            {
                if (mat.SwapTables[i] == null)
                    writer.Write((short)-1);
                else
                    writer.Write((short)m_swapTableBlock.IndexOf(mat.SwapTables[i]));
            }
            // Unknown indexes
            for (int i = 0; i < 12; i++)
            {
                writer.Write((short)0);
            }
            // FogIndex
            writer.Write((short)m_fogBlock.IndexOf(mat.FogInfo));
            // AlphaCompare
            writer.Write((short)m_alphaCompBlock.IndexOf(mat.AlphCompare));
            // BlendMode
            writer.Write((short)m_blendModeBlock.IndexOf(mat.BMode));
            // Unknown1
            writer.Write((short)0);
        }
        /// <summary>
        /// Outputs the MAT3 header to the stream.
        /// </summary>
        private void WriteHeader(EndianBinaryWriter writer)
        {
            // Tag
            writer.Write("MAT3".ToCharArray());
            // Placeholder for section size
            writer.Write((int)0);
            // Material count
            writer.Write((short)Materials.Count);
            //Padding
            writer.Write((short)-1);
            // Material offset
            writer.Write((int)0x84);
            // Placeholder offets for the rest of the data
            for (int i = 0; i < 29; i++)
            {
                writer.Write((int)0);
            }
        }
        /// <summary>
        /// Outputs the material string table to the stream.
        /// </summary>
        private void WriteStringTable(EndianBinaryWriter writer)
        {
            // String count
            writer.Write((short)Materials.Count);
            // Pad to 4 bytes
            writer.Write((short)-1);

            short nameOffset = (short)(4 + (Materials.Count * 4));

            foreach (Material mat in Materials)
            {
                writer.Write((short)HashName(mat.Name));
                writer.Write(nameOffset);
                nameOffset += (short)(mat.Name.Length + 1);
            }

            foreach (Material mat in Materials)
            {
                writer.Write(mat.Name.ToCharArray());
                writer.Write((byte)0);
            }
        }
        /// <summary>
        /// Writes the current length of the specified stream to the specified offset and returns the stream to the
        /// end.
        /// </summary>
        /// <param name="writer">Stream to write to.</param>
        /// <param name="offset">Offset to write to.</param>
        private void WriteOffset(EndianBinaryWriter writer, int offset)
        {
            writer.BaseStream.Seek(offset, 0);
            writer.Write((int)writer.BaseStream.Length);
            writer.BaseStream.Seek(writer.BaseStream.Length, 0);
        }

        private static ushort HashName(string name)
        {
            short hash = 0;

            short multiplier = 1;

            if (name.Length + 1 == 2)
            {
                multiplier = 2;
            }

            if (name.Length + 1 >= 3)
            {
                multiplier = 3;
            }

            foreach (char c in name)
            {
                hash = (short)(hash * multiplier);
                hash += (short)c;
            }

            return (ushort)hash;
        }

        private static void Pad32(EndianBinaryWriter writer)
        {
            // Pad up to a 4 byte alignment
            // Formula: (x + (n-1)) & ~(n-1)
            long nextAligned = (writer.BaseStream.Length + 0x1F) & ~0x1F;

            long delta = nextAligned - writer.BaseStream.Length;
            writer.BaseStream.Position = writer.BaseStream.Length;
            for (int i = 0; i < delta; i++)
            {
                writer.Write(padString[i]);
            }
        }

        public void WriteTEX1(EndianBinaryWriter writer, List<BinaryTextureImage> textures)
        {
            // Base address for the start of the BTI header data for each image
            int imageHeadersBase = 0x20;

            writer.Write("TEX1".ToCharArray()); // Chunk tag, "TEX1"
            writer.Write((int)0); // Placeholder for chunk size
            writer.Write((short)textures.Count); // Texture count
            writer.Write(ushort.MaxValue); // Padding
            writer.Write((int)imageHeadersBase);
            writer.Write((int)0); // Placeholder for string table offset

            Util.PadStreamWithString(writer, 32);

            foreach (BinaryTextureImage tex in textures)
                tex.WriteHeader(writer);

            // Write image data offsets and image data
            for (int i = 0; i < textures.Count; i++)
            {
                int curHeaderOffset = imageHeadersBase + (i * 0x20);
                uint imageDataOffset = (uint)(writer.BaseStream.Length - curHeaderOffset);

                // 0x20 is the TEX1 header size, 0x0C is the offset to paletteDataOffset,
                // i * 0x20 is the current header
                writer.Seek(imageHeadersBase + 0x0C + (i * 0x20), 0);
                writer.Write((uint)(imageDataOffset - (i * 0x20)));

                // 0x20 is the TEX1 header size, 0x1C is the offset to the imageDataOffset,
                // i * 0x20 is the current header
                writer.Seek(imageHeadersBase + 0x1C + (i * 0x20), 0);
                writer.Write((uint)imageDataOffset);

                // Write actual data
                writer.Seek((int)writer.BaseStream.Length, 0);

                //imageDataOffset = (int)writer.BaseStream.Length - 0x20;
                writer.Write(textures[i].GetData());
            }

            // Write string table offset
            writer.Seek(0x10, 0);
            writer.Write((uint)writer.BaseStream.Length);
            writer.Seek((int)writer.BaseStream.Length, 0);

            // Write string table
            writer.Write((ushort)textures.Count);
            writer.Write((short)-1);

            ushort stringOffset = (ushort)(4 + ((textures.Count) * 4));

            // Hash and string offset
            foreach (BinaryTextureImage tex in textures)
            {
                writer.Write((ushort)HashName(tex.Name));
                writer.Write(stringOffset);

                stringOffset += (ushort)(tex.Name.Length + 1);
            }

            // String data with null terminators
            foreach (BinaryTextureImage tex in textures)
            {
                writer.Write(tex.Name.ToCharArray());
                writer.Write((byte)0);
            }

            Util.PadStreamWithString(writer, 32);

            writer.Seek(4, 0);
            writer.Write((uint)writer.BaseStream.Length);
            writer.Seek(0, System.IO.SeekOrigin.End);
        }
    }
}
