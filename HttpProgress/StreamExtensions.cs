using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HttpProgress
{
    /// <summary>
    /// Extensions for System.IO.Stream to support progress reporting on CopyToAsync.
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Provides a stream copy operation which supports IProgress.
        /// </summary>
        /// <param name="source">The source stream. Must support reading.</param>
        /// <param name="destination">The destination stream. Must support writing.</param>
        /// <param name="bufferSize">The size of the buffer to allocate in bytes. Sane values are typically 4096-81920. Setting a buffer of more than ~85k is likely to degrade performance.</param>
        /// <param name="expectedTotalBytes">The number of bytes expected. If set to greater than zero, this will override source.Length for progress calculations.</param>
        /// <param name="progress">An IProgress object that will be used to report progress.</param>
        /// <param name="cancelToken">A typical cancellation token.</param>
        /// <returns></returns>
        public static async Task CopyToAsync(this Stream source, Stream destination, int bufferSize = 32768, long expectedTotalBytes = 0, IProgress<ICopyProgress> progress = null, CancellationToken cancelToken = default(CancellationToken))
        {
            if (source == null) { throw new ArgumentNullException("source"); }
            if (!source.CanRead) { throw new ArgumentException("Source stream must be readable.", "source"); }
            if (destination == null) { throw new ArgumentNullException("destination"); }
            if (!destination.CanWrite) { throw new ArgumentException("Destination stream must be writable.", "destination"); }
            if (bufferSize < 0) { throw new ArgumentOutOfRangeException(nameof(bufferSize)); }

            expectedTotalBytes = expectedTotalBytes >= 0 ? source.Length : 0;

            var buffer = new byte[bufferSize];
            long totalBytesRead = 0;
            int bytesRead;

            var totalTime = new System.Diagnostics.Stopwatch();
            var singleTime = new System.Diagnostics.Stopwatch();
            totalTime.Start();
            singleTime.Start();

            while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancelToken).ConfigureAwait(false)) != 0)
            {
                await destination.WriteAsync(buffer, 0, bytesRead, cancelToken).ConfigureAwait(false);
                totalBytesRead += bytesRead;
                progress?.Report(new CopyProgress(totalTime.Elapsed, (int)(bytesRead * TimeSpan.TicksPerSecond / singleTime.ElapsedTicks), totalBytesRead, expectedTotalBytes));
                singleTime.Restart();

                if (cancelToken.IsCancellationRequested) { break; }
            }
        }
    }
}
