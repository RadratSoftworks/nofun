/*
 * (C) 2023 Radrat Softworks
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Collections.Generic;
using System;
using Nofun.Module.VMGP3D;
using UnityEngine;
using Nofun.Util;

namespace Nofun.Driver.Graphics
{
    public interface IGraphicDriver
    {
        #region Resource manipulation
        ITexture CreateTexture(byte[] data, int width, int height, int mipCount, TextureFormat format, Memory<SColor> palettes = new Memory<SColor>(),
            bool zeroAsTransparent = false);
        #endregion

        #region General draw functions
        void ClearScreen(SColor color);
        void ClearDepth(float depth);
        void EndFrame();
        void FlipScreen();
        #endregion

        #region Text functions
        void DrawText(int posX, int posY, int sizeX, int sizeY, List<int> positions, ITexture atlas,
            TextDirection direction, SColor textColor);
        void DrawSystemText(short x0, short y0, string text, SColor backColor, SColor foreColor);
        void SelectSystemFont(uint fontSize, uint fontFlags, int charCodeShouldBeInFont);
        int GetStringExtentRelativeToSystemFont(string str);
        #endregion

        #region 2D general draw
        void DrawLine(int x0, int y0, int x1, int y1, SColor lineColor);
        void DrawTexture(int posX, int posY, int centerX, int centerY, int rotation, ITexture texture,
            int sourceX = -1, int sourceY = -1, int width = -1, int height = -1, bool blackAsTransparent = false,
            bool flipX = false, bool flipY = false);
        void FillRect(int x0, int y0, int x1, int y1, SColor color);
        #endregion

        #region 3D draw
        void DrawBillboard(NativeBillboard billboard);
        void DrawPrimitives(MpMesh meshToDraw);
        #endregion

        #region State manipulation
        MpCullMode Cull { get; set; }
        MpCompareFunc DepthFunction { get; set; }
        MpBlendMode ColorBufferBlend { get; set; }
        bool TextureMode { get; set; }
        NRectangle ClipRect { get; set; }
        NRectangle Viewport { get; set; }

        Matrix4x4 ProjectionMatrix3D { get; set;}
        Matrix4x4 ViewMatrix3D { get; set; }

        ITexture MainTexture { get; set; }
        #endregion

        /// Other properties
        int ScreenWidth { get; }
        int ScreenHeight { get; }
    };
}