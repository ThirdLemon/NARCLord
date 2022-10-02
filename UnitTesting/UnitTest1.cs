using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Diagnostics;
using NARCLord;

namespace UnitTesting
{
    [TestClass]
    public class UnitTest1
    {
        string garbageData;
        string badNarc1;

        [TestInitialize]
        public void Initialize()
        {
            garbageData = "../../../Files/sample1.bin";
            badNarc1 = "../../../Files/sample2.bin";
        }

        [TestMethod]
        public void DontPassInvalidNarcs()
        {
            
        }
    }
}
