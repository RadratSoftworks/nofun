using System;

namespace Nofun.Module.VMGP
{
    public interface ICacheEntry
    {
        public DateTime LastAccessed { get; set; }
    }
}