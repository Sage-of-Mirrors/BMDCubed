using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DmitryBrant.ImageFormats;
using grendgine_collada;
using System.Drawing;
using BMDCubed.src.BMD.Geometry;

namespace BMDCubed.Materials
{
    class Material
    {
        public string Name;
        public byte Flag;
        public byte ColorChannelControlsCount;
        public byte NumTexGensCount;
        public byte NumTevStagesCount;
        public IndirectTexturing IndTexEntry;
        public GXCullMode CullMode;
        public Color?[] MaterialColors;
        public ChannelControl[] ChannelControls;
        public Color?[] AmbientColors;
        public Color?[] LightingColors;
        public TexCoordGen[] TexCoord1Gens;
        public TexCoordGen[] TexCoord2Gens;
        public TexMatrix[] TexMatrix1;
        public TexMatrix[] TexMatrix2;
        public BinaryTextureImage[] Textures;
        public TevOrder[] TevOrders;
        public GXKonstColorSel[] ColorSels;
        public GXKonstAlphaSel[] AlphaSels;
        public Color?[] TevColors;
        public Color?[] KonstColors;
        public TevStage[] TevStages;
        public TevSwapMode[] SwapModes;
        public TevSwapModeTable[] SwapTables;
        public Fog FogInfo;
        public AlphaCompare AlphCompare;
        public BlendMode BMode;
        public ZMode ZMode;
        public bool ZCompLoc;
        public bool Dither;

        public Batch MatBatch;

        public Material()
        {
            Name = "";
            Flag = 1;
            IndTexEntry = new IndirectTexturing();
            CullMode = GXCullMode.Back;
            MaterialColors = new Color?[2] { new Color(1, 1, 1, 1), new Color(1, 1, 1, 1) };
            ChannelControls = new ChannelControl[4]
            {
                new ChannelControl(false, (GXColorSrc)0, (GXLightId)0, (GXDiffuseFn)2, (GXAttenuationFn)1, (GXColorSrc)0),
                new ChannelControl(true, (GXColorSrc)0, (GXLightId)0, (GXDiffuseFn)1, (GXAttenuationFn)0, (GXColorSrc)0),
                new ChannelControl(false, (GXColorSrc)0, (GXLightId)0, (GXDiffuseFn)0, (GXAttenuationFn)2, (GXColorSrc)0),
                new ChannelControl(false, (GXColorSrc)1, (GXLightId)0, (GXDiffuseFn)2, (GXAttenuationFn)1, (GXColorSrc)0)
            };
            AmbientColors = new Color?[2] { new Color(0.1960f, 0.1960f, 0.1960f, 0.1960f), new Color(0, 0, 0, 0) };
            LightingColors = new Color?[8];
            TexCoord1Gens = new TexCoordGen[8];
            TexCoord2Gens = new TexCoordGen[8];
            TexMatrix1 = new TexMatrix[10];
            TexMatrix2 = new TexMatrix[20];
            Textures = new BinaryTextureImage[8];
            TevOrders = new TevOrder[16];
            ColorSels = new GXKonstColorSel[16];
            AlphaSels = new GXKonstAlphaSel[16];
            for (int i = 0; i < 16; i++)
            {
                ColorSels[i] = (GXKonstColorSel)0x0C;
                AlphaSels[i] = (GXKonstAlphaSel)0x1C;
            }
            TevColors = new Color?[4] { new Color(), new Color(), new Color(), new Color() };
            KonstColors = new Color?[4] { new Color(), new Color(), new Color(), new Color() };
            TevStages = new TevStage[16];
            TevStages[0] = new TevStage();
            SwapModes = new TevSwapMode[16];
            SwapModes[0] = new TevSwapMode();
            SwapTables = new TevSwapModeTable[4] { new TevSwapModeTable(), new TevSwapModeTable(), new TevSwapModeTable(), new TevSwapModeTable() };
            FogInfo = new Fog();
            AlphCompare = new AlphaCompare();
            BMode = new BlendMode();
            ZMode = new ZMode();
        }

