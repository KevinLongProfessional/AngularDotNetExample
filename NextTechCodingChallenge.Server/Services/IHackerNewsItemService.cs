using HackerNewsKevinLong.Server.Data;

namespace HackerNewsKevinLong.Server.Services
{
    public interface IHackerNewsItemService
    {
        Task<IList<HackerNewsItem>> SearchItems(int itemCount, int? startIndex, string? searchText);
    }
}