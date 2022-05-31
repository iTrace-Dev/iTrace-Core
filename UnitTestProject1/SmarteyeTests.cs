/********************************************************************************************************************************************************
* @file SmarteyeTests.cs
*
* @Copyright (C) 2022 i-trace.org
*
* This file is part of iTrace Infrastructure http://www.i-trace.org/.
* iTrace Infrastructure is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
* iTrace Infrastructure is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
* You should have received a copy of the GNU General Public License along with iTrace Infrastructure. If not, see <https://www.gnu.org/licenses/>.
********************************************************************************************************************************************************/

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
        [ExpectedException(typeof(ArgumentOutOfRangeException), "")]
        public void BadLengthNetstringNoPass()
        {
            string netstring = "9:jeffrey,";
            string result = NetstringUtils.TrimSENetstring(netstring);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "")]
        public void ZeroLengthNetstringNoPass()
        {
            string netstring = "2:,";
            string result = NetstringUtils.TrimSENetstring(netstring);
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
