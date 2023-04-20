using Nofun.Driver.Graphics;
using Nofun.Util;
using UnityEngine;

using System.Collections.Generic;
using System;
using UnityEngine.UI;

namespace Nofun.Driver.Unity.Graphics
{
    public class GraphicDriver : MonoBehaviour, IGraphicDriver
    {
        private readonly int NGAGE_PPI = 130;

        private RenderTexture screenTexture;
        private RenderTexture screenTexture2;
        private RenderTexture currentScreenTexture;
        private bool began = false;
        private Action stopProcessor;
        private Rect scissorRect;
        private Texture2D whiteTexture;
        private Vector2 screenSize;

        [SerializeField]
        private TMPro.TMP_Text textRender;

        [SerializeField]
        private UnityEngine.UI.RawImage displayImage;

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

        public void Initialize(Vector2 size)
        {
            screenTexture = new RenderTexture((int)size.x, (int)size.y, 32);
            screenTexture2 = new RenderTexture((int)size.x, (int)size.y, 32);
            currentScreenTexture = screenTexture;

            SetupWhiteTexture();

            displayImage.texture = screenTexture;
            this.screenSize = size;
        }

        private void Start()
        {
            Initialize(new Vector2(176, 208));
        }

        /// <summary>
        /// Get draw rectangle in unity coordinates.
        /// </summary>
        /// <param name="x">The bottom-left screen coordinates X.</param>
        /// <param name="y">The bottom-left screen coordinates Y.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        /// <returns></returns>
        private Rect GetUnityScreenRect(int x, int y, int width, int height)
        {
            return new Rect(x, screenSize.y - y, width, height);
        }

        private Vector2 GetUnityCoords(float x, float y)
        {
            return new Vector2(x, screenSize.y - y);
        }

        private void BeginRender()
        {
            if (began)
            {
                return;
            }

            began = true;
            UnityEngine.Graphics.SetRenderTarget(currentScreenTexture);

            GL.PushMatrix();
            GL.LoadPixelMatrix(0, currentScreenTexture.width, 0, currentScreenTexture.height);
        }

        public void EndFrame()
        {
            if (began)
            {
                GL.PopMatrix();
                UnityEngine.Graphics.SetRenderTarget(null);

                began = false;
            }
        }

        public void ClearScreen(SColor color)
        {
            BeginRender();
            GL.Clear(false, true, color.ToUnityColor());
        }

        public ITexture CreateTexture(byte[] data, int width, int height, int mipCount, Driver.Graphics.TextureFormat format)
        {
            return new Texture(data, width, height, mipCount, format);
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
            Color textColorUed = textColor.ToUnityColor();

            float sizeXNormed = (float)sizeX / nativeTex.width;
            float sizeYNormed = (float)sizeY / nativeTex.height;

            for (int i = 0; i < positions.Count; i += 2)
            {
                Rect destRect = GetUnityScreenRect(posX, posY, sizeX, sizeY);
                Rect sourceRect = new Rect((float)positions[i] / nativeTex.width, (float)positions[i + 1] / nativeTex.height, sizeXNormed, sizeYNormed);

                UnityEngine.Graphics.DrawTexture(destRect, nativeTex, sourceRect, 0, 0, 0, 0, textColorUed);

                posX += advX;
                posY += advY;
            }
        }

        public void DrawTexture(int posX, int posY, int centerX, int centerY, int rotation, ITexture texture)
        {
            BeginRender();

            // The position in its original form, non-rotation is (posX - centerX, posY - centerY),
            // while the center (pivot) is actually (posX, posY)
            // We translate the matrix to pivot and rotate, and later translate back, it's the standard
            Vector3 relatePosition = GetUnityCoords(posX, posY);

            Matrix4x4 modelMatrix = Matrix4x4.TRS(relatePosition, Quaternion.AngleAxis(rotation, Vector3.forward),
                Vector3.one) * Matrix4x4.Translate(-relatePosition);

            GL.PushMatrix();
            GL.MultMatrix(modelMatrix);

            Rect destRect = GetUnityScreenRect(posX - centerX, posY + centerY, texture.Width, texture.Height);
            UnityEngine.Graphics.DrawTexture(destRect, ((Texture)texture).NativeTexture);

            GL.PopMatrix();
        }

        public void FillRect(int x0, int y0, int x1, int y1, SColor color)
        {
            Rect destRect = GetUnityScreenRect(x0, y1, x1 - x0, y1 - y0);
            Rect sourceRect = new Rect(0, 0, 1, 1);

            UnityEngine.Graphics.DrawTexture(destRect, whiteTexture, sourceRect, 0, 0, 0, 0, color.ToUnityColor());
        }

        public void FlipScreen()
        {
            displayImage.texture = currentScreenTexture;

            if (currentScreenTexture == screenTexture)
            {
                currentScreenTexture = screenTexture2;
            }
            else
            {
                currentScreenTexture = screenTexture;
            }

            // Exit the processing so that Update can begin
            stopProcessor();
        }

        public void SetClipRect(ushort x0, ushort y0, ushort x1, ushort y1)
        {
            scissorRect = new Rect(x0, y0, x1 - x0, y1 - y0);
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

            textRender.fontSize = fontSizeNew;

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
                textRender.outlineWidth = 0.2f;
            }
            else
            {
                textRender.outlineWidth = 0.0f;
            }

            textRender.fontStyle = styles;
        }

        public void DrawSystemText(short x0, short y0, string text, SColor backColor, SColor foreColor)
        {
            BeginRender();

            textRender.text = text;
            textRender.color = foreColor.ToUnityColor();
            textRender.outlineColor = backColor.ToUnityColor();

            LayoutRebuilder.ForceRebuildLayoutImmediate(textRender.rectTransform);

            textRender.ForceMeshUpdate();

            Matrix4x4 modelMatrix = Matrix4x4.TRS(GetUnityCoords(x0, y0), Quaternion.identity, Vector3.one);

            Material textMaterial = textRender.fontSharedMaterial;
            bool canRender = textMaterial.SetPass(0);

            if (canRender)
            {
                UnityEngine.Graphics.DrawMeshNow(textRender.mesh, modelMatrix);
            }
        }

        public int GetStringExtentRelativeToSystemFont(string value)
        {
            Vector2 size = textRender.GetPreferredValues(value);
            if (value == " ")
            {
                size.x = textRender.fontSize / 4.0f;
            }
            return (ushort)Math.Round(size.x) | ((ushort)Math.Round(size.y) << 16);
        }

        public int ScreenWidth => (int)screenSize.x;

        public int ScreenHeight => (int)screenSize.y;
    } 
}