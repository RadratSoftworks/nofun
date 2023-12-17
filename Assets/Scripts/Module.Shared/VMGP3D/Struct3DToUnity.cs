/*
 * (C) 2023 Radrat Softworks
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Nofun.Module.VMGP3D;
using Nofun.Util;
using UnityEngine;

namespace Nofun.Driver.Unity.Graphics
{
    public static class Struct3DToUnity
    {
        public static Vector2 ToUnity(this NativeUV uv)
        {
            return new Vector2(FixedUtil.Fixed9PointToFloat(uv.fixedU), FixedUtil.Fixed9PointToFloat(uv.fixedV));
        }

        public static Vector3 ToUnity(this NativeVector3D v)
        {
            return new Vector3(FixedUtil.FixedToFloat(v.fixedX), FixedUtil.FixedToFloat(v.fixedY), FixedUtil.FixedToFloat(v.fixedZ));
        }

        public static Vector4 ToUnity4D(this NativeVector3D v)
        {
            return new Vector4(FixedUtil.FixedToFloat(v.fixedX), FixedUtil.FixedToFloat(v.fixedY), FixedUtil.FixedToFloat(v.fixedZ), 1.0f);
        }

        public static Vector4 ToUnity(this NativeVector4D v)
        {
            return new Vector4(FixedUtil.FixedToFloat(v.fixedX), FixedUtil.FixedToFloat(v.fixedY), FixedUtil.FixedToFloat(v.fixedZ), FixedUtil.FixedToFloat(v.fixedW));
        }

        public static Color ToUnity(this NativeDiffuseColor color)
        {
            return new Color(color.r / 255.0f, color.g / 255.0f, color.b / 255.0f, color.a / 255.0f);
        }

        public static Color ToUnity(this NativeSpecularColor color)
        {
            return new Color(color.r / 255.0f, color.g / 255.0f, color.b / 255.0f, color.f / 255.0f);
        }

        public static Rect ToUnity(this NRectangle rect)
        {
            return new Rect(rect.x, rect.y, rect.width, rect.height);
        }

        public static NRectangle ToMophun(this Rect rect)
        {
            return new NRectangle((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
        }

        public static NativeVector3D ToMophun(this Vector3 v)
        {
            return new NativeVector3D()
            {
                fixedX = FixedUtil.FloatToFixed(v.x),
                fixedY = FixedUtil.FloatToFixed(v.y),
                fixedZ = FixedUtil.FloatToFixed(v.z),
            };
        }

        public static NativeVector4D ToMophun(this Vector4 v)
        {
            return new NativeVector4D()
            {
                fixedX = FixedUtil.FloatToFixed(v.x),
                fixedY = FixedUtil.FloatToFixed(v.y),
                fixedZ = FixedUtil.FloatToFixed(v.z),
                fixedW = FixedUtil.FloatToFixed(v.w),
            };
        }

        public static bool Intersects(this Rect r1, Rect r2, out Rect area)
        {
            area = new Rect();

            if (r2.Overlaps(r1))
            {
                area.x = Mathf.Max(r1.x, r2.x);
                area.y = Mathf.Max(r1.y, r2.y);

                float x2 = Mathf.Min(r1.max.x, r2.max.x);
                float y2 = Mathf.Min(r1.max.y, r2.max.y);

                area.width = x2 - area.x;
                area.height = y2 - area.y;

                return true;
            }

            return false;
        }
    }
}