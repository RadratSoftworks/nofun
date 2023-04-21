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

        /// <summary>
        /// Save texture to a png file.
        /// 
        /// This function main use is for debugging purposes. Implementation may not implement it.
        /// </summary>
        /// <param name="path">Path to save the texture to.</param>
        void SaveToPng(string path);
    }
}