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

namespace Nofun.Module.VMGP
{
    public enum MessageBoxFlags : uint
    {
        Small = 0,
        Big = 1,
        YesNo = 2,
        OKCancel = 4,
        No = 0,
        Cancel = 0,
        OK = 1,
        Yes = 1,
        Error = 8,
        Warning = 0x10,
        Info = 0x20,
        Question = 0x40,
        Title = 0x80
    }
}