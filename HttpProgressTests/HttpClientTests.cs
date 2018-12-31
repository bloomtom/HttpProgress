using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using HttpProgress;
using System.Net.Http;
using RichardSzalay.MockHttp;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace HttpProgressTests
{
    [TestClass]
    public class HttpClientTests
    {
        private const int streamLength = 1024 * 1024 * 10;
        private const string testPoint = "http://localhost/stream/test";

        [TestMethod]
        public async Task TestGet()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Get, testPoint)
                .Respond("application/octet-stream", GenerateStream(streamLength));

            int progressEventCounter = 0;
            var progress = new NaiveProgress<ICopyProgress>(x =>
            {
                progressEventCounter++;
            });

            var client = new HttpClient(mockHttp);
            using (Stream s = new MemoryStream())
            {
                await client.GetAsync(testPoint, s, progress);

                Assert.IsTrue(progressEventCounter > 0);

                int bytesRead = 0;
                while(s.ReadByte() != -1)
                {
                    bytesRead++;
                }
                Assert.AreEqual(streamLength, bytesRead);
                Assert.AreEqual(streamLength, s.Length);
            }
        }

        [TestMethod]
        public async Task TestPut()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Put, testPoint).With(new Func<HttpRequestMessage, bool>(x =>
            {
                var s = x.Content.ReadAsStreamAsync().Result;
                Assert.AreEqual(streamLength, s.Length);
                return true;
            })).Respond(HttpStatusCode.OK);

            int progressEventCounter = 0;
            long lastBytesTransferred = 0;
            double lastProgress = 0;
            var progress = new NaiveProgress<ICopyProgress>(x =>
            {
                progressEventCounter++;
                Assert.IsTrue(x.BytesTransferred > lastBytesTransferred);
                lastBytesTransferred = x.BytesTransferred;
                lastProgress = x.PercentComplete;
            });

            var client = new HttpClient(mockHttp);
            using (Stream s = GenerateStream(streamLength))
            {
                var result = await client.PutAsync(testPoint, s, false, progress);

                s.Position = 0; // Should be able to do this because stream not closed yet.
                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
                Assert.IsTrue(progressEventCounter > 0);
                Assert.AreEqual(1, lastProgress);
            }
            using (Stream s = GenerateStream(streamLength))
            {
                var result = await client.PutAsync(testPoint, s, true, new Progress<ICopyProgress>((x) => { }));

                Assert.ThrowsException<ObjectDisposedException>(() => { s.Position = 0; });
                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            }
        }

        [TestMethod]
        public async Task TestPost()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, testPoint).With(new Func<HttpRequestMessage, bool>(x =>
            {
                var s = x.Content.ReadAsStreamAsync().Result;
                Assert.AreEqual(streamLength, s.Length);
                return true;
            })).Respond(HttpStatusCode.OK);

            int progressEventCounter = 0;
            long lastBytesTransferred = 0;
            double lastProgress = 0;
            var progress = new NaiveProgress<ICopyProgress>(x =>
            {
                progressEventCounter++;
                Assert.IsTrue(x.BytesTransferred > lastBytesTransferred);
                lastBytesTransferred = x.BytesTransferred;
                lastProgress = x.PercentComplete;
            });

            var client = new HttpClient(mockHttp);
            using (Stream s = GenerateStream(streamLength))
            {
                var result = await client.PostAsync(testPoint, s, false, progress);
                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
                Assert.IsTrue(progressEventCounter > 0);
                Assert.AreEqual(1, lastProgress);
            }
        }

        [TestMethod]
        public async Task TestNoEvent()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, testPoint).With(new Func<HttpRequestMessage, bool>(x =>
            {
                var s = x.Content.ReadAsStreamAsync().Result;
                Assert.AreEqual(streamLength, s.Length);
                return true;
            })).Respond(HttpStatusCode.OK);

            NaiveProgress<ICopyProgress> progress = null;

            var client = new HttpClient(mockHttp);
            using (Stream s = GenerateStream(streamLength))
            {
                var result = await client.PostAsync(testPoint, s, false, progress);
                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            }
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
