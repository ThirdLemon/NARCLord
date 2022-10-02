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
        string onemisalignment;

        string pokemartNARC;

        [TestInitialize]
        public void Initialize()
        {
            garbageData = "../../../Files/sample1.bin";
            badBTAF = "../../../Files/sample2.bin";
            badFileLength = "../../../Files/sample3.bin";
            badBTNF = "../../../Files/sample5.bin";
            badGMIF = "../../../Files/sample6.bin";

            simple = "../../../Files/sample4.bin";
            onemisalignment = "../../../Files/sample7.bin";

            pokemartNARC = "../../../Files/pokemart.narc";
        }

        [TestMethod]
        public void DontPassInvalidNarcs()
        {
            Assert.ThrowsException<InvalidDataException>(() => { NARC.Build(garbageData); });
            Assert.ThrowsException<InvalidDataException>(() => { NARC.Build(badBTAF); });
            Assert.ThrowsException<InvalidDataException>(() => { NARC.Build(badFileLength); });
            Assert.ThrowsException<InvalidDataException>(() => { NARC.Build(badBTNF); });
            Assert.ThrowsException<InvalidDataException>(() => { NARC.Build(badGMIF); });
        }

        [TestMethod]
        public void MakeSimpleNarc()
        {
            NARC simpleNarc = NARC.Build(simple);

            byte[] file1 = new byte[] { 0x47, 0x41, 0x52, 0x42 };
            byte[] file2 = new byte[] { 0x41, 0x47, 0x45, 0x44, 0x41, 0x54, 0x41, 0x21 };

            for (int i = 0; i < 4; i++)
                Assert.AreEqual(file1[i], simpleNarc[0][i]);
            for (int i = 0; i < 8; i++)
                Assert.AreEqual(file2[i], simpleNarc[1][i]);
        }

        [TestMethod]
        public void MakeImperfectNarc()
        {
            NARC imperfectNarc = NARC.Build(onemisalignment);

            byte[] file1 = new byte[] { 0x46, 0x49, 0x4C, 0x45 };
            byte[] file2 = new byte[] { 0x46, 0x49, 0x4C, 0x45, 0x30, 0x32 };
            byte[] file3 = new byte[] { 0x46, 0x49, 0x4C, 0x45, 0x54, 0x48, 0x52, 0x45 };

            for (int i = 0; i < 4; i++)
                Assert.AreEqual(file1[i], imperfectNarc[0][i], "File 1 byte " + i);
            for (int i = 0; i < 6; i++)
                Assert.AreEqual(file2[i], imperfectNarc[1][i], "File 2 byte " + i);
            for (int i = 0; i < 8; i++)
                Assert.AreEqual(file3[i], imperfectNarc[2][i], "File 3 byte " + i);
        }

        [TestMethod]
        public void CompileTrivialNarc()
        {
            NARC emptyNarc = new NARC
            {
                new byte[4] { 0x66, 0x69, 0x72, 0x65 }
            };
            byte[] compiled = emptyNarc.Compile();

            Assert.AreEqual(compiled.Length, compiled[8]);

            byte[] desired = new byte[]
            {
                0x4E, 0x41, 0x52, 0x43, 0xFE, 0xFF, 0x00, 0x01, 0x40, 0x00, 0x00, 0x00, 0x10, 0x00, 0x03, 0x00,
                0x42, 0x54, 0x41, 0x46, 0x14, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x04, 0x00, 0x00, 0x00, 0x42, 0x54, 0x4E, 0x46, 0x10, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x01, 0x00, 0x47, 0x4D, 0x49, 0x46, 0x0C, 0x00, 0x00, 0x00, 0x66, 0x69, 0x72, 0x65
            };

            for (int i = 0; i < compiled.Length; i++)
                Assert.AreEqual(desired[i], compiled[i]);
        }

        [TestMethod]
        public void CompileSimpleNarc()
        {
            NARC emptyNarc = new NARC
            {
                new byte[4] { 0x47, 0x41, 0x52, 0x42 },
                new byte[8] { 0x41, 0x47, 0x45, 0x44, 0x41, 0x54, 0x41, 0x21 }
            };
            byte[] compiled = emptyNarc.Compile();

            Assert.AreEqual(compiled.Length, compiled[8]);

            byte[] desired = File.ReadAllBytes(simple);

            Assert.AreEqual(desired.Length, compiled.Length);

            for (int i = 0; i < compiled.Length; i++)
                Assert.AreEqual(desired[i], compiled[i]);
        }

        [TestMethod]
        public void DecompThenRecompile()
        {
            NARC simpleNarc = NARC.Build(simple);

            byte[] compiled = simpleNarc.Compile();

            byte[] raw = File.ReadAllBytes(simple);

            Assert.AreEqual(raw.Length, compiled.Length);

            for (int i = 0; i < raw.Length; i++)
                Assert.AreEqual(raw[i], compiled[i]);
        }

        [TestMethod]
        public void ValidateWithReal()
        {
            NARC pokemart = NARC.Build(pokemartNARC);

            byte[] compiled = pokemart.Compile();

            byte[] desired = File.ReadAllBytes(pokemartNARC);

            Assert.AreEqual(desired.Length, compiled.Length);

            for (int i = 0; i < compiled.Length; i++)
                Assert.AreEqual(desired[i], compiled[i], "Byte " + i);
        }
    }
}