        public Material(Grendgine_Collada_Phong source, Batch batch, string modelPath, Grendgine_Collada_Image[] textures)
        {
            MatBatch = batch;

            Name = batch.MaterialName;
            Flag = 1;
            IndTexEntry = new IndirectTexturing();
            CullMode = GXCullMode.Back;
            MaterialColors = new Color?[2] { new Color(1, 1, 1, 1), new Color(1, 1, 1, 1) };
            ChannelControls = new ChannelControl[4]
            {
                new ChannelControl(false, GXColorSrc.Register, GXLightId.None, GXDiffuseFn.Clamp, GXAttenuationFn.Spot, GXColorSrc.Register),
                new ChannelControl(false, GXColorSrc.Register, GXLightId.None, GXDiffuseFn.Clamp, GXAttenuationFn.Spot, GXColorSrc.Register),
                new ChannelControl(false, GXColorSrc.Register, GXLightId.None, GXDiffuseFn.Signed, GXAttenuationFn.Spec, GXColorSrc.Register),
                new ChannelControl(false, GXColorSrc.Register, GXLightId.None, GXDiffuseFn.None, GXAttenuationFn.None, GXColorSrc.Register),
            };
            AmbientColors = new Color?[2] { new Color(0.1960f, 0.1960f, 0.1960f, 0.1960f), new Color(0, 0, 0, 0) };
            LightingColors = new Color?[8];
            TexCoord1Gens = new TexCoordGen[8];
            TexCoord2Gens = new TexCoordGen[8];
            TexMatrix1 = new TexMatrix[10];
            TexMatrix2 = new TexMatrix[20];
            Textures = new BinaryTextureImage[8];
            TevOrders = new TevOrder[16];
            TevOrders[0] = new TevOrder(GXTexCoordSlot.Null, 0, GXColorChannelId.Color0A0);
            ColorSels = new GXKonstColorSel[16];
            AlphaSels = new GXKonstAlphaSel[16];
            for (int i = 0; i < 16; i++)
            {
                ColorSels[i] = GXKonstColorSel.KCSel_K0;
                AlphaSels[i] = GXKonstAlphaSel.KASel_K0_A;
            }
            TevColors = new Color?[4] { new Color(0, 0, 0, 1), new Color(1, 1, 1, 1), new Color(0, 0, 0, 0), new Color(1, 1, 1, 1) };
            KonstColors = new Color?[4] { new Color(1, 1, 1, 1), new Color(1, 1, 1, 1), new Color(1, 1, 1, 1), new Color(1, 1, 1, 1) };
            TevStages = new TevStage[16];
            TevStages[0] = new TevStage(new GXCombineColorInput[] { GXCombineColorInput.Zero, GXCombineColorInput.Zero, GXCombineColorInput.Zero, GXCombineColorInput.Zero },
                GXTevOp.Add, GXTevBias.Zero, GXTevScale.Scale_1, true, 0, new GXCombineAlphaInput[] { GXCombineAlphaInput.RasAlpha, GXCombineAlphaInput.Zero, GXCombineAlphaInput.Zero, GXCombineAlphaInput.Zero, },
                GXTevOp.Add, GXTevBias.Zero, GXTevScale.Scale_1, true, 0);
            SwapModes = new TevSwapMode[16];
            SwapModes[0] = new TevSwapMode();
            SwapModes[1] = new TevSwapMode();
            SwapTables = new TevSwapModeTable[4] { new TevSwapModeTable(0, 1, 2, 3), new TevSwapModeTable(0, 1, 2, 3), new TevSwapModeTable(0, 1, 2, 3), new TevSwapModeTable(0, 1, 2, 3) };
            FogInfo = new Fog();
            AlphCompare = new AlphaCompare(GXCompareType.Always, 0, GXAlphaOp.Or, GXCompareType.Always, 0);
            BMode = new BlendMode(GXBlendMode.None, GXBlendModeControl.One, GXBlendModeControl.Zero, GXLogicOp.Copy);
            ZMode = new ZMode(true, GXCompareType.LEqual, true);

            ZCompLoc = true;
            Dither = true;

            // Add texture to TEV stage data if there is one
            
            if (source.Diffuse.Texture != null)
            {
                string texName = source.Diffuse.Texture.Texture;
                string path = textures.First(x => x.ID == texName).Init_From.Replace("file://","");

                if (!Path.IsPathRooted(path))
                {
                    string modelDir = Path.GetDirectoryName(modelPath);
                    string texPath = string.Format("{0}\\{1}", modelDir, path);

                    if (File.Exists(texPath))
                        path = texPath;
                    else
                        throw new ArgumentException(string.Format("Could not find texture \"{0}\" at \"{1}\"!", path, texPath));
                }

                string imageExt = Path.GetExtension(path).ToLower();
                Bitmap imageData = null;

                switch (imageExt)
                {
                    case ".png":
                    case ".bmp":
                        imageData = new Bitmap(path);
                        break;
                    case ".tga":
                        imageData = TgaReader.Load(path);
                        break;
                    default:
                        throw new ArgumentException(string.Format("Texture {0} was a {1}. Only PNG, BMP, or TGA images are supported!", path, imageExt));
                }

                if (imageData == null)
                    throw new ArgumentException(string.Format("Texture {0} could not be loaded!", path));

                SetTexture(source.Diffuse.Texture, Path.GetFileNameWithoutExtension(path), imageData);
            }

            // Add vertex colors to the shader if there are any
            
            if (batch.ActiveAttributes.Contains(VertexAttributes.Color0))
            {
                ChannelControls = new ChannelControl[4]
                {
                    new ChannelControl(false, GXColorSrc.Vertex, GXLightId.None, GXDiffuseFn.Clamp, GXAttenuationFn.Spot, GXColorSrc.Register),
                    new ChannelControl(false, GXColorSrc.Vertex, GXLightId.None, GXDiffuseFn.Clamp, GXAttenuationFn.Spot, GXColorSrc.Register),
                    new ChannelControl(false, GXColorSrc.Register, GXLightId.None, GXDiffuseFn.Signed, GXAttenuationFn.Spec, GXColorSrc.Register),
                    new ChannelControl(false, GXColorSrc.Register, GXLightId.None, GXDiffuseFn.None, GXAttenuationFn.None, GXColorSrc.Register),
                };

                TevStages[1] = TevStages[0];
                TevStages[1].ColorIn[0] = GXCombineColorInput.ColorPrev;

                TevStages[0] = new TevStage(new GXCombineColorInput[] { GXCombineColorInput.C0, GXCombineColorInput.TexColor, GXCombineColorInput.RasColor, GXCombineColorInput.Zero },
                GXTevOp.Add, GXTevBias.Zero, GXTevScale.Scale_1, true, 0, new GXCombineAlphaInput[] { GXCombineAlphaInput.A0, GXCombineAlphaInput.TexAlpha, GXCombineAlphaInput.Zero, GXCombineAlphaInput.Zero, },
                GXTevOp.Add, GXTevBias.Zero, GXTevScale.Scale_1, true, 0);

                TevOrders[1] = new TevOrder(GXTexCoordSlot.TexCoord0, 0, GXColorChannelId.Color0A0);
            }

            foreach (ChannelControl chan in ChannelControls)
            {
                if (chan != null)
                    ColorChannelControlsCount++;
            }

            foreach (TexCoordGen gen in TexCoord1Gens)
            {
                if (gen != null)
                    NumTexGensCount++;
            }

            foreach (TevStage stage in TevStages)
            {
                if (stage != null)
                    NumTevStagesCount++;
            }
        }

