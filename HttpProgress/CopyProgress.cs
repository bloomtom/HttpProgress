using System;
using System.Collections.Generic;
using System.Text;

namespace HttpProgress
{
    /// <summary>
    /// A DTO for the rate of a stream copy operation.
    /// </summary>
    public class CopyProgress : ICopyProgress
    {
        /// <summary>
        /// The total time elapsed so far.
        /// </summary>
        public TimeSpan TransferTime { get; private set; }
        /// <summary>
        /// The instantaneous data transfer rate.
        /// </summary>
        public int BytesPerSecond { get; private set; }
        /// <summary>
        /// The total number of bytes transfered so far.
        /// </summary>
        public long BytesTransfered { get; private set; }
        /// <summary>
        /// The total number of bytes expected to be copied.
        /// </summary>
        public long ExpectedBytes { get; private set; }
        /// <summary>
        /// The percentage complete as a value 0-1.
        /// </summary>
        public double PercentComplete => ExpectedBytes <= 0 ? 0 : (double)BytesTransfered / ExpectedBytes;

        /// <summary>
        /// Create a new CopyRate instance.
        /// </summary>
        public CopyProgress(TimeSpan totalTransferTime, int bytesPerSecond, long bytesTotal, long expectedBytes)
        {
            TransferTime = totalTransferTime;
            BytesPerSecond = bytesPerSecond;
            BytesTransfered = bytesTotal;
            ExpectedBytes = expectedBytes;
        }
    }
}
