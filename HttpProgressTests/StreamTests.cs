using Microsoft.VisualStudio.TestTools.UnitTesting;
using HttpProgress;
using System.IO;
using System;
using System.Threading.Tasks;

namespace HttpProgressTests
{
    [TestClass]
    public class StreamTests
    {
        private const int streamLength = 1024 * 1024 * 100;
        private const int bufferSize = 16384;

        [TestMethod]
        public async Task TestStreamCopy()
        {
            int progressEventCounter = 0;
            double percentComplete = 0;

            using (var source = GenerateStream(streamLength))
            using (var destination = new MemoryStream())
            {
                var progress = new Action<ICopyProgress>(x =>
                {
                    progressEventCounter++;
                    Assert.IsTrue(x.PercentComplete >= percentComplete);
                    percentComplete = x.PercentComplete;
                });
                await source.CopyToAsync(destination, bufferSize, streamLength, progress);
            }

            Assert.AreEqual(1, percentComplete);
            Assert.AreEqual((int)Math.Ceiling((double)streamLength / bufferSize), progressEventCounter);
        }

        private Stream GenerateStream(int length)
        {
            byte[] bytes = new byte[length];
            Stream s = new MemoryStream();
            s.Write(bytes, 0, length);
            s.Position = 0;
            return s;
        }
    }
}
