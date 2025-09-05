using HackerNewsKevinLong.Server.Data;

namespace HackerNewsKevinLong.Server.Services
{
    public interface IHackerNewsItemService
    {
        Task<IList<HackerNewsItem>> GetLatestItems(int itemCount, int? startIndex, IList<HackerNewsItem> items);
        Task<IList<HackerNewsItem>> SearchItems(int itemCount, int? startIndex, IList<HackerNewsItem> items, string searchText);
    }
}