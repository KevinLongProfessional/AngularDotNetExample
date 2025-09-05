
namespace HackerNewsKevinLong.Server.Wrappers
{
    public interface IHttpClientWrapper
    {
        Task<string> GetStringAsync(string url);
    }
}