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