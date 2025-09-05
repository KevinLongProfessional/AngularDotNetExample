using Microsoft.AspNetCore.Mvc;
using HackerNewsKevinLong.Server.Data;
using HackerNewsKevinLong.Server.Services;

namespace HackerNewsKevinLong.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NewsController : ControllerBase
    {
        IHackerNewsItemService _Service;

        public NewsController(ILogger<NewsController> Logger, IHackerNewsItemService Service)
        {
            _Service = Service;
        }

        [HttpGet("GetNews")]
        public async Task<IEnumerable<HackerNewsItem>> Get(int itemCount, int? startIndex)
        {
            return await _Service.SearchItems(itemCount, startIndex, null);
        }


        [HttpGet("SearchNews")]
        public async Task<IEnumerable<HackerNewsItem>> Search(int itemCount, int? startIndex, string searchText)
        {
            return await _Service.SearchItems(itemCount, startIndex, searchText);
        }
    }
}