        public void SetTexture(Grendgine_Collada_Texture sourceTex, string texName, Bitmap bmp)
        {
            BinaryTextureImage tex = null;

            // Check to see if we need an alpha channel in our texture
            bool hasAlpha = HasAlpha(bmp);

            // If so, we'll use RGB565
            if (hasAlpha)
            {
                tex = new BinaryTextureImage(texName, bmp, BinaryTextureImage.TextureFormats.RGBA32);

                // This sets the alpha compare settings so that alpha is set correctly.
                // It won't allow translucency, just a sharp transparent/opaque dichotemy.
                AlphCompare.Comp0 = GXCompareType.GEqual;
                AlphCompare.Comp1 = GXCompareType.LEqual;
                AlphCompare.Operation = GXAlphaOp.And;
                AlphCompare.Reference0 = 0x80;
                AlphCompare.Reference1 = 0xFF;

                // We'll default to showing the texture on both sides for now. Maybe it'll be changable in the future
                CullMode = GXCullMode.None;

                // Make sure z compare happens *after* texture look up so we can take the alpha compare results into account
                ZCompLoc = false;
            }
            // Otherwise, we'll use CMPR to save space
            else
            {
                tex = new BinaryTextureImage(texName, bmp, BinaryTextureImage.TextureFormats.CMPR);
            }

            tex.SetTextureSettings(sourceTex);
            CullMode = GXCullMode.None;

            // Search for an open texture slot and if there is one, put the texture there,
            // Include tex cooord gens,
            // Include a tex matrix,
            // And configure tev orders
            for (int i = 0; i < 8; i++)
            {
                if (Textures[i] == null)
                {
                    Textures[i] = tex;
                    TexCoord1Gens[i] = new TexCoordGen(GXTexGenType.Matrix2x4, GXTexGenSrc.Tex0 + i, GXTexMatrix.TexMtx0);
                    TexMatrix1[i] = new TexMatrix();

                    TevOrders[i].TexCoordId = GXTexCoordSlot.TexCoord0 + i;
                    TevOrders[i].TexMap = (byte)i;
                    TevOrders[i].ChannelId = GXColorChannelId.Color0A0;
                    break;
                }
            }

            TevStages[0].ColorIn[0] = GXCombineColorInput.TexColor;
            TevStages[0].ColorIn[1] = GXCombineColorInput.TexColor;
            TevStages[0].AlphaIn[0] = GXCombineAlphaInput.TexAlpha;
        }

        private bool HasAlpha(Bitmap source)
        {
            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    if (source.GetPixel(x, y).A != 255)
                        return true;
                }
            }

            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(Material))
                return Compare(obj as Material);
            else
                return false;
        }

        private bool Compare(Material obj)
        {
            if ((Name == obj.Name) && (Flag == obj.Flag)
                && (ColorChannelControlsCount == obj.ColorChannelControlsCount) && (NumTexGensCount == obj.NumTexGensCount)
                && (NumTevStagesCount == obj.NumTevStagesCount) /*&& (IndTexEntry == obj.IndTexEntry)*/)
                return true;
            else
                return false;
        }
    }
}
