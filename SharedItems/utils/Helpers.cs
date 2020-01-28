using System;
using System.Threading;
using System.Threading.Tasks;

using Windows.Web.Http;

namespace HttpClientSample
{
    internal static class Helpers
    {
        internal static async Task<string> DisplayTextResultAsync(HttpResponseMessage response, CancellationToken token)
        {
            string responseBodyAsText = await response.Content.ReadAsStringAsync().AsTask(token).ConfigureAwait(false);

            token.ThrowIfCancellationRequested();

            // Insert new lines.
            return responseBodyAsText.Replace("<br>", Environment.NewLine);
        }
    }
}