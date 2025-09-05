using static System.Net.WebRequestMethods;

namespace HackerNewsKevinLong.Server
{
    public static class Constants
    {
        public readonly static string HackerNewsItemUrl = "https://hacker-news.firebaseio.com/v0/item/{0}.json";

        public readonly static string MaxIdUrl = "https://hacker-news.firebaseio.com/v0/maxitem.json";
    }
}
