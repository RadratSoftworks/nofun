using System;
using System.IO;

namespace Nofun.Module.VMStream
{
    public static class StreamTranslationUtils
    {
        public static SeekOrigin ToSeekOrigin(StreamSeekMode seekMode)
        {
            switch (seekMode)
            {
                case StreamSeekMode.Set:
                    {
                        return SeekOrigin.Begin;
                    }

                case StreamSeekMode.Cur:
                    {
                        return SeekOrigin.Current;
                    }

                case StreamSeekMode.End:
                    {
                        return SeekOrigin.End;
                    }

                default:
                    {
                        throw new ArgumentException("Invalid stream seek mode!");
                    }
            }
        }
    }
}