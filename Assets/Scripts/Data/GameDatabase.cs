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

using System.Linq;
using SQLite4Unity3d;

namespace Nofun.Data
{
    public class GameDatabase
    {
        private SQLiteConnection _connection;

        public GameDatabase(string path)
        {
            _connection = new SQLiteConnection(path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
            _connection.CreateTable<Model.GameInfo>();
        }

        public bool AddGame(Model.GameInfo game)
        {
            try
            {
                if (_connection.Insert(game) == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public void RemoveGame(Model.GameInfo game)
        {
            _connection.Delete(game);
        }

        public Model.GameInfo[] AllGames => _connection.Table<Model.GameInfo>().ToArray();


        public Model.GameInfo[] GamesByKeyword(string keyword)
        {
            var table = _connection.Table<Model.GameInfo>();
            return table.Where(x => x.Name.Contains(keyword)).ToArray();
        }
    }
}
