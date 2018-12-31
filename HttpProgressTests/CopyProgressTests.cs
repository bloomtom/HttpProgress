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
            const int bytesTransferred = 500;
            const int expectedBytes = 2001;

            ICopyProgress p = new CopyProgress(TimeSpan.FromSeconds(time), bytesPerSecond, bytesTransferred, expectedBytes);
            Assert.AreEqual((double)bytesTransferred / expectedBytes, p.PercentComplete);
            Assert.AreEqual(TimeSpan.FromSeconds(time), p.TransferTime);
            Assert.AreEqual(bytesPerSecond, p.BytesPerSecond);
            Assert.AreEqual(bytesTransferred, p.BytesTransferred);
            Assert.AreEqual(expectedBytes, p.ExpectedBytes);
        }
    }
}
