namespace Nofun.Driver.Graphics
{
    public interface ITexture
    {
        int Width { get; }
        int Height { get; }
        int MipCount { get; }
        TextureFormat Format { get; }

        void SetData(byte[] data, int mipLevel);
        void Apply();
    }
}