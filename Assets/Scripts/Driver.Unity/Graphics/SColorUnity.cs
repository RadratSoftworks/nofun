using Nofun.Driver.Graphics;
using UnityEngine;

namespace Nofun.Driver.Unity.Graphics
{
    public static class SColorUnity
    {
        public static Color ToUnityColor(this SColor colorOg)
        {
            return new Color(colorOg.r, colorOg.g, colorOg.b, colorOg.a);
        }
    }
}