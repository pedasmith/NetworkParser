using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetworkParsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest
{
    [TestClass]
    public class TestGopherString
    {
        [TestMethod]
        public void TestGopherEscape()
        {
            Assert.AreEqual("Simple String Blank", "Simple String Blank".EscapeGopher());
            Assert.AreEqual("", "".EscapeGopher());
            Assert.AreEqual("Simple?String LF", "Simple\nString LF".EscapeGopher());
            Assert.AreEqual("Simple?String CR", "Simple\rString CR".EscapeGopher());
            Assert.AreEqual("Simple?String NUL", "Simple\0String NUL".EscapeGopher());

            Assert.AreEqual("Simple_String", "Simple\nString".EscapeGopher('_'));

            Assert.AreEqual("?Char Placement", "\nChar Placement".EscapeGopher());
            Assert.AreEqual("Char Placement?", "Char Placement\r".EscapeGopher());
            Assert.AreEqual("???Multiple Chars???", "\r\n\tMultiple Chars\n\n\n".EscapeGopher());
        }
    }
}
