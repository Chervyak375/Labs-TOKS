using System;
using lab2;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lab2.Tests
{
    [TestClass]
    public class PackTests
    {
        char flag = 'g';

        [TestMethod]
        public void EncodeTest()
        {
            string strSource = "ggg";
            Pack pack = Pack.Encode(strSource, flag);
            Assert.AreEqual(pack.Str, "10+0+0+0");

            strSource = "123456789g931hg0bkkkdgij";
            pack = Pack.Encode(strSource, 'g');
            Assert.AreEqual(pack.Str, "209123456789+04931h+060bkkkd+02ij");

            strSource = "123g93";
            pack = Pack.Encode(strSource, flag);
            Assert.AreEqual(pack.Str, "13123+293");

            strSource = "12393";
            pack = Pack.Encode(strSource, flag);
            Assert.AreEqual(pack.Str, "1512393");

            strSource = "";
            pack = Pack.Encode(strSource, flag);
            Assert.AreEqual(pack.Str, "");
        }

        [TestMethod]
        public void DecodeTest()
        {
            string str = "10+0+0+0";
            string result = Pack.Decode(str, flag);
            Assert.AreEqual(result, "ggg");

            str = "1512393";
            result = Pack.Decode(str, flag);
            Assert.AreEqual(result, "12393");

            str = "13123+293";
            result = Pack.Decode(str, flag);
            Assert.AreEqual(result, "123g93");

            str = "209123456789+04931h+060bkkkd+02ij";
            result = Pack.Decode(str, flag);
            Assert.AreEqual(result, "123456789g931hg0bkkkdgij");

        }
    }
}
