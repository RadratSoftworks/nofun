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

using Nofun.Driver.Graphics;
using Nofun.Util;
using Nofun.Util.Logging;
using Nofun.VM;

namespace Nofun.Module.VMGP3D
{
    [Module]
    public partial class VMGP3D
    {
        private SColor fogColour;

        [ModuleCall]
        private void vSetMaterial2(VMPtr<NativeMaterial2> materialPtr)
        {
            NativeMaterial2 materialCopy = materialPtr.Read(system.Memory);

            system.GraphicDriver.Material = new MpExtendedMaterial()
            {
                ambient = materialCopy.ambient,
                diffuse = materialCopy.diffuse,
                specular = materialCopy.specular,
                emission = materialCopy.emission,
                shininess = FixedUtil.FixedToFloat(materialCopy.fixedShininess)
            };
        }

        [ModuleCall]
        private void vSetMaterial(VMPtr<NativeMaterial> materialPtr)
        {
            NativeMaterial materialLegacyCopy = materialPtr.Read(system.Memory);

            system.GraphicDriver.Material = new MpExtendedMaterial()
            {
                ambient = new SColor(1.0f, 1.0f, 1.0f, 1.0f),
                diffuse = materialLegacyCopy.diffuse,
                specular = materialLegacyCopy.specular,
                emission = new SColor(0.0f, 0.0f, 0.0f, 0.0f),
                shininess = 4.0f
            };
        }

        [ModuleCall]
        private void vResetLights()
        {
            system.GraphicDriver.ClearLights();
        }

        [ModuleCall]
        private void vSetFogColor(uint colour)
        {
            fogColour = SColor.FromRgb888(colour);
        }

        [ModuleCall]
        private void vSetLight(int index, VMPtr<NativeLight> lightPtr)
        {
            if (index < 0 || index >= system.GraphicDriver.MaxLights)
            {
                Logger.Error(LogClass.VMGP3D, $"Invalid light index: {index}");
                return;
            }

            NativeLight lightCopy = lightPtr.Read(system.Memory);
            MpLightSourceType sourceType = (MpLightSourceType)lightCopy.type;

            if ((sourceType != MpLightSourceType.Point) && (sourceType != MpLightSourceType.Spot) && (sourceType != MpLightSourceType.Directional))
            {
                Logger.Error(LogClass.VMGP3D, $"Invalid light source type: {sourceType}");
                return;
            }

            MpLight lightDriver = new MpLight()
            {
                pos = lightCopy.pos,
                dir = lightCopy.dir,
                lightSourceType = sourceType,
                diffuse = new SColor(lightCopy.r / 255.0f, lightCopy.g / 255.0f, lightCopy.b / 255.0f),
                specular = new SColor(lightCopy.sr / 255.0f, lightCopy.sg / 255.0f, lightCopy.sb / 255.0f),
                lightRange = FixedUtil.FixedToFloat(lightCopy.fixedRange),
                exponent = lightCopy.exponent,
                cutoff = (float)(FixedUtil.Fixed11PointToFloat((short)lightCopy.cutoff) * FullCircleRads)
            };

            if (!system.GraphicDriver.SetLight(index, lightDriver))
            {
                Logger.Error(LogClass.VMGP3D, $"Failed to set light {index}");
            }
        }

        [ModuleCall]
        private void vSetAmbientLight(uint colour)
        {
            system.GraphicDriver.GlobalAmbient = SColor.FromRgb888(colour);
        }
    }
}