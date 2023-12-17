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

#if UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
using System;
using System.Runtime.InteropServices;

namespace Nofun.Plugins.Private
{
    public static class NativeFileDialog
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct NFDU8FilterItem
        {
            public IntPtr name;
            public IntPtr spec;
        }

        private const int NFD_RESULT_ERROR = 0;
        private const int NFD_RESULT_OK = 1;
        private const int NFD_RESULT_CANCEL = 2;

        [DllImport("nfd", EntryPoint = "NFD_OpenDialogU8")]
        private static extern int NFD_OpenDialogU8(out IntPtr outPath, IntPtr filterList, uint count, IntPtr defaultPath);

        [DllImport("nfd", EntryPoint = "NFD_FreePathU8")]
        private static extern void NFD_FreePathU8(IntPtr path);

        private static IntPtr StringToMarshalledUtf8(string str)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);
            IntPtr ptr = Marshal.AllocHGlobal(bytes.Length + 1);
            Marshal.Copy(bytes, 0, ptr, bytes.Length);
            Marshal.WriteByte(ptr, bytes.Length, 0);
            return ptr;
        }

        private static void FreeMarshalledUtf8(IntPtr ptr)
        {
            Marshal.FreeHGlobal(ptr);
        }

        public static string OpenPickFileDialog(FilterItem[] filters, string defaultPath = null)
        {
            IntPtr filterList = IntPtr.Zero;
            NFDU8FilterItem[] filterItems = null;
            GCHandle filterListHandle = default;

            if (filters != null)
            {
                filterItems = new NFDU8FilterItem[filters.Length];
                for (int i = 0; i < filters.Length; i++)
                {
                    filterItems[i].name = StringToMarshalledUtf8(filters[i].name);
                    filterItems[i].spec = StringToMarshalledUtf8(filters[i].spec);
                }
                filterListHandle = GCHandle.Alloc(filterItems, GCHandleType.Pinned);
                filterList = filterListHandle.AddrOfPinnedObject();
            }

            IntPtr defaultPathPtr = IntPtr.Zero;
            if (defaultPath != null)
            {
                defaultPathPtr = StringToMarshalledUtf8(defaultPath);
            }

            IntPtr outPath = IntPtr.Zero;
            int result = NFD_OpenDialogU8(out outPath, filterList, (uint)filters.Length, defaultPathPtr);

            if (result != NFD_RESULT_OK)
            {
                return null;
            }

            string path = Marshal.PtrToStringUTF8(outPath);
            NFD_FreePathU8(outPath);

            if (defaultPathPtr != IntPtr.Zero)
            {
                FreeMarshalledUtf8(defaultPathPtr);
            }

            if (filterList != IntPtr.Zero)
            {
                for (int i = 0; i < filters.Length; i++)
                {
                    FreeMarshalledUtf8(filterItems[i].name);
                    FreeMarshalledUtf8(filterItems[i].spec);
                }

                if (filterListHandle.IsAllocated)
                {
                    filterListHandle.Free();
                }
            }

            return path;
        }
    }
}
#endif