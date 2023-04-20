namespace Nofun.Driver.Graphics
{
    public enum SystemFontStyle : uint
    {
        Normal = 0,
        Italic = 1,
        Bold = 2,
        Underline = 4,
        Monospace = 8,

        EffectMask = 0xF8000000U,

        OutlineEffect = 1 << 27,
        ShadowLowerRightEffect = 2 << 27,
        ShadowLowerLeftEffect = 3 << 27,
        ShadowUpperRightEffect = 4 << 27,
        ShadowUpperLeftEffect = 5 << 27
    }
}