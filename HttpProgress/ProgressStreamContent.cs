﻿using System;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;

namespace HttpProgress
{
    /// <summary>
    /// An HttpContent which supports an event action for send operations in an HttpClient.
    /// Mostly lifted from a post made on SO by Bruno Zell
    /// </summary>
    public class ProgressStreamContent : HttpContent
    {
        private const int defaultBufferSize = 16384;

        private Stream content;
        private readonly int bufferSize;
        private readonly long expectedContentLength;
        private readonly bool handleStreamDispose = false;
        private bool contentConsumed;
        private readonly IProgress<ICopyProgress> progressReport;

        /// <summary>
        /// Basic constructor which uses a default bufferSize and a zero expectedContentLength.
        /// </summary>
        /// <param name="content">The stream content to write.</param>
        /// <param name="progressReport">A progress action which fires every time the write buffer is cycled.</param>
        /// <param name="handleStreamDispose">When set true, the content stream is disposed when this object is disposed.</param>
        public ProgressStreamContent(Stream content, IProgress<ICopyProgress> progressReport, bool handleStreamDispose) : this(content, defaultBufferSize, 0, progressReport, handleStreamDispose) { }

        /// <summary>
        /// Constructor which allows configuration of all parameters.
        /// </summary>
        /// <param name="content">The source stream to read from.</param>
        /// <param name="bufferSize">The size of the buffer to allocate in bytes. Sane values are typically 4096-81920. Setting a buffer of more than ~85k is likely to degrade performance.</param>
        /// <param name="expectedContentLength">Overrides the content stream length if the stream type does not provide one. Used for progress reporting.</param>
        /// <param name="progressReport">A progress action which fires every time the write buffer is cycled.</param>
        /// <param name="handleStreamDispose">When set true, the content stream is disposed when this object is disposed.</param>
        public ProgressStreamContent(Stream content, int bufferSize, long expectedContentLength, IProgress<ICopyProgress> progressReport, bool handleStreamDispose)
        {
            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException("bufferSize");
            }

            this.content = content ?? throw new ArgumentNullException("content");
            this.handleStreamDispose = handleStreamDispose;
            this.bufferSize = bufferSize;
            this.expectedContentLength = expectedContentLength;
            this.progressReport = progressReport;
        }

        /// <summary>
        /// Copies the source content stream into the given destination stream.
        /// </summary>
        /// <param name="stream">The destination stream to write to.</param>
        /// <param name="context">Transportation context.</param>
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            if (stream == null) { throw new ArgumentNullException("stream"); }

            PrepareContent();

            return Task.Run(() =>
            {
                var totalTime = new System.Diagnostics.Stopwatch();
                var singleTime = new System.Diagnostics.Stopwatch();
                totalTime.Start();
                singleTime.Start();

                var buffer = new byte[bufferSize];
                long streamLength = content.CanSeek ? content.Length : 0;
                long size = expectedContentLength > 0 ? expectedContentLength : streamLength;
                long uploaded = 0;

                while (true)
                {
                    var length = content.Read(buffer, 0, buffer.Length);
                    uploaded += length;
                    if (length <= 0) { break; }

                    stream.Write(buffer, 0, length);

                    long singleElapsed = Math.Max(1, singleTime.ElapsedTicks);
                    singleTime.Restart();

                    progressReport?.Report(new CopyProgress(totalTime.Elapsed, length * TimeSpan.TicksPerSecond / singleElapsed, uploaded, size));
                }
            });
        }

        /// <summary>
        /// Returns the http content length.
        /// </summary>
        protected override bool TryComputeLength(out long length)
        {
            if (content.CanSeek)
            {
                length = content.Length;
                return true;
            }
            length = 0;
            return false;
        }

        /// <summary>
        /// Disposes the stream handled by this object if handleStreamDispose is true.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && handleStreamDispose)
            {
                content.Dispose();
            }
            base.Dispose(disposing);
        }

        private void PrepareContent()
        {
            if (contentConsumed)
            {
                if (content.CanSeek)
                {
                    content.Position = 0;
                }
                else
                {
                    throw new InvalidOperationException("Stream already read. Cannot seek on this stream type.");
                }
            }

            contentConsumed = true;
        }
    }
}
