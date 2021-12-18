using iTrace_Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTestProject1
{
    [TestClass]
    public class SmarteyeTests
    {
        [TestMethod]
        public void ValidNetstringPass()
        {
            string netstring = "7:jeffrey,";
            string result = NetstringUtils.TrimSENetstring(netstring);

            Assert.AreEqual(result, "jeffrey");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException),"")]
        public void NoLengthNetstringNoPass()
        {
            string netstring = ":jeffrey,";
            string result = NetstringUtils.TrimSENetstring(netstring);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "")]
        public void NoSeperatorNetstringNoPass()
        {
            string netstring = "7jeffrey,";
            string result = NetstringUtils.TrimSENetstring(netstring);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "")]
        public void NoCommaNetstringNoPass()
        {
            string netstring = "7:jeffrey";
            string result = NetstringUtils.TrimSENetstring(netstring);
        }
    }
}
