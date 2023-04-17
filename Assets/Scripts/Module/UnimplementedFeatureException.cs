using System;

namespace Nofun.Module
{
    public class UnimplementedFeatureException : Exception
    {
        public UnimplementedFeatureException(string message) : base(message) { }
    };
}