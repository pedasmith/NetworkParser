
using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    [TestClass]
    public class TestCRLF
    {
        [TestMethod]
        public void TestCRLFProperEnding()
        {
            ValidateTwoLines("line1\r\nline2\r\n");
        }

        [TestMethod]
        public void TestCRLFLFCR()
        {
            ValidateTwoLines("line1\n\rline2\n\r");
        }

        [TestMethod]
        public void TestCRLFMix()
        {
            ValidateTwoLines("line1\r\nline2\n");
        }

        [TestMethod]
        public void TestCRLFJustCR()
        {
            ValidateTwoLines("line1\rline2\r");
        }

        [TestMethod]
        public void TestCRLFJustLF()
        {
            ValidateTwoLines("line1\nline2\n");
        }

        [TestMethod]
        public void TestCRLFNoEOL()
        {
            ValidateTwoLines("line1\nline2", true);
            ValidateTwoLines("line1\r\nline2", true);
            ValidateTwoLines("line1\n\rline2", true);
        }

        [TestMethod]
        public void TestCRLFFirstBlankLine()
        {
            ValidateTwoLines("\r\nline2\r\n", false, "", "line2");
        }

        [TestMethod]
        public void TestCRLFSecondBlankLine()
        {
            ValidateTwoLines("line1\r\n\r\n", false, "line1", "");
        }

        [TestMethod]
        public void TestCRLFTwoBlankLines()
        {
            ValidateTwoLines("\r\n\r\n", false, "", "");
        }

        [TestMethod]
        public void TestCRLFTwoBlankLinesCRLFCR()
        {
            ValidateTwoLines("\r\n\r", false, "", "");
        }

        [TestMethod]
        public void TestCRLFTwoBlankCRLines()
        {
            ValidateTwoLines("\r\r", false, "", "");
        }

        [TestMethod]
        public void TestCRLFTwoCalls()
        {
            ValidateTwoLinesTwoCalls("line1\r\n", "line2\r\n");
        }

        [TestMethod]
        public void TestCRLFTwoCallsSecondBlank()
        {
            ValidateTwoLinesTwoCalls("line1\r\nline2\r\n", "");
        }

        [TestMethod]
        public void TestCRLFTwoCallsFirstBlank()
        {
            ValidateTwoLinesTwoCalls("", "line1\r\nline2\r\n");
        }

        [TestMethod]
        public void TestCRLFTwoCallsFirstPartial()
        {
            ValidateTwoLinesTwoCalls("line", "1\r\nline2\r\n");
        }

        [TestMethod]
        public void TestCRLFTwoCallsSecondPartial()
        {
            ValidateTwoLinesTwoCalls("line1\r\nlin", "e2\r\n");
        }

        [TestMethod]
        public void TestCRLFTwoCallsSplitCRLF()
        {
            ValidateTwoLinesTwoCalls("line1\r", "\nline2\r\n");
        }
        [TestMethod]
        public void TestCRLFTwoCallsSplitCRLaterCRLF()
        {
            ValidateTwoLinesTwoCalls("line1\r", "line2\r\n");
        }

        [TestMethod]
        public void TestCRLFTwoCallsSplitCRThenCRLF()
        {
            ValidateThreeLinesTwoCalls("line1\r", "\rline3\r\n", false, "line1", "", "line3");
        }



        public void ValidateTwoLines(string text, bool lastLinePartial = false, string line1text = "line1", string line2text = "line2")
        {
            var testBytes = Encoding.UTF8.GetBytes(text);
            var state = NetworkParsers.ParseCRLF.SplitCRLF(testBytes, new NetworkParsers.ParseCRLF.SplitState());
            var line1 = Encoding.UTF8.GetBytes(line1text);
            var line2 = Encoding.UTF8.GetBytes(line2text);

            const int nline = 2;
            Assert.AreEqual(nline, state.Lines.Count, $"Expected {nline} lines");
            CollectionAssert.AreEqual(state.Lines[0], line1, $"Line1 is {line1text}");
            CollectionAssert.AreEqual(state.Lines[1], line2, $"Line2 is {line2text}");
            Assert.AreEqual(lastLinePartial, state.LastLinePartial, "Should end with EOL");
        }


        public void ValidateTwoLinesTwoCalls(string text1, string text2, bool lastLinePartial = false, string line1text = "line1", string line2text = "line2")
        {
            var test1Bytes = Encoding.UTF8.GetBytes(text1);
            var test2Bytes = Encoding.UTF8.GetBytes(text2);
            var state = NetworkParsers.ParseCRLF.SplitCRLF(test1Bytes, new NetworkParsers.ParseCRLF.SplitState());
            state = NetworkParsers.ParseCRLF.SplitCRLF(test2Bytes, state);

            var line1 = Encoding.UTF8.GetBytes(line1text);
            var line2 = Encoding.UTF8.GetBytes(line2text);

            const int nline = 2;
            Assert.AreEqual(nline, state.Lines.Count, $"Expected {nline} lines");
            CollectionAssert.AreEqual(state.Lines[0], line1, $"Line1 is {line1text}");
            CollectionAssert.AreEqual(state.Lines[1], line2, $"Line2 is {line2text}");
            Assert.AreEqual(lastLinePartial, state.LastLinePartial, "Should end with EOL");
        }

        public void ValidateThreeLinesTwoCalls(string text1, string text2, bool lastLinePartial = false, string line1text = "line1", string line2text = "line2", string line3text = "line3")
        {
            var test1Bytes = Encoding.UTF8.GetBytes(text1);
            var test2Bytes = Encoding.UTF8.GetBytes(text2);
            var state = NetworkParsers.ParseCRLF.SplitCRLF(test1Bytes, new NetworkParsers.ParseCRLF.SplitState());
            state = NetworkParsers.ParseCRLF.SplitCRLF(test2Bytes, state);

            var line1 = Encoding.UTF8.GetBytes(line1text);
            var line2 = Encoding.UTF8.GetBytes(line2text);
            var line3 = Encoding.UTF8.GetBytes(line3text);

            const int nline = 3;
            Assert.AreEqual(nline, state.Lines.Count, $"Expected {nline} lines");
            CollectionAssert.AreEqual(state.Lines[0], line1, $"Line1 is {line1text}");
            CollectionAssert.AreEqual(state.Lines[1], line2, $"Line2 is {line2text}");
            CollectionAssert.AreEqual(state.Lines[2], line3, $"Line3 is {line3text}");
            Assert.AreEqual(lastLinePartial, state.LastLinePartial, "Should end with EOL");
        }

    }
}
