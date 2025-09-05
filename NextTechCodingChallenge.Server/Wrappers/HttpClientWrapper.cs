namespace HackerNewsKevinLong.Server.Wrappers
{
    public class HttpClientWrapper : IHttpClientWrapper
    {
        private HttpClient _Client;

        public HttpClientWrapper(HttpClient client)
        {
            _Client = client;
        }

        public virtual async Task<string> GetStringAsync(string url)
        {
            return await _Client.GetStringAsync(url);
        }
    }
}
