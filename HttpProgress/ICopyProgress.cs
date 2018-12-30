using System;

namespace HttpProgress
{
    /// <summary>
    /// An interface for CopyProgress.
    /// </summary>
    public interface ICopyProgress
    {
        /// <summary>
        /// The instantaneous data transfer rate.
        /// </summary>
        long BytesPerSecond { get; }
        /// <summary>
        /// The total number of bytes transfered so far.
        /// </summary>
        long BytesTransfered { get; }
        /// <summary>
        /// The total number of bytes expected to be copied.
        /// </summary>
        long ExpectedBytes { get; }
        /// <summary>
        /// The percentage complete as a value 0-1.
        /// </summary>
        double PercentComplete { get; }
        /// <summary>
        /// The total time elapsed so far.
        /// </summary>
        TimeSpan TransferTime { get; }
    }
}