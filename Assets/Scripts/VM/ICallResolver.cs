using Nofun.PIP2;
using System;

namespace Nofun.VM
{
    public interface ICallResolver
    {
        Action Resolve(string funcName);
    }
}