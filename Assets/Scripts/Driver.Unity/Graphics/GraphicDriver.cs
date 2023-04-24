using Nofun.Driver.Graphics;
using Nofun.Util;
using UnityEngine;

using System.Collections.Generic;
using System;

using UnityEngine.UI;
using UnityEngine.Rendering;

namespace Nofun.Driver.Unity.Graphics
{
    public class GraphicDriver : MonoBehaviour, IGraphicDriver
    {
        private readonly int NGAGE_PPI = 130;
        private const string BlackTransparentUniformName = "_Black_Transparent";
        private const string MainTextUniformName = "_MainTex";
        private const string TexcoordTransformMatrixUniformName = "_TexToLocal";
        private const string ColorUniformName = "_Color";

        private RenderTexture screenTextureBackBuffer;
        private RenderTexture screenTextureBackup;

        private bool began = false;
        private Action stopProcessor;
        private Rect scissorRect;
        private Rect viewportRect;
        private Texture2D whiteTexture;
        private Vector2 screenSize;
        private float frameTimePassed = -1.0f;

        private TMPro.FontStyles selectedFontStyles;
        private float selectedFontSize = 11.5f;
        private float selectedOutlineWidth = 0.0f;
        private int fontMeshUsed = 0;
        private bool useStagingScreen = false;

        [SerializeField]
        private TMPro.TMP_Text[] textRenders;

        [SerializeField]
        private TMPro.TMP_Text textMeasure;

        [SerializeField]
        private UnityEngine.UI.RawImage displayImage;

        [SerializeField]
        private Material mophunDrawTextureMaterial;

        private CommandBuffer commandBuffer;
        private Mesh quadMesh;

        [HideInInspector]
        public float FpsLimit { get; set; }

        private float SecondPerFrame => 1.0f / FpsLimit;

        public Action StopProcessorAction
        {
            set => stopProcessor = value;
        }

        private void SetupWhiteTexture()
        {
            whiteTexture = new Texture2D(1, 1);
            whiteTexture.SetPixel(0, 0, Color.white);
            whiteTexture.Apply();
        }

        private void SetupQuadMesh()
        {
            quadMesh = new Mesh();

            Vector3[] vertices = new Vector3[4]
            {
                new Vector3(0, 0, 0),
                new Vector3(0, 1, 0),
                new Vector3(1, 1, 0),
                new Vector3(1, 0, 0)
            };

            quadMesh.vertices = vertices;

            int[] indicies =
            {
                0, 1, 2,
                0, 2, 3
            };

            quadMesh.triangles = indicies;

            Vector2[] uv = new Vector2[4]
            {
                new Vector2(0, 1),
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(1, 1)
            };

            quadMesh.uv = uv;
        }

        private void ForceFlush()
        {
            if (!useStagingScreen)
            {
                useStagingScreen = true;

                if (screenTextureBackup == null)
                {
                    screenTextureBackup = new RenderTexture((int)screenSize.x, (int)screenSize.y, 32);
                }

                UnityEngine.Graphics.Blit(screenTextureBackBuffer, screenTextureBackup);
                displayImage.texture = screenTextureBackup;
            }

            UnityEngine.Graphics.ExecuteCommandBuffer(commandBuffer);
            commandBuffer.Clear();

            fontMeshUsed = 0;
            began = false;
        }

        public void Initialize(Vector2 size)
        {
            screenTextureBackBuffer = new RenderTexture((int)size.x, (int)size.y, 32);
            scissorRect = new Rect(0, 0, size.x, size.y);
            scissorRect = new Rect(0, 0, viewportRect.x, viewportRect.y);

            SetupWhiteTexture();
            SetupQuadMesh();

            displayImage.texture = screenTextureBackBuffer;
            this.screenSize = size;
        }

        private void Start()
        {
            Initialize(new Vector2(176, 208));
        }

