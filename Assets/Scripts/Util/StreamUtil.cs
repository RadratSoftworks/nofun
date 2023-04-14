using System.IO;

namespace Nofun.Util
{
    public static class StreamUtil
    {
        public static string ReadNullTerminatedString(BinaryReader reader)
        {
            string result = "";
            do
            {
                char value = reader.ReadChar();
                if (value == '\0')
                {
                    break;
                }
                result += value;
            } while (true);

            return result;
        }
    }
}