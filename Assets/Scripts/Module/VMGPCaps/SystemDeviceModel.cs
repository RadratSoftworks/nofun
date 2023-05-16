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

namespace Nofun.Module.VMGPCaps
{
    public enum SystemDeviceModel : uint
    {
        // For Unknown model
        Unknown = 0,
        UnknownUnix = 1,
        UnknownWindows = 2,
        UnknownPocketPC = 3,

        // For other vendor combination
        SonyEricssonT300 = 0,
        SonyEricssonT310 = 1,
        SonyEricssonT610 = 2,
        Nokia7650 = 3,
        SonyErricssonP800 = 4,
        SonyErricssonT226 = 5,
        MotorolaA920 = 6,
        NokiaNgage = 7,
        Nokia3650 = 8,
        TigerTelematicGametrac = 9,
        SonyErricisonP900 = 10,
        Nokia6600 = 11,
        MotorolaA925 = 12,
        SiemensSX1 = 13,
        ArchosAV500 = 14,
        SendoX = 15
    }
}