        private Rect GetUnityScreenRect(Rect curRect)
        {
            return new Rect(curRect.x, screenSize.y - (curRect.y + curRect.height), curRect.width, curRect.height);
        }

        private Vector2 GetUnityCoords(float x, float y)
        {
            return new Vector2(x, screenSize.y - y);
        }

        private void DrawTexture(ITexture tex, Rect destRect, Rect sourceRect, float centerX, float centerY, float rotation, SColor color, bool blackIsTransparent)
        {
            DrawTexture(((Texture)tex).NativeTexture, destRect, sourceRect, centerX, centerY, rotation, color, blackIsTransparent);
        }

        private void DrawTexture(Texture2D tex, Rect destRect, Rect sourceRect, float centerX, float centerY, float rotation, SColor color, bool blackIsTransparent)
        {
            var coordMatrix = Matrix4x4.TRS(new Vector3(sourceRect.x, sourceRect.y, 0.0f), Quaternion.identity, new Vector3(sourceRect.width, sourceRect.height));

            Matrix4x4 drawMatrix;

            if (rotation == 0.0f)
            {
                drawMatrix = Matrix4x4.TRS(new Vector3(destRect.x - centerX, ScreenHeight - (destRect.y - centerY + destRect.height), 0),
                    Quaternion.identity, new Vector3(destRect.width, destRect.height, 0));
            }
            else
            {
                drawMatrix = Matrix4x4.Translate(new Vector3(destRect.x - centerX, ScreenHeight - (destRect.y - centerY + destRect.height), 0));
                drawMatrix *= Matrix4x4.TRS(new Vector3(centerX, -centerY), Quaternion.AngleAxis(rotation, Vector3.forward), new Vector3(destRect.width, destRect.height, 0));
                drawMatrix = Matrix4x4.Translate(new Vector3(-centerX, centerY));
            }

            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetTexture(MainTextUniformName, tex);
            block.SetFloat(BlackTransparentUniformName, blackIsTransparent ? 1.0f : 0.0f);
            block.SetMatrix(TexcoordTransformMatrixUniformName, coordMatrix);
            block.SetColor(ColorUniformName, color.ToUnityColor());

            commandBuffer.DrawMesh(quadMesh, drawMatrix, mophunDrawTextureMaterial, 0, 0, block);
        }

        private void BeginRender()
        {
            if (began)
            {
                return;
            }

            if (commandBuffer == null)
            {
                commandBuffer = new CommandBuffer();
                commandBuffer.name = "Mophun render buffer";
            }

            commandBuffer.SetRenderTarget(screenTextureBackBuffer);
            commandBuffer.SetViewport(GetUnityScreenRect(viewportRect));
            
            if (scissorRect.size != Vector2.zero)
            {
                commandBuffer.EnableScissorRect(GetUnityScreenRect(scissorRect));
            }

            commandBuffer.SetViewMatrix(Matrix4x4.identity);
            commandBuffer.SetProjectionMatrix(Matrix4x4.Ortho(0, ScreenWidth, 0, ScreenHeight, 1, -100));

            began = true;
        }

        public void EndFrame()
        {
            if (began)
            {
                began = false;
            }
        }

        public void ClearScreen(SColor color)
        {
            BeginRender();

            commandBuffer.ClearRenderTarget(false, true, color.ToUnityColor());
        }

        public ITexture CreateTexture(byte[] data, int width, int height, int mipCount, Driver.Graphics.TextureFormat format, Span<SColor> palettes = new Span<SColor>())
        {
            return new Texture(data, width, height, mipCount, format, palettes);
        }

