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

using Nofun.Util;
using System;
using System.Collections.Generic;

namespace Nofun.Module
{
    public class SimpleObjectManager<T> where T : class
    {
        private Dictionary<int, T> objects;
        private int nextFd;
        private int maxFd;
        private BitTable handleAlloc;

        public SimpleObjectManager(int maxFd = Int16.MaxValue)
        {
            objects = new();
            handleAlloc = new((uint)maxFd);
            nextFd = 1;

            this.maxFd = maxFd;
        }

        public T Get(int handle)
        {
            if (objects.TryGetValue(handle, out T obj))
            {
                return obj;
            }

            return null;
        }

        public void Remove(int handle)
        {
            if (handle <= 0)
            {
                throw new ArgumentException("Handle is smaller or equal to zero!");
            } 

            if (objects.Remove(handle))
            {
                handleAlloc.Clear((uint)(handle - 1));
            }
        }

        public int Add(T value)
        {
            int handleToUse = nextFd;

            if (nextFd == maxFd)
            {
                int allocated = handleAlloc.Allocate();
                if (allocated < 0)
                {
                    return -1;
                }
                handleToUse = allocated + 1;
            }
            else
            {
                nextFd++;
                handleAlloc.Set((uint)(handleToUse - 1));
            }

            objects.Add(handleToUse, value);

            return handleToUse++;
        }
    }
}