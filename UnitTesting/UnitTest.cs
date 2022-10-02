using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Diagnostics;
using NARCLord;

namespace UnitTesting
{
    [TestClass]
    public class UnitTest
    {
        string garbageData;
        string badBTAF;
        string badFileLength;
        string badBTNF;
        string badGMIF;

        string simple;

        [TestInitialize]
        public void Initialize()
        {
            garbageData = "../../../Files/sample1.bin";
            badBTAF = "../../../Files/sample2.bin";
            badFileLength = "../../../Files/sample3.bin";
            badBTNF = "../../../Files/sample5.bin";
            badGMIF = "../../../Files/sample6.bin";

            simple = "../../../Files/sample4.bin";
        }

        [TestMethod]
        public void DontPassInvalidNarcs()
        {
            Assert.ThrowsException<InvalidDataException>(() => { NARC.BuildFromFile(garbageData); });
            Assert.ThrowsException<InvalidDataException>(() => { NARC.BuildFromFile(badBTAF); });
            Assert.ThrowsException<InvalidDataException>(() => { NARC.BuildFromFile(badFileLength); });
            Assert.ThrowsException<InvalidDataException>(() => { NARC.BuildFromFile(badBTNF); });
            Assert.ThrowsException<InvalidDataException>(() => { NARC.BuildFromFile(badGMIF); });
        }

        [TestMethod]
        public void MakeSimpleNarc()
        {
            NARC simpleNarc = NARC.BuildFromFile(simple);

            Console.WriteLine(simpleNarc[0]);
        }
    }
}
