using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using HttpProgress;

namespace HttpProgressTests
{
    [TestClass]
    public class CopyProgressTests
    {

        [TestMethod]
        public void TestCopyProgress()
        {
            const int time = 2;
            const int bytesPerSecond = 100;
            const int bytesTransfered = 500;
            const int expectedBytes = 2001;

            ICopyProgress p = new CopyProgress(TimeSpan.FromSeconds(time), bytesPerSecond, bytesTransfered, expectedBytes);
            Assert.AreEqual((double)bytesTransfered / expectedBytes, p.PercentComplete);
            Assert.AreEqual(TimeSpan.FromSeconds(time), p.TransferTime);
            Assert.AreEqual(bytesPerSecond, p.BytesPerSecond);
            Assert.AreEqual(bytesTransfered, p.BytesTransfered);
            Assert.AreEqual(expectedBytes, p.ExpectedBytes);
        }
    }
}
