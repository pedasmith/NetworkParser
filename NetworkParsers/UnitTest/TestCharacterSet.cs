using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetworkParsers;
using NetworkParsers.Foundation;

namespace UnitTest
{
    [TestClass]
    public class TestCharacterSet
    {
        // Common set of byte arrays to try to convert.
        readonly byte[] nullbuffer = null;
        readonly byte[] empty = new byte[] { };
        readonly byte[] simple = new byte[] { (byte)'a', (byte)'b', (byte)'c' };
        readonly byte[] bit8 = new byte[] { (byte)'a', 0xA2, (byte)'c' };
        readonly byte[] bit83 = new byte[] { (byte)'a', 0xA2, 0xA3, 0xA4, (byte)'c' };
        readonly byte[] embeddednull = new byte[] { (byte)'a', 0x00, 0x00, (byte)'c' };
        readonly byte[] embeddedc080 = new byte[] { (byte)'a', 0xC0, 0x80, (byte)'c' };

        [TestMethod]
        public void TestTryConvertASCIIByteArrayToString()
        {
            bool ok;
            string result;

            ok = CharacterSetConverters.TryConvertASCIIByteArrayToString(nullbuffer, out result);
            Assert.AreEqual(true, ok, "null will convert");
            Assert.AreEqual("", result, "null converts to empty string");

            ok = CharacterSetConverters.TryConvertASCIIByteArrayToString(empty, out result);
            Assert.AreEqual(true, ok, "empty will convert");
            Assert.AreEqual("", result, "empty converts to empty string");

            ok = CharacterSetConverters.TryConvertASCIIByteArrayToString(simple, out result);
            Assert.AreEqual(true, ok, "simple will convert");
            Assert.AreEqual("abc", result, "simple converts to abc");

            ok = CharacterSetConverters.TryConvertASCIIByteArrayToString(bit8, out result);
            Assert.AreEqual(false, ok, "8bit will not convert");
            Assert.AreEqual("a[A2]c", result, "8bits will convert to...");

            ok = CharacterSetConverters.TryConvertASCIIByteArrayToString(bit83, out result);
            Assert.AreEqual(false, ok, "83bit will not convert");
            Assert.AreEqual("a[A2][A3][A4]c", result, "83bits will convert to...");

            ok = CharacterSetConverters.TryConvertASCIIByteArrayToString(embeddednull, out result);
            Assert.AreEqual(true, ok, "embedded null will convert");
            Assert.AreEqual("a\0\0c", result, "embedded null will convert to...");

            ok = CharacterSetConverters.TryConvertASCIIByteArrayToString(embeddedc080, out result);
            Assert.AreEqual(false, ok, "embedded c080 will convert");
            Assert.AreEqual("a[C0][80]c", result, "embedded c080 will convert to...");
        }


        [TestMethod]
        public void TestTryConvertLatin1ByteArrayToString()
        {
            bool ok;
            string result;

            ok = CharacterSetConverters.TryConvertLatin1ByteArrayToString(nullbuffer, out result);
            Assert.AreEqual(true, ok, "null will convert");
            Assert.AreEqual("", result);

            ok = CharacterSetConverters.TryConvertLatin1ByteArrayToString(empty, out result);
            Assert.AreEqual(true, ok, "empty will convert");
            Assert.AreEqual("", result);

            ok = CharacterSetConverters.TryConvertLatin1ByteArrayToString(simple, out result);
            Assert.AreEqual(true, ok, "simple will convert");
            Assert.AreEqual("abc", result, "simple will convert to abc");

            ok = CharacterSetConverters.TryConvertLatin1ByteArrayToString(bit8, out result);
            Assert.AreEqual(true, ok, "8bit will convert");
            Assert.AreEqual("a¢c", result, "8bit converts--is three chars long!");

            ok = CharacterSetConverters.TryConvertLatin1ByteArrayToString(bit83, out result);
            Assert.AreEqual(true, ok, "83bit will convert");
            Assert.AreEqual("a¢£¤c", result, "83bit converts--is three chars long!");

            ok = CharacterSetConverters.TryConvertLatin1ByteArrayToString(embeddednull, out result);
            Assert.AreEqual(true, ok, "embedded null will convert");
            Assert.AreEqual("a\0\0c", result, "embedded null will convert");

            ok = CharacterSetConverters.TryConvertLatin1ByteArrayToString(embeddedc080, out result);
            Assert.AreEqual(true, ok, "embedded c080 will convert");
            Assert.AreEqual("aÀc", result, "embedded c080 will convert to...");

            // Prove that every single possible byte will convert.
            for (int i=0; i<256; i++) // can't be a byte indexer
            {
                byte[] seq = new byte[] { (byte)i };
                ok = CharacterSetConverters.TryConvertLatin1ByteArrayToString(seq, out result);
                Assert.AreEqual(true, ok, $"char {i} will convert will convert");
                Assert.AreEqual(1, result.Length, $"char {i}.length is 1");
            }
        }

