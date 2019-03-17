using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkParsers
{
    public class Utf8
    {
        /// <summary>
        /// Returns true if the given buffer is valid UTF8 chars.
        /// If the buffer is null or has no chars, then return true.
        /// </summary>
        /// <param name="buffer">Array of byte values to evaluate</param>
        /// <param name="length">Length of buffer; if -1 (the default) then the entire buffer will be checked </param>
        /// <returns></returns>
        public static bool IsUtf8 (byte[] buffer, int length=-1)
        {
            if (buffer == null) return true; // null buffes are Utf8 (there aren't any invalid bytes)
            if (length == -1) length = buffer.Length;
            if (length > buffer.Length) return false;

            for (int i=0; i<length; i++)
            {
                var tailLength = nextra(buffer[i]);
                if (tailLength < 0) return false; 
                for (int j=0; j<tailLength; j++)
                {
                    var index = i + j + 1;
                    if (index >= length)
                    {
                        return false;
                    }
                    byte b = buffer[index];
                    if ((b & ~0x3F) != 0x80)
                    {
                        return false;
                    }
                }
                i += tailLength;
            }
            return true;
        }

        private static int nextra (byte b)
        {
            if ((b & ~0x7F) == 0)
            {
                return 0; // is 7-bit ascii
            }
            else if ((b & ~0x1F) == 0xC0)
            {
                return 1; // is 2-byte
            }
            else if ((b & ~0x0F) == 0xE0)
            {
                return 2; // is 3-byte
            }
            else if ((b & ~0x07) == 0xF0)
            {
                return 3; // is 4-byte
            }
            return -1; // is not valid UTF8
        }
    }
}
