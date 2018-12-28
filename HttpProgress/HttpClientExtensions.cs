using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HttpProgress
{
    /// <summary>
    /// Extensions the HttpClient class which give progress reporting support.
    /// </summary>
    public static class HttpClientExtensions
    {
        /// <summary>
        /// Perform an HTTP GET with progress reporting capabilities.
        /// </summary>
        /// <param name="client">Extension variable.</param>
        /// <param name="requestUri">The URI the request is sent to.</param>
        /// <param name="destination">The output stream to write the data response to.</param>
        /// <param name="progressReport">A progress action which fires every time the write buffer is cycled.</param>
        /// <param name="cancelToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The full HTTP response. Reading from the response stream is discouraged.</returns>
        public static async Task<HttpResponseMessage> GetAsync(this HttpClient client, string requestUri, Stream destination, IProgress<ICopyProgress> progressReport = null, CancellationToken cancelToken = default(CancellationToken))
        {
            var response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancelToken);
            long contentLength = response.Content.Headers.ContentLength ?? 0;

            using (var download = await response.Content.ReadAsStreamAsync())
            {
                if (progressReport == null)
                {
                    await download.CopyToAsync(destination);
                }
                else
                {
                    await download.CopyToAsync(
                        destination,
                        16384,
                        expectedTotalBytes: contentLength,
                        progressReport: progressReport,
                        cancelToken: cancelToken);
                }
                if (destination.CanSeek) { destination.Position = 0; }
            }
            return response;
        }

        /// <summary>
        /// Perform an HTTP PUT with progress reporting capabilities.
        /// </summary>
        /// <param name="client">Extension variable.</param>
        /// <param name="requestUri">The URI the request is sent to.</param>
        /// <param name="content">The stream to write out.</param>
        /// <param name="progressReport">A progress action which fires every time the write buffer is cycled.</param>
        /// <param name="expectedContentLength">Used for progress reporting, this can be used to override the content stream length if the stream type does not provide one.</param>
        /// <param name="cancelToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <param name="autoDisposeStream">When set true, the content stream is disposed automatically after being consumed.</param>
        /// <returns>The full HTTP response.</returns>
        public static async Task<HttpResponseMessage> PutAsync(this HttpClient client, string requestUri, Stream content, bool autoDisposeStream = false, IProgress<ICopyProgress> progressReport = null, long expectedContentLength = 0, CancellationToken cancelToken = default(CancellationToken))
        {
            using (var httpContent = new ProgressStreamContent(content, progressReport, autoDisposeStream))
            {
                return await client.PutAsync(requestUri, httpContent, cancelToken);
            }
        }

        /// <summary>
        /// Perform an HTTP POST with progress reporting capabilities.
        /// </summary>
        /// <param name="client">Extension variable.</param>
        /// <param name="requestUri">The URI the request is sent to.</param>
        /// <param name="content">The stream to write out.</param>
        /// <param name="progressReport">A progress action which fires every time the write buffer is cycled.</param>
        /// <param name="expectedContentLength">Used for progress reporting, this can be used to override the content stream length if the stream type does not provide one.</param>
        /// <param name="cancelToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <param name="autoDisposeStream">When set true, the content stream is disposed automatically after being consumed.</param>
        /// <returns>The full HTTP response.</returns>
        public static async Task<HttpResponseMessage> PostAsync(this HttpClient client, string requestUri, Stream content, bool autoDisposeStream = false, IProgress<ICopyProgress> progressReport = null, long expectedContentLength = 0, CancellationToken cancelToken = default(CancellationToken))
        {
            using (var httpContent = new ProgressStreamContent(content, progressReport, autoDisposeStream))
            {
                return await client.PostAsync(requestUri, httpContent, cancelToken);
            }
        }
    }
}
