using System;

namespace Nofun.PIP2.Encoding
{
    public class InvalidPIP2EncodingException: Exception
    {
        public InvalidPIP2EncodingException(string message)
            : base("An PIP instruction is not valid. The error: " + message)
        {

        }
    }
}