using Nofun.Driver.Graphics;
using UnityEngine;

using System.Collections.Generic;
using System;

namespace Nofun.Driver.Unity.Graphics
{
    public class GraphicDriver : IGraphicDriver
    {
        private RenderTexture screenTexture;
        private bool began = false;
        private Action stopProcessor;

        public RenderTexture DisplayResult => screenTexture;

        public Action StopProcessorAction
        {
            set => stopProcessor = value;
        }

        public GraphicDriver(Vector2 size)
        {
            screenTexture = new RenderTexture((int)size.x, (int)size.y, 32);
        }

        private Rect GetUnityScreenRect(int x, int y, int width, int height)
        {
            return new Rect(x, screenTexture.height - y, width, height);
        }

        private void BeginRender()
        {
            if (began)
            {
                return;
            }

            began = true;
            UnityEngine.Graphics.SetRenderTarget(screenTexture);

            GL.PushMatrix();
            GL.LoadPixelMatrix(0, screenTexture.width, 0, screenTexture.height);
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

        public void FlipScreen()
        {
            // Exit the processing so that Update can begin
            stopProcessor();
        }

        public int ScreenWidth => screenTexture.width;

        public int ScreenHeight => screenTexture.height;
    } 
}