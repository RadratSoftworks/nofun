using Nofun.Driver.Graphics;
using System;
using UnityEngine.Rendering;

namespace Nofun.Driver.Unity.Graphics
{
    public static class MpEnumUtils
    {
        public static Tuple<BlendMode, BlendMode> MpBlendModeToBlendFactors(MpBlendMode mode)
        {
            switch (mode)
            {
                case MpBlendMode.Replace:
                    return new Tuple<BlendMode, BlendMode>(BlendMode.One, BlendMode.Zero);

                case MpBlendMode.AlphaAdd:
                    return new Tuple<BlendMode, BlendMode>(BlendMode.SrcAlpha, BlendMode.One);

                case MpBlendMode.Alpha:
                    return new Tuple<BlendMode, BlendMode>(BlendMode.SrcAlpha, BlendMode.OneMinusSrcAlpha);

                case MpBlendMode.Modulate:
                    return new Tuple<BlendMode, BlendMode>(BlendMode.Zero, BlendMode.SrcColor);

                default:
                    throw new ArgumentException($"Unknown Mophun blend mode {mode}");
            }
        }

        public static CullMode MpCullModeToUnity(MpCullMode mode)
        {
            switch (mode)
            {
                case MpCullMode.None:
                    return CullMode.Off;

                case MpCullMode.Clockwise:
                    return CullMode.Front;

                case MpCullMode.CounterClockwise:
                    return CullMode.Back;

                default:
                    throw new ArgumentException($"Invalid cull mode: {mode}");
            }
        }

        public static CompareFunction MpCompareFunctionToUnity(MpCompareFunc func)
        {
            CompareFunction compareFuncNew = CompareFunction.LessEqual;
            switch (func)
            {
                case MpCompareFunc.Never:
                    compareFuncNew = CompareFunction.Never;
                    break;

                case MpCompareFunc.Less:
                    compareFuncNew = CompareFunction.Greater;
                    break;

                case MpCompareFunc.LessEqual:
                    compareFuncNew = CompareFunction.GreaterEqual;
                    break;

                case MpCompareFunc.Equal:
                    compareFuncNew = CompareFunction.Equal;
                    break;

                case MpCompareFunc.Greater:
                    compareFuncNew = CompareFunction.Less;
                    break;

                case MpCompareFunc.GreaterEqual:
                    compareFuncNew = CompareFunction.LessEqual;
                    break;

                case MpCompareFunc.NotEqual:
                    compareFuncNew = CompareFunction.NotEqual;
                    break;

                case MpCompareFunc.Always:
                    compareFuncNew = CompareFunction.Always;
                    break;

                default:
                    throw new ArgumentException($"Unknown depth function {func}");
            }

            return compareFuncNew;
        }
    }
}