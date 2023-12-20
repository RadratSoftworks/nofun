using Nofun.Driver.Graphics;

namespace Nofun.Module.VMGP3D
{
    public static class NativeExtension
    {
        public static SColor ToSColor(this NativeDiffuseColor color)
        {
            return new SColor(color.r / 255.0f, color.g / 255.0f, color.b / 255.0f, color.a / 255.0f);
        }

        public static SColor ToSColor(this NativeSpecularColor color)
        {
            return new SColor(color.r / 255.0f, color.g / 255.0f, color.b / 255.0f, color.f / 255.0f);
        }
    }
}