        public void DrawText(int posX, int posY, int sizeX, int sizeY, List<int> positions, ITexture atlas, TextDirection direction, SColor textColor)
        {
            BeginRender();

            if (positions.Count % 2 != 0)
            {
                throw new ArgumentException("The list of bound values are not aligned by 2!");
            }

            // Just draw them one by one, verticies this small should not be instancing.
            // Batching them would be cool though :)
            int advX = (direction == TextDirection.Horizontal) ? sizeX : 0;
            int advY = (direction == TextDirection.VerticalUp) ? -sizeY : (direction == TextDirection.VerticalDown) ? sizeY : 0;

            Texture2D nativeTex = ((Texture)atlas).NativeTexture;

            float sizeXNormed = (float)sizeX / nativeTex.width;
            float sizeYNormed = (float)sizeY / nativeTex.height;

            for (int i = 0; i < positions.Count; i += 2)
            {
                Rect destRect = new Rect(posX, posY, sizeX, sizeY);
                Rect sourceRect = new Rect((float)positions[i] / nativeTex.width, (float)positions[i + 1] / nativeTex.height, sizeXNormed, sizeYNormed);

                DrawTexture(nativeTex, destRect, sourceRect, 0, 0, 0, textColor, false);

                posX += advX;
                posY += advY;
            }
        }

        public void DrawTexture(int posX, int posY, int centerX, int centerY, int rotation, ITexture texture,
            int sourceX = -1, int sourceY = -1, int width = -1, int height = -1, bool blackIsTransparent = false)
        {
            BeginRender();

            int widthToUse = (width == -1) ? texture.Width : width;
            int heightToUse = (height == -1) ? texture.Height : height;

            Rect destRect = new Rect(posX, posY, widthToUse, heightToUse);
            Rect sourceRect = new Rect(0, 0, 1, 1);

            if ((sourceX != -1) && (sourceY != -1))
            {
                sourceRect = new Rect((float)sourceX / texture.Width, (float)sourceY / texture.Height, (float)widthToUse / texture.Width, (float)heightToUse / texture.Height);
            }

            DrawTexture(texture, destRect, sourceRect, centerX, centerY, rotation, new SColor(1, 1, 1), blackIsTransparent);
        }

        public void FillRect(int x0, int y0, int x1, int y1, SColor color)
        {
            Rect destRect = new Rect(x0, y0, x1 - x0, y1 - y0);
            Rect sourceRect = new Rect(0, 0, 1, 1);

            DrawTexture(whiteTexture, destRect, sourceRect, 0, 0, 0, color, false);
        }

        public void FlipScreen()
        {
            UnityEngine.Graphics.ExecuteCommandBuffer(commandBuffer);
            commandBuffer.Clear();

            frameTimePassed = 0.0f;
            useStagingScreen = false;

            displayImage.texture = screenTextureBackBuffer;

            // Exit the processing so that Update can begin
            stopProcessor();
        }

        public bool FrameFlipFinishedEmulating()
        {
            if (frameTimePassed < 0.0f)
            {
                return true;
            }

            frameTimePassed += UnityEngine.Time.deltaTime;
            if (frameTimePassed >= SecondPerFrame)
            {
                frameTimePassed = -1.0f;
                return true;
            }

            return false;
        }

        public void SetClipRect(int x0, int y0, int x1, int y1)
        {
            Rect previousRect = scissorRect;
            scissorRect = new Rect(x0, y0, x1 - x0, y1 - y0);
        
            if (previousRect != scissorRect)
            {
                if (began)
                {
                    commandBuffer.EnableScissorRect(GetUnityScreenRect(scissorRect));
                }
            }
        }

        public void SetViewport(int left, int top, int width, int height)
        {
            Rect previousRect = viewportRect;
            viewportRect = new Rect(left, top, width, height);

            if (previousRect != viewportRect)
            {
                if (began)
                {
                    commandBuffer.SetViewport(GetUnityScreenRect(viewportRect));
                }
            }
        }

        public void GetClipRect(out int x0, out int y0, out int x1, out int y1)
        {
            x0 = (int)scissorRect.xMin;
            x1 = (int)scissorRect.xMax;
            y0 = (int)scissorRect.yMin;
            y1 = (int)scissorRect.yMax;
        }

