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
using System.Collections.Generic;
using System.Linq;

#if UNITY_ANDROID
using UnityEngine;
#elif UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
using Nofun.Plugins.Private;
#endif

namespace Nofun.Plugins
{
    public class FilePicker
    {
#if UNITY_EDITOR
        public static bool OpenPickFileDialog(FilterItem[] filters, Action<string> onPathReceived, string defaultPath = null)
        {
            List<string> filterMapped = new();
            foreach (FilterItem filter in filters)
            {
                filterMapped.Add(filter.name);
                filterMapped.Add(filter.spec);
            }

            string path = UnityEditor.EditorUtility.OpenFilePanelWithFilters("Select file", defaultPath ?? "", filterMapped.ToArray());
            onPathReceived(path);

            return true;
        }
#elif UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
        public static bool OpenPickFileDialog(FilterItem[] filters, Action<string> onPathReceived, string defaultPath = null)
        {
            string path = NativeFileDialog.OpenPickFileDialog(filters, defaultPath);
            onPathReceived(path);

            return true;
        }
#elif UNITY_ANDROID
        public static bool OpenPickFileDialog(FilterItem[] filters, Action<string> onPathReceived, string defaultPath = null)
        {
            if (NativeFilePicker.PickFile(
                (string path) => onPathReceived(path),
                filters.Select(item => item.spec).ToArray()
            ) != NativeFilePicker.Permission.Granted)
            {
                Debug.LogError("Open file picker permission denied!");
                return false;
            }
            else
            {
                return true;
            }
        }
#endif
    }
}
