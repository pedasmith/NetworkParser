using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace NetworkParsers
{
    namespace Foundation
    {
        /// <summary>
        /// Handles common problems when dealing with text-based internet protocols.
        /// </summary>
        public class CharacterSetConverters
        {
            static Encoding Utf8Encoder = new UTF8Encoding(false, true); // No BOM and throw on error
            static Encoding Utf8EncoderNonThrowing = new UTF8Encoding(false, false); // No BOM and do not throw on error

            /// <summary>
            /// Converts a byte array in correct Utf8 format to a string. Non-UTF8 returns false.
            /// </summary>
            /// <param name="data">byte array with input data</param>
            /// <param name="str">returned string. If return is true, guaranteed to be non-null.</param>
            /// <returns>true if the original data is utf8. Null and empty are both considered utf8 </returns>
            public static bool TryConvertUtf8ByteArrayToString(byte[] data, out string str)
            {
                // null data isn't not utf8.
                if (data == null)
                {
                    str = "";
                    return true;
                }

                bool isUtf8 = NetworkParsers.Utf8.IsUtf8(data, data.Length);
                if (isUtf8)
                {
                    var unicodeString = "";
                    try
                    {
                        unicodeString = Utf8Encoder.GetString(data);
                        str = unicodeString ?? ""; // never null.
                        return true;
                    }
                    catch (Exception)
                    {
                        // failed to decode. Our utf8 guess must have been wrong, which is essentially inconceivable.
                        str = null;
                        //return false;
                    }

                    // Try the other decoder. I don't know that it ever converts when the Utf8Encoder doesn't work.
                    string cryptoString = "";
                    try
                    {
                        IBuffer buffer = CryptographicBuffer.CreateFromByteArray(data);
                        cryptoString = CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, buffer);
                        str = cryptoString ?? "";
                        return true;
                    }
                    catch (Exception)
                    {
                        ; // also fails
                    }

                    unicodeString = "";
                    try
                    {
                        unicodeString = Utf8EncoderNonThrowing.GetString(data);
                        str = unicodeString ?? ""; // never null.
                        return true;
                    }
                    catch (Exception)
                    {
                        // more failures? How? Really just give up now.
                    }
                    // note that this is a total failure of the IsUtf8 promise.
                    str = null;
                    return false;
                }

                // Not utf8. 
                str = null;
                return false;
            }

            /// <summary>
            /// Converts a byte array in correct Latin1 format to a string. Non-Latin1 returns false. Note that Latin1 includes a transcription for every byte!
            /// </summary>
            /// <param name="data">byte array with input data</param>
            /// <param name="str">returned string. If return is true, guaranteed to be non-null.</param>
            /// <returns>true if the original data is utf8. Null and empty are both considered utf8 </returns>
            public static bool TryConvertLatin1ByteArrayToString(byte[] data, out string str)
            {
                // null data isn't not Latin1.
                if (data == null)
                {
                    str = "";
                    return true;
                }

                try
                {
                    var results = Encoding.GetEncoding(28591).GetString(data);
                    str = results ?? "";
                    return true;
                }
                catch (Exception)
                {
                    ; // failed to decode...
                }

                // Not utf8. 
                str = null;
                return false;
            }

            /// <summary>
            /// Converts a byte array in correct ASCII format to a string. Non-ASCII chars are converted to a HEX output.
            /// /// </summary>
            /// <param name="data">byte array with input data</param>
            /// <param name="str">returned string. Will always be the some representation of the string</param>
            /// <returns>true if the original data is ASCII. Null and empty are both considered utf8 </returns>
            public static bool TryConvertASCIIByteArrayToString(byte[] data, out string str)
            {
                // null data isn't not Latin1.
                if (data == null)
                {
                    str = "";
                    return true;
                }
                bool retval = true;

                // Try the giant hammer of decoding. This should never be called because
                // the 28591 encoding can convert any byte!
                var sb = new StringBuilder();
                foreach (var ch in data)
                {
                    if (ch >= 128)
                    {
                        retval = false;
                        sb.Append($"[{ch:X}]");
                    }
                    else
                    {
                        sb.Append((char)ch);
                    }
                }
                str = sb.ToString();
                return retval;
            }
        }// End of the Foundation namespace.

        /// <summary>
        /// Simple statics method to convert byte arrays into strings, guessing at the correct encoding. 
        /// In all cases, a string will be returned.
        /// Will attempt to decode the string as UTF8 or LATIN1. In the impossible case that the LAIN1 converter doesn't work,
        /// will convert as ASCII, writing out bytes with the top bit on as [HEX].
        /// In the case of improper UTF8 (like MUTF-8's overlong "C080"--> "\0" conversion),
        /// the output will replace the C0 and 80 with � characters.
        /// </summary>
        public class CharacterSets
        {
            /// <summary>
            /// Converts a byte[] array into a string with either a UTF8 (preferred) or Latin-1 character set.
            /// </summary>
            /// <param name="data">byte[] of data</param>
            /// <returns>non-null string in all cases (even if data is null)</returns>
            public static string ConvertByteArrayToString(byte[] data)
            {
                bool okUtf8 = Foundation.CharacterSetConverters.TryConvertUtf8ByteArrayToString(data, out var utf8str);
                if (okUtf8) return utf8str;

                bool okLatin1 = Foundation.CharacterSetConverters.TryConvertLatin1ByteArrayToString(data, out var latin1str);
                if (okLatin1) return latin1str;

                Foundation.CharacterSetConverters.TryConvertASCIIByteArrayToString(data, out var asciistr); // never returns false.
                return asciistr;
            }

            /// <summary>
            /// Converts an IBuffer into a string with either a UTF8 (preferred) or Latin-1 character set.
            /// </summary>
            /// <param name="buffer">IBuffer with data</param>
            /// <returns>non-null string in all cases (even if data is null)</returns>
            public static string ConvertIBufferToString(IBuffer buffer)
            {
                byte[] data = buffer.ToArray();
                return ConvertByteArrayToString(data);
            }
        }
    }
}
