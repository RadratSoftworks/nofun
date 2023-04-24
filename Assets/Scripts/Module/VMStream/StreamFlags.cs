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

namespace Nofun.Module.VMStream
{
    public enum StreamFlags : uint
    {
        Read = 0x100,
        Write = 0x200,
        ReadWrite = Read | Write,
        Binary = 0x400,
        Create = 0x800,
        Trunc = 0x1000,
        MustNotExistBefore = 0x2000,
        Deleted = 0x4000,
        WaitAccept = 0x8000
    }
}