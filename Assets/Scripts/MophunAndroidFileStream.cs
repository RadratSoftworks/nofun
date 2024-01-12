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

#if UNITY_ANDROID
using System;
using System.IO;
using System.Runtime.InteropServices;

using UnityEngine;

namespace Nofun
{
    public class MophunAndroidFileStream : Stream
    {
        [DllImport("SAFFileRouter", EntryPoint = "saf_router_open")]
        public static extern IntPtr OpenRouter(int fd);
        
        [DllImport("SAFFileRouter", EntryPoint = "saf_router_read")]
        public static extern Int64 ReadRouter(IntPtr handle, IntPtr buffer, int count);
        
        [DllImport("SAFFileRouter", EntryPoint = "saf_router_tell")]
        public static extern Int64 TellRouter(IntPtr handle);

        [DllImport("SAFFileRouter", EntryPoint = "saf_router_seek")]
        public static extern Int64 SeekRouter(IntPtr handle, int offset, int whence);

        [DllImport("SAFFileRouter", EntryPoint = "saf_router_close")]
        public static extern Int64 CloseRouter(IntPtr handle);

        public static int ToCWhence(SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    return 0;
                case SeekOrigin.Current:
                    return 1;
                case SeekOrigin.End:
                    return 2;
                default:
                    throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
            }
        }

        private long fileLength;
        private IntPtr currentHandle;

        public MophunAndroidFileStream()
        {
            // https://answers.unity.com/questions/1350799/convert-android-uri-to-a-file-path-unity-can-read.html
            // Thanks Guy-Corbett!
            AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activityObject = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");

            // Get the current intent
            AndroidJavaObject intent = activityObject.Call<AndroidJavaObject>("getIntent");

            // Get the intent data using AndroidJNI.CallObjectMethod so we can check for null
            IntPtr method_getData = AndroidJNIHelper.GetMethodID(intent.GetRawClass(), "getData", "()Ljava/lang/Object;");
            IntPtr getDataResult = AndroidJNI.CallObjectMethod(intent.GetRawObject(), method_getData, AndroidJNIHelper.CreateJNIArgArray(new object[0]));
            
            if (getDataResult.ToInt32() != 0)
            {
                // Now actually get the data. We should be able to get it from the result of AndroidJNI.CallObjectMethod, but I don't now how so just call again
                AndroidJavaObject intentURI = intent.Call<AndroidJavaObject>("getData");

                // Open the URI
                AndroidJavaObject contentResolver = activityObject.Call<AndroidJavaObject>("getContentResolver");
                AndroidJavaObject fdParcel = contentResolver.Call<AndroidJavaObject>("openFileDescriptor", intentURI, "rw");

                int fd = fdParcel.Call<int>("detachFd");
                currentHandle = OpenRouter(fd);

                if (currentHandle == IntPtr.Zero)
                {
                    throw new Exception("Can't open mophun file!");
                }

                fileLength = SeekRouter(currentHandle, 0, ToCWhence(SeekOrigin.End));
                SeekRouter(currentHandle, 0, ToCWhence(SeekOrigin.Begin));
            }
            else
            {
                throw new Exception("Can't get intent data!");
            }
        }

        ~MophunAndroidFileStream()
        {
            if (currentHandle != IntPtr.Zero)
            {
                CloseRouter(currentHandle);
            }
        }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        public override long Length => fileLength;

        public override long Position
        {
            get => TellRouter(currentHandle);
            set => SeekRouter(currentHandle, (int)value, ToCWhence(SeekOrigin.Begin));
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            unsafe
            {
                fixed (byte* bufferPtr = buffer)
                {
                    return (int)ReadRouter(currentHandle, (IntPtr)(bufferPtr + offset), count);
                }
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return SeekRouter(currentHandle, (int)offset, ToCWhence(origin));
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
#endif