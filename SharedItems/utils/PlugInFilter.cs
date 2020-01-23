using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace HttpClientSample
{
    public class PlugInFilter : IHttpFilter
    {
        private readonly IHttpFilter innerFilter;

        public PlugInFilter(IHttpFilter innerFilter)
        {
            this.innerFilter = innerFilter ?? throw new ArgumentException("innerFilter cannot be null.");
        }

        public IAsyncOperationWithProgress<HttpResponseMessage, HttpProgress> SendRequestAsync(HttpRequestMessage request)
        {
            return AsyncInfo.Run<HttpResponseMessage, HttpProgress>(async (cancellationToken, progress) =>
            {
                request.Headers.Add("Custom-Header", "CustomRequestValue");
                HttpResponseMessage response = await innerFilter.SendRequestAsync(request).AsTask(cancellationToken, progress).ConfigureAwait(false);

                cancellationToken.ThrowIfCancellationRequested();

                response.Headers.Add("Custom-Header", "CustomResponseValue");
                return response;
            });
        }

        public void Dispose()
        {
            //CA1063 Fix warning: innerFilter.Dispose(true);
            innerFilter.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
