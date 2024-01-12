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

using System;
using Nofun.Module.VMGPCaps;

namespace Nofun.Settings
{
    [Serializable]
    public struct GameSetting
    {
        public ScreenMode screenMode;
        public int screenSizeX;
        public int screenSizeY;
        public SystemDeviceModel deviceModel;
        public bool enableSoftwareScissor;
        public int fps;
        public ScreenOrientation orientation;
        public SystemVersion systemVersion;
        public CPUBackend cpuBackend;
    }
}
