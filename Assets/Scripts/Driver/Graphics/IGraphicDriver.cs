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

namespace Nofun.Driver.Graphics
{
    public interface IGraphicDriver
    {
        ITexture CreateTexture(byte[] data, int width, int height, int mipCount, TextureFormat format, Span<SColor> palettes = new Span<SColor>());

        void DrawText(int posX, int posY, int sizeX, int sizeY, List<int> positions, ITexture atlas,
            TextDirection direction, SColor textColor);

        void ClearScreen(SColor color);

        void EndFrame();

        void FlipScreen();

        void SetClipRect(int x0, int y0, int x1, int y1);
        void GetClipRect(out int x0, out int y0, out int x1, out int y1);

        void DrawTexture(int posX, int posY, int centerX, int centerY, int rotation, ITexture texture,
            int sourceX = -1, int sourceY = -1, int width = -1, int height = -1, bool blackAsTransparent = false);
        
        void FillRect(int x0, int y0, int x1, int y1, SColor color);

        void DrawSystemText(short x0, short y0, string text, SColor backColor, SColor foreColor);

        void SelectSystemFont(uint fontSize, uint fontFlags, int charCodeShouldBeInFont);

        int GetStringExtentRelativeToSystemFont(string str);

        void SetViewport(int left, int top, int width, int height);

        int ScreenWidth { get; }

        int ScreenHeight { get; }
    };
}