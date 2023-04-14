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
    };
}