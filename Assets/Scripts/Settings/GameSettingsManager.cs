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

using System.IO;
using Nofun.Util;
using UnityEngine;

namespace Nofun.Settings
{
    public class GameSettingsManager
    {
        private string settingsPath;

        public GameSettingsManager(string persistentDataPath)
        {
            settingsPath = Path.Join(persistentDataPath, "__GameSettings");
            Directory.CreateDirectory(settingsPath);
        }

        private string GetSettingPath(string gameName)
        {
            string validGameNamePath = Path.ChangeExtension(gameName.ToValidFileName(), ".json");
            return Path.Join(settingsPath, validGameNamePath);
        }

        public GameSetting? Get(string gameName)
        {
            string gameSettingPath = GetSettingPath(gameName);
            if (!File.Exists(gameSettingPath))
            {
                return null;
            }

            return JsonUtility.FromJson<GameSetting>(File.ReadAllText(gameSettingPath));
        }

        public bool Set(string gameName, GameSetting setting)
        {
            string gameSettingPath = GetSettingPath(gameName);
            bool firstTime = false;

            if (!File.Exists(gameSettingPath))
            {
                firstTime = true;
            }

            File.WriteAllText(gameSettingPath, JsonUtility.ToJson(setting));
            return firstTime;
        }
    };
}
