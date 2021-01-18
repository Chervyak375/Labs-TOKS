using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lab3;

namespace CRCTests
{
    [TestClass]
    public class CRCTest
    {
        [TestMethod]
        public void EncodeTest()
        {
            byte[] input = new byte[] { 170 };
            CRC crc = new CRC();
            byte[] check = crc.Encode(input);
            byte[] output = crc.Decode(check, input.Length * 8);
            Assert.AreEqual(output[0], input[0]);
        }
    }
}
