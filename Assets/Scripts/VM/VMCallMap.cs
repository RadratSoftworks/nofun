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
using Nofun.PIP2;

namespace Nofun.VM
{
    public class VMCallMap : ICallResolver
    {
        private Dictionary<string, Action<Processor, VMMemory>> callmap;
        private VMSystem system;

        public VMCallMap(VMSystem system)
        {
            callmap = new();
            this.system = system;
        }

        public void Add(string funcName, Action<Processor, VMMemory> func)
        {
            callmap.Add(funcName, func);
        }

        Action ICallResolver.Resolve(string funcName)
        {
            if (callmap.ContainsKey(funcName))
            {
                var func = callmap[funcName];
                var processor = system.Processor;
                var memory = system.Memory;

                return () => func(processor, memory);
            }
            else
            {
                return () => throw new Exception("Unimplemented function: " + funcName);
            }
        }
    }
}