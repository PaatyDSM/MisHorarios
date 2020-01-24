using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;

using Windows.Data.Json;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Headers;

namespace HttpClientSample
{
    internal class HttpJsonContent : IHttpContent
    {
        private readonly IJsonValue jsonValue;

        public HttpContentHeaderCollection Headers { get; }

        public HttpJsonContent(IJsonValue jsonValue)
        {
            this.jsonValue = jsonValue ?? throw new ArgumentException("jsonValue cannot be null.");
            Headers = new HttpContentHeaderCollection
            {
                ContentType = new HttpMediaTypeHeaderValue("application/json")
            };
            Headers.ContentType.CharSet = "UTF-8";
        }

        public IAsyncOperationWithProgress<ulong, ulong> BufferAllAsync()
        {
            return AsyncInfo.Run<ulong, ulong>((_, progress) =>
            {
                return Task.Run(() =>
                {
                    ulong length = GetLength();

                    // Report progress.
                    progress.Report(length);

                    // Just return the size in bytes.
                    return length;
                });
            });
        }

        public IAsyncOperationWithProgress<IBuffer, ulong> ReadAsBufferAsync()
        {
            return AsyncInfo.Run<IBuffer, ulong>((_, progress) =>
            {
                return Task.Run(() =>
                {
                    DataWriter writer = new DataWriter();
                    writer.WriteString(jsonValue.Stringify());

                    // Make sure that the DataWriter destructor does not free the buffer.
                    IBuffer buffer = writer.DetachBuffer();

                    // Report progress.
                    progress.Report(buffer.Length);

                    return buffer;
                });
            });
        }

        public IAsyncOperationWithProgress<IInputStream, ulong> ReadAsInputStreamAsync()
        {
            return AsyncInfo.Run<IInputStream, ulong>(async (cancellationToken, progress) =>
            {
                InMemoryRandomAccessStream randomAccessStream = new InMemoryRandomAccessStream();
                DataWriter writer = new DataWriter(randomAccessStream);
                writer.WriteString(jsonValue.Stringify());

                uint bytesStored = await writer.StoreAsync().AsTask(cancellationToken).ConfigureAwait(false);

                // Make sure that the DataWriter destructor does not close the stream.
                writer.DetachStream();

                // Report progress.
                progress.Report(randomAccessStream.Size);

                return randomAccessStream.GetInputStreamAt(0);
            });
        }

        public IAsyncOperationWithProgress<string, ulong> ReadAsStringAsync()
        {
            return AsyncInfo.Run<string, ulong>((_, progress) =>
            {
                return Task.Run(() =>
                {
                    string jsonString = jsonValue.Stringify();

                    // Report progress (length of string).
                    progress.Report((ulong)jsonString.Length);

                    return jsonString;
                });
            });
        }

        public bool TryComputeLength(out ulong length)
        {
            length = GetLength();
            return true;
        }

        public IAsyncOperationWithProgress<ulong, ulong> WriteToStreamAsync(IOutputStream outputStream)
        {
            return AsyncInfo.Run<ulong, ulong>(async (cancellationToken, progress) =>
            {
                DataWriter writer = new DataWriter(outputStream);
                writer.WriteString(jsonValue.Stringify());
                uint bytesWritten = await writer.StoreAsync().AsTask(cancellationToken).ConfigureAwait(false);

                // Make sure that DataWriter destructor does not close the stream.
                writer.DetachStream();

                // Report progress.
                progress.Report(bytesWritten);

                return bytesWritten;
            });
        }

        public void Dispose()
        {
        }

        private ulong GetLength()
        {
            DataWriter writer = new DataWriter();
            writer.WriteString(jsonValue.Stringify());

            IBuffer buffer = writer.DetachBuffer();
            return buffer.Length;
        }
    }
}