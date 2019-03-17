using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace UnitTest
{
    [TestClass]
    public class TestUtf8
    {
        Random r = new Random();

        [TestMethod]
        public void TestUtf8Ascii()
        {
        }


        [TestMethod]
        public void TestUtf8Null()
        {
            Assert.AreEqual(true, IsUtf8(null), "Null buffers are utf8");
            Assert.AreEqual(true, IsUtf8(new byte[] { }), "zero length buffers are utf8");
            Assert.AreEqual(false, IsUtf8(new byte[] { 0xFF, 0xFF }, 2), "Otherise invalid buffer with length=0 are valid");
            Assert.AreEqual(true, IsUtf8(new byte[] { 0xFF, 0xFF }, 0), "Otherise invalid buffer with length=0 are valid");
        }

        [TestMethod]
        public void TestRandomValidUtf8()
        {
            const int NLoop = 300;

            for (int i = 0; i < NLoop; i++)
            {
                var len = r.Next(0, 10);
                var array = new byte[len];
                SetRandomValidUtf8(array);
                bool isValid = IsUtf8(array);
                if (!isValid)
                {
                    AssertIfIsNotValid("Created array", array, isValid);
                }
                else
                {
                    Assert.AreEqual(true, isValid);
                }
            }
        }

        [TestMethod]
        public void TestRandomInvalidUtf8()
        {
            // 2019-03-16 Ran 1_000_000 loops in 2 minutes, PASS
            const int NLoop = 1000;

            for (int i = 0; i < NLoop; i++)
            {
                var len = r.Next(1, 10); // Unlike the valid tests, testing for invalid can only happen on 
                // an array that's at least one byte long. A zero-length array is always valid.
                var array = new byte[len];
                SetRandomInvalidUtf8(array);
                bool isValid = IsUtf8(array);
                if (isValid)
                {
                    AssertIfIsValid("Created array", array, isValid);
                }
                else
                {
                    Assert.AreEqual(false, isValid); // we're only testing invalid utf8
                }
            }
        }


        private void AssertIfIsNotValid(string prefix, byte[] array, bool isValid)
        {
            int len = array.Length;

            var output = $"ERROR: {prefix} is not valid len={len} [";
            for (int j = 0; j < len; j++) output += $"{array[j]:X} ";
            output += "]";
            Assert.AreEqual(true, isValid, output);
        }

        private void AssertIfIsValid(string prefix, byte[] array, bool isValid)
        {
            int len = array.Length;

            var output = $"ERROR: {prefix} is valid (but should be invalid) len={len} [";
            for (int j = 0; j < len; j++) output += $"{array[j]:X} ";
            output += "]";
            Assert.AreEqual(false, isValid, output);
        }

        [TestMethod]
        public void CompareRandomValidUtf8ToMicrosoftDecoding()
        {
            // 2019-03-16 Did 3_000_000 loops in 1 minute; PASS
            // 2019-03-16 Did   100_000 loops in 5 seconds; PASS
            const int NLoop = 10_000;

            Encoding Utf8Encoder = new UTF8Encoding(false, true); // No BOM and throw on error

            for (int i = 0; i < NLoop; i++)
            {
                var len = r.Next(0, 10);
                var array = new byte[len];
                SetRandomValidUtf8(array);

                bool isValid = IsUtf8(array);
                if (!isValid)
                {
                    AssertIfIsNotValid("Created Array", array, isValid);
                    continue; // The array should be valid for us to continue.
                }

                string cryptoString = "";
                try
                {
                    IBuffer buffer = CryptographicBuffer.CreateFromByteArray(array);
                    cryptoString = CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, buffer);
                }
                catch (Exception ex)
                {
                    AssertIfIsNotValid($"CryptographicBuffer.ConvertBinaryToString ({ex.Message})", array, isValid);
                    continue; // The array should be valid for us to continue.
                }

                string systextString = "";
                try
                {
                    systextString = System.Text.Encoding.UTF8.GetString(array);
                }
                catch (Exception ex)
                {
                    AssertIfIsNotValid($"System.Text.Encoding.UTF8 ({ex.Message})", array, isValid);
                    continue; // The array should be valid for us to continue.
                }

                string sysutf8String = "";
                try
                {
                    sysutf8String = Utf8Encoder.GetString(array);
                }
                catch (Exception ex)
                {
                    AssertIfIsNotValid($"NEW System.Text.Encoding.UTF8 ({ex.Message})", array, isValid);
                    continue; // The array should be valid for us to continue.
                }


                string datareaderString = "";
                try
                {
                    IBuffer buffer = CryptographicBuffer.CreateFromByteArray(array);
                    var dr = DataReader.FromBuffer(buffer);
                    //dr.LoadAsync(buffer.Length).AsTask().Wait();
                    datareaderString = dr.ReadString(buffer.Length);
                }
                catch (Exception ex)
                {
                    AssertIfIsNotValid($"DataReader.ReadString ({ex.Message})", array, isValid);
                    continue; // The array should be valid for us to continue.
                }

                Assert.AreEqual(cryptoString, systextString, $"Crypto string ({cryptoString}) should be systext ({systextString})");
                Assert.AreEqual(cryptoString, sysutf8String, $"Crypto string ({cryptoString}) should be sysutftext ({systextString})");
                Assert.AreEqual(cryptoString, datareaderString, $"Crypto string ({cryptoString}) should be datareader ({datareaderString})");
            }
        }


        /// <summary>
        /// Fills a byte array with a valid UTF8 string
        /// </summary>
        /// <param name="array"></param>
        private void SetRandomValidUtf8(byte[] array)
        {
            for (int i=0; i<array.Length; i++)
            {
                int maxTail = Math.Min (4, array.Length-(i+1));
                var tailBytes = r.Next(maxTail);
                try
                {
                    SetValidUtfBytes(array, i, tailBytes);
                }
                catch (Exception)
                {
                    ;
                }
                i += tailBytes;
            }
        }

        /// <summary>
        /// Fills a byte array with a invalid UTF8 string. There will be exactly one invalid utf8 sequence, placed randomly.
        /// This is done by first making a fully valid UTF8 sequence and then replacing one sequence with a bad sequence.
        /// </summary>
        /// <param name="array"></param>
        private void SetRandomInvalidUtf8(byte[] array)
        {
            var utf8Indexes = new List<Tuple<int, int>>();
            for (int i = 0; i < array.Length; i++)
            {
                int maxTail = Math.Min(4, array.Length - (i + 1));
                var tailBytes = r.Next(maxTail);
                try
                {
                    SetValidUtfBytes(array, i, tailBytes);
                    utf8Indexes.Add(new Tuple<int, int>(i, tailBytes));
                }
                catch (Exception)
                {
                    Assert.AreEqual(false, true, $"Failed to add UTF8 nbytes {tailBytes} in SetRandomInvalidUtf8");
                    ;
                }
                i += tailBytes;
            }

            // Convert one of the Utf8 sequences to be invalid
            var invalidIndex = r.Next(utf8Indexes.Count);
            SetInvalidUtfBytes(array, utf8Indexes[invalidIndex].Item1, utf8Indexes[invalidIndex].Item2);
        }

        /// <summary>
        /// Place a valid UTF8 sequence into the array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="startIndex"></param>
        /// <param name="tailBytes"></param>
        /// <returns></returns>
        private int SetValidUtfBytes (byte[] array, int startIndex, int tailBytes)
        {
            switch (tailBytes)
            {
                case 0:
                    array[startIndex] = GetRandomByte(CharClass.Ascii);
                    return 1;

                case 1:
                    array[startIndex] = GetRandomByte(CharClass.Top2);
                    array[startIndex + 1] = GetRandomByte(CharClass.Tail);
                    return 1;

                case 2:
                    array[startIndex] = GetRandomByte(CharClass.Top3);
                    array[startIndex + 1] = GetRandomByte(CharClass.Tail);
                    array[startIndex + 2] = GetRandomByte(CharClass.Tail);
                    return 1;

                case 3:
                    array[startIndex] = GetRandomByte(CharClass.Top4);
                    array[startIndex + 1] = GetRandomByte(CharClass.Tail);
                    array[startIndex + 2] = GetRandomByte(CharClass.Tail);
                    array[startIndex + 3] = GetRandomByte(CharClass.Tail);
                    return 1;

                default:
                    Assert.AreEqual(false, true, $"Unknown UTF8 nbytes {tailBytes} in AddValidUtf8Bytes (s.b 1..4)");
                    return 0xFF; // not legal UTF8
            }
        }

        private int SetInvalidUtfBytes(byte[] array, int startIndex, int tailBytes)
        {
            var whichIsInvalid = tailBytes < 1 ? 0: r.Next(tailBytes+1);
            switch (tailBytes)
            {
                case 0:
                    array[startIndex] = GetRandomInvalidByte(CharClass.Ascii);
                    return 1;

                case 1:
                    array[startIndex] = GetRandomPotentiallyInvalidByte(CharClass.Top2, whichIsInvalid == 0);
                    array[startIndex + 1] = GetRandomPotentiallyInvalidByte(CharClass.Tail, whichIsInvalid == 1);
                    return 1;

                case 2:
                    array[startIndex] = GetRandomPotentiallyInvalidByte(CharClass.Top3, whichIsInvalid == 0);
                    array[startIndex + 1] = GetRandomPotentiallyInvalidByte(CharClass.Tail, whichIsInvalid == 1);
                    array[startIndex + 2] = GetRandomPotentiallyInvalidByte(CharClass.Tail, whichIsInvalid == 2);
                    return 1;

                case 3:
                    array[startIndex] = GetRandomPotentiallyInvalidByte(CharClass.Top4, whichIsInvalid == 0);
                    array[startIndex + 1] = GetRandomPotentiallyInvalidByte(CharClass.Tail, whichIsInvalid == 1);
                    array[startIndex + 2] = GetRandomPotentiallyInvalidByte(CharClass.Tail, whichIsInvalid == 2);
                    array[startIndex + 3] = GetRandomPotentiallyInvalidByte(CharClass.Tail, whichIsInvalid == 3);
                    return 1;

                default:
                    Assert.AreEqual(false, true, $"Unknown UTF8 nbytes {tailBytes} in AddInvalidUtf8Bytes (s.b 1..4)");
                    return 0xFF; // not legal UTF8
            }
        }

        enum CharClass { Ascii, Top2, Top3, Top4, Tail };

        /// <summary>
        /// Given a utf8 character class, returns a random character from that class.
        /// </summary>
        /// <param name="charClass"></param>
        /// <returns></returns>
        private byte GetRandomByte (CharClass charClass)
        {
            switch (charClass)
            {
                case CharClass.Ascii: return GetRandomByte(7);
                case CharClass.Top2: return (byte)(0xC0 | GetRandomByte(5));
                case CharClass.Top3: return (byte)(0xE0 | GetRandomByte(4));
                case CharClass.Top4: return (byte)(0xF0 | GetRandomByte(3));
                case CharClass.Tail: return (byte)(0x80 | GetRandomByte(6));
                default:
                    Assert.AreEqual(false, true, $"Unknown CharClass {charClass} in GetRandomByte");
                    return 0xFF; // not legal UTF8
            }
        }

        /// <summary>
        /// Given a utf8 character class, returns a random character not from that class.
        /// </summary>
        /// <param name="charClass"></param>
        /// <returns></returns>
        private byte GetRandomInvalidByte(CharClass charClass)
        {
            bool isValid = true;
            int nloop = 0;
            byte value = 0;
            while (isValid)
            {
                value = GetRandomByte(8);
                switch (charClass)
                {
                    case CharClass.Ascii: isValid = (value & 0x80) == 0; break;
                    case CharClass.Top2: isValid = (value & 0xE0) == 0xC0; break;
                    case CharClass.Top3: isValid = (value & 0xF0) == 0xE0; break;
                    case CharClass.Top4: isValid = (value & 0xF8) == 0xF0; break;
                    case CharClass.Tail: isValid = (value & 0xC0) == 0x80; break;
                    default:
                        Assert.AreEqual(false, true, $"Unknown CharClass {charClass} in GetRandomInvalidByte");
                        return 0xFF; // not legal UTF8
                }
                nloop++;
                if (nloop > 40)
                {
                    ; // Not good; we should have gotten a proper invalid byte by now.
                }
            }
            return value;
        }

        /// <summary>
        /// Given a utf8 character class, returns a random character not from that class.
        /// </summary>
        /// <param name="charClass"></param>
        /// <returns></returns>
        private byte GetRandomPotentiallyInvalidByte(CharClass charClass, bool shouldBeInvalid)
        {
            if (shouldBeInvalid) return GetRandomInvalidByte(charClass);
            return GetRandomByte(charClass);
        }

        [TestMethod]
        public void TestGetRandomByte()
        {
            for (int i=0; i<100; i++)
            {
                byte ascii = GetRandomByte(CharClass.Ascii);
                Assert.AreEqual(true, ascii >= 0, $"Ascii ({ascii:X}) value must be >= 0");
                Assert.AreEqual(true, ascii <= 0x7F, $"Ascii ({ascii:X}) value must be <= 7F");

                byte top2 = GetRandomByte(CharClass.Top2);
                Assert.AreEqual(true, top2 >= 0xC0, $"Top2 ({top2:X}) value must be >= C0");
                Assert.AreEqual(true, top2 < 0xE0, $"Top2 ({top2:X}) value must be < E0");

                byte top3 = GetRandomByte(CharClass.Top3);
                Assert.AreEqual(true, top3 >= 0xE0, $"Top3 ({top3:X}) value must be >= C0");
                Assert.AreEqual(true, top3 < 0xF0, $"Top3 ({top3:X}) value must be < F0");

                byte top4 = GetRandomByte(CharClass.Top4);
                Assert.AreEqual(true, top4 >= 0xF0, $"Top4 ({top4:X}) value must be >= F0");
                Assert.AreEqual(true, top4 < 0xF8, $"Top4 ({top4:X}) value must be < F8");

                byte tail = GetRandomByte(CharClass.Tail);
                Assert.AreEqual(true, tail >= 0x80, $"Tail ({tail:X}) value must be >= 80");
                Assert.AreEqual(true, tail < 0xC0, $"Tail ({tail:X}) value must be < C0");

            }
        }

        /// <summary>
        /// Given a number of bits, return a byte where the top (8-nbits) bits are zero
        /// and the bottom bits are random.
        /// </summary>
        /// <param name="nbits">Value 1..8 inclusive</param>
        private byte GetRandomByte (int nbits)
        {
            Assert.AreEqual(true, nbits > 0, $"NBITS ({nbits}) must be > 0");
            Assert.AreEqual(true, nbits <= 8, $"NBITS ({nbits}) must be <= 8");
            // Example:
            // If nbits is 4, I want a byte where the bottom 4 bits can be anything.
            // 1<<4 is (binary) 10000 which is 1 more than 01111 aka 0xF
            // By asking for a random number whose exclusive max is 1<<nbits, I get a correct value.
            int random = r.Next(1 << nbits);
            return (byte)random;
        }

        /// <summary>
        /// Facade for the real IsUtf8 lets me swap out different implementations
        /// </summary>
        private bool IsUtf8(byte[] buffer)
        {
            return NetworkParsers.Utf8.IsUtf8(buffer);
        }

        /// <summary>
        /// Facade for the real IsUtf8 lets me swap out different implementations
        /// </summary>
        private bool IsUtf8(byte[] buffer, int length)
        {
            return NetworkParsers.Utf8.IsUtf8(buffer, length);
        }
    }
}