        [TestMethod]
        public void TestTryConvertUtf8ByteArrayToString()
        {
            bool ok;
            string result;

            ok = CharacterSetConverters.TryConvertUtf8ByteArrayToString(nullbuffer, out result);
            Assert.AreEqual(true, ok, "null will convert");
            Assert.AreEqual("", result);

            ok = CharacterSetConverters.TryConvertUtf8ByteArrayToString(empty, out result);
            Assert.AreEqual(true, ok, "empty will convert");
            Assert.AreEqual("", result);

            ok = CharacterSetConverters.TryConvertUtf8ByteArrayToString(simple, out result);
            Assert.AreEqual(true, ok, "simple will convert");
            Assert.AreEqual("abc", result, "simple will convert to abc");

            ok = CharacterSetConverters.TryConvertUtf8ByteArrayToString(bit8, out result);
            Assert.AreEqual(false, ok, "8bit will convert");
            Assert.AreEqual(null, result, "8bit converts--is three chars long!");

            ok = CharacterSetConverters.TryConvertUtf8ByteArrayToString(bit83, out result);
            Assert.AreEqual(false, ok, "83bit will convert");
            Assert.AreEqual(null, result, "83bit converts--is three chars long!");

            ok = CharacterSetConverters.TryConvertUtf8ByteArrayToString(embeddednull, out result);
            Assert.AreEqual(true, ok, "embedded null will convert");
            Assert.AreEqual("a\0\0c", result, "embedded null will convert");

            ok = CharacterSetConverters.TryConvertUtf8ByteArrayToString(embeddedc080, out result);
            Assert.AreEqual(true, ok, "embedded c080 will convert");
            //NOTE:
            // The C080 sequence is an over-long NUL. Normally NUL (8 bits, all zero)
            // will be encoded in UTF8 as \0, NUL. But in Modified-UTF8 (MUTF-8),
            // NUL is encoded in an over-long sequence C080 so that the resuting
            // string doesn't have any NUL chars, ever.
            // All MUTF-8 readers can also read CESU-8
            // For now, pin a somewhat unhappy behavior: the overlong sequence
            // is best-effort converted and the overlong NUL is treated as two
            // bogus characters and silently replaced.
            //Assert.AreEqual("a\0c", result, "embedded c080 will convert to...");
            Assert.AreEqual("a��c", result, "embedded c080 will convert to...");
        }

        [TestMethod]
        public void TestTryConvertByteArrayToString()
        {
            string result;

            result = CharacterSets.ConvertByteArrayToString(nullbuffer);
            Assert.AreEqual("", result);

            result = CharacterSets.ConvertByteArrayToString(empty);
            Assert.AreEqual("", result);

            result = CharacterSets.ConvertByteArrayToString(simple);
            Assert.AreEqual("abc", result, "simple will convert to abc");

            result = CharacterSets.ConvertByteArrayToString(bit8);
            Assert.AreEqual("a¢c", result, "8bit converts--is three chars long!");

            result = CharacterSets.ConvertByteArrayToString(bit83);
            Assert.AreEqual("a¢£¤c", result, "83bit converts--is three chars long!");

            result = CharacterSets.ConvertByteArrayToString(embeddednull);
            Assert.AreEqual("a\0\0c", result, "embedded null will convert");

            result = CharacterSets.ConvertByteArrayToString(embeddedc080);
            //NOTE:
            // The C080 sequence is an over-long NUL. Normally NUL (8 bits, all zero)
            // will be encoded in UTF8 as \0, NUL. But in Modified-UTF8 (MUTF-8),
            // NUL is encoded in an over-long sequence C080 so that the resuting
            // string doesn't have any NUL chars, ever.
            // All MUTF-8 readers can also read CESU-8
            // For now, pin a somewhat unhappy behavior: the overlong sequence
            // is best-effort converted and the overlong NUL is treated as two
            // bogus characters and silently replaced.
            //Assert.AreEqual("a\0c", result, "embedded c080 will convert to...");
            Assert.AreEqual("a��c", result, "embedded c080 will convert to...");
        }

    }
}
