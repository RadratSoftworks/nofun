using System;

namespace Nofun.Parser
{
    public class VMGPInvalidHeaderException : Exception
    {
        public VMGPInvalidHeaderException(string reason)
            : base("The header is invalid! Reason: " + reason) {
        }
    }
}