using System.IO;
using System.Buffers.Binary;

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

        public static string ReadUTF16String(this BinaryReader reader, int lengthInBytes)
        {
            string result = "";

            for (int i = 0; i < lengthInBytes >> 1; i++)
            {
                ushort charValue = BinaryPrimitives.ReverseEndianness(reader.ReadUInt16());
                if (charValue == 0)
                {
                    break;
                }
                result += (char)charValue;
            }

            return result;
        }
    }
}