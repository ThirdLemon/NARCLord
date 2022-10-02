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

            byte[] file1 = new byte[] { 0x47, 0x41, 0x52, 0x42 };
            byte[] file2 = new byte[] { 0x41, 0x47, 0x45, 0x44, 0x41, 0x54, 0x41, 0x21 };

            for (int i = 0; i < 4; i++)
                Assert.AreEqual(file1[i], simpleNarc[0][i]);
            for (int i = 0; i < 8; i++)
                Assert.AreEqual(file2[i], simpleNarc[1][i]);
        }

        [TestMethod]
        public void CompileSimpleNarc()
        {
            NARC emptyNarc = new NARC();
            emptyNarc[0] = new byte[4] { 0x66, 0x69, 0x72, 0x65 };
            byte[] compiled = emptyNarc.Compile();

            Assert.AreEqual(compiled.Length, compiled[8]);

            
        }
    }
}