        private float EmulatedPointToPixels(float point)
        {
            return point * NGAGE_PPI / 72.0f;
        }

        public void SelectSystemFont(uint fontSize, uint fontFlags, int charCodeShouldBeInFont)
        {
            // TODO: Use the character hint
            float fontSizeNew = fontSize;

            if (BitUtil.FlagSet(fontSize, SystemFontSize.PixelFlag))
            {
                fontSizeNew = (fontSize & ~(uint)SystemFontSize.PixelFlag);
            }
            else if (BitUtil.FlagSet(fontSize, SystemFontSize.PointFlag))
            {
                fontSizeNew = (fontSize & ~(uint)SystemFontSize.PointFlag);
                fontSizeNew = EmulatedPointToPixels(fontSize);
            }
            else
            {
                switch ((SystemFontSize)fontSize)
                {
                    case SystemFontSize.Large:
                        {
                            fontSizeNew = 22;
                            break;
                        }

                    case SystemFontSize.Normal:
                        {
                            fontSizeNew = 17;
                            break;
                        }

                    case SystemFontSize.Small:
                        {
                            fontSizeNew = 11.5f;
                            break;
                        }
                }
            }

            selectedFontSize = fontSizeNew;

            TMPro.FontStyles styles = TMPro.FontStyles.Normal;

            if (BitUtil.FlagSet(fontFlags, SystemFontStyle.Bold))
            {
                styles |= TMPro.FontStyles.Bold;
            }

            if (BitUtil.FlagSet(fontFlags, SystemFontStyle.Italic))
            {
                styles |= TMPro.FontStyles.Italic;
            }

            if (BitUtil.FlagSet(fontFlags, SystemFontStyle.Underline))
            {
                styles |= TMPro.FontStyles.Underline;
            }

            if (BitUtil.FlagSet(fontFlags, SystemFontStyle.OutlineEffect))
            {
                selectedOutlineWidth = 0.2f;
            }
            else
            {
                selectedOutlineWidth = 0.0f;
            }

            selectedFontStyles = styles;

            textMeasure.fontSize = selectedFontSize;
            textMeasure.fontStyle = selectedFontStyles;
            textMeasure.outlineWidth = selectedOutlineWidth;
        }

        public void DrawSystemText(short x0, short y0, string text, SColor backColor, SColor foreColor)
        {
            if (fontMeshUsed >= textRenders.Length)
            {
                ForceFlush();
            }

            BeginRender();

            TMPro.TMP_Text textRender = textRenders[fontMeshUsed];

            textRender.text = text;
            textRender.color = foreColor.ToUnityColor();
            textRender.fontStyle = selectedFontStyles;
            textRender.outlineWidth = selectedOutlineWidth;
            textRender.fontSize = selectedFontSize;
            textRender.outlineColor = backColor.ToUnityColor();

            LayoutRebuilder.ForceRebuildLayoutImmediate(textRender.rectTransform);
            textRender.ForceMeshUpdate();

            Matrix4x4 modelMatrix = Matrix4x4.TRS(GetUnityCoords(x0, y0), Quaternion.identity, Vector3.one);

            commandBuffer.DrawMesh(textRender.mesh, modelMatrix, textRender.fontSharedMaterial);
            fontMeshUsed++;
        }

        public int GetStringExtentRelativeToSystemFont(string value)
        {
            textMeasure.fontStyle = selectedFontStyles;
            textMeasure.outlineWidth = selectedOutlineWidth;
            textMeasure.fontSize = selectedFontSize;

            Vector2 size = textMeasure.GetPreferredValues(value);
            if (value == " ")
            {
                size.x = textMeasure.fontSize / 4.0f;
            }

            return (ushort)Math.Round(size.x) | ((ushort)Math.Round(size.y) << 16);
        }

        public int ScreenWidth => (int)screenSize.x;

        public int ScreenHeight => (int)screenSize.y;
    } 
}