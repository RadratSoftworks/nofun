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

using SharpMik.Drivers;
using System.Runtime.InteropServices;

namespace Nofun.Driver.Unity.Audio.SharpMik
{
    public class FeedToUnityDriver : VirtualSoftwareDriver
    {
        /// <summary>
        /// Current sample count that need to be fed to Unity. This already multiplies with channel count.
        /// </summary>
        private sbyte[] currentSamples = null;
        private AudioDriver unityDriver;

        public AudioDriver UnityDriver
        {
            get => unityDriver;
            set
            {
                unityDriver = value;
            }
        }

        public FeedToUnityDriver()
        {
			m_Next = null;
			m_Name = "Unity Audio Driver";
			m_Version = "Unity Audio Driver v1.0";
			m_HardVoiceLimit = 0;
			m_SoftVoiceLimit = 255;
			m_AutoUpdating = false;
        }

		public override bool IsPresent()
		{
			return true;
		}
        
        public override void Update()
		{
            if ((currentSamples == null) || (currentSamples.Length * 2 < unityDriver.TotalDestinationSamples))
            {
                currentSamples = new sbyte[unityDriver.TotalDestinationSamples * 2];
            }

			uint done = WriteBytes(currentSamples, (uint)unityDriver.TotalDestinationSamples * 2);
            unityDriver.Mix(MemoryMarshal.Cast<sbyte, short>(currentSamples), (int)done / 2);
		}
    }
}