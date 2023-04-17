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

        int ScreenWidth { get; }

        int ScreenHeight { get; }
    };
}