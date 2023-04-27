using Nofun.Module.VMGP3D;
using Nofun.Util;

using UnityEngine;

namespace Nofun.Driver.Unity.Graphics
{
    public static class Struct3DToUnity
    {
        public static Vector2 MophunUVToUnity(NativeUV uv)
        {
            return new Vector2(FixedUtil.Fixed9PointToFloat(uv.fixedU), FixedUtil.Fixed9PointToFloat(uv.fixedV));
        }

        public static Vector3 MophunVector3ToUnity(NativeVector3D v)
        {
            return new Vector3(FixedUtil.FixedToFloat(v.fixedX), FixedUtil.FixedToFloat(v.fixedY), FixedUtil.FixedToFloat(v.fixedZ));
        }

        public static Color MophunDColorToUnity(NativeDiffuseColor color)
        {
            return new Color(color.r / 255.0f, color.g / 255.0f, color.b / 255.0f, color.a / 255.0f);
        }
    }
}