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

using SQLite4Unity3d;

namespace Nofun.Data.Model
{
    public class GameInfo
    {
        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }

        [Unique]
        public string Name { get; set; }
        public string Vendor { get; set; }
        public int Major { get; set; }
        public int Minor { get; set; }
        public int Revision { get; set; }

        public GameInfo()
        {
            Id = 0;
            Name = "";
            Vendor = "";
            Major = 0;
            Minor = 0;
            Revision = 0;
        }

        public GameInfo(string name, string vendor, int major, int minor, int revision)
        {
            Id = 0;
            Name = name;
            Vendor = vendor;
            Major = major;
            Minor = minor;
            Revision = revision;
        }
    }
}