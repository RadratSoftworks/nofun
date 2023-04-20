using System.Collections.Generic;

namespace Nofun.Driver.Graphics
{
    public interface IGraphicDriver
    {
        ITexture CreateTexture(byte[] data, int width, int height, int mipCount, TextureFormat format);

        void DrawText(int posX, int posY, int sizeX, int sizeY, List<int> positions, ITexture atlas,
            TextDirection direction, SColor textColor);

        void ClearScreen(SColor color);

        void EndFrame();

        void FlipScreen();

        void SetClipRect(ushort x0, ushort y0, ushort x1, ushort y1);

        void DrawTexture(int posX, int posY, int centerX, int centerY, int rotation, ITexture texture);
        
        void FillRect(int x0, int y0, int x1, int y1, SColor color);

        void DrawSystemText(short x0, short y0, string text, SColor backColor, SColor foreColor);

        void SelectSystemFont(uint fontSize, uint fontFlags, int charCodeShouldBeInFont);

        int GetStringExtentRelativeToSystemFont(string str);

        int ScreenWidth { get; }

        int ScreenHeight { get; }
    };
}