using Microsoft.Extensions.Caching.Memory;
using HackerNewsKevinLong.Server.Data;
using System.Text.Json;
using HackerNewsKevinLong.Server.Wrappers;

namespace HackerNewsKevinLong.Server.Services
{
    public class HackerNewsItemService : IHackerNewsItemService
    {
        private IHttpClientWrapper _Client;
        private IMemoryCacheWrapper _Cache;

        public HackerNewsItemService(IHttpClientWrapper Client, IMemoryCacheWrapper Cache)
        {
            _Client = Client;
            _Cache = Cache;
        }

        public async Task<IList<HackerNewsItem>> SearchItems(int itemCount, int? startIndex = null, string? searchText = null)
        {
            string cacheKey = itemCount + "_" + startIndex;
            string[] searchWords = new string[0];
            List<Func<HackerNewsItem, bool>> filters = new List<Func<HackerNewsItem, bool>>();

            if (searchText != null)
            {
                cacheKey += "_" + searchText.Trim().ToLower();
                searchWords = searchText.Split(" ").Where(s => s.Length != 0).ToArray();

                //add all conditions to array which will then be processed in GetItemsRecursive.
                searchText = searchText.Trim().ToLower();
                Func<HackerNewsItem, bool> searchFilter = (Item) => { return true; };
                filters.Add(searchFilter);

                foreach (string word in searchWords)
                {
                    Func<HackerNewsItem, bool> wordFilter = (Item) => { return Item.Title.ToLower().Contains(word); };
                    filters.Add(wordFilter);
                }
            }

            //to do: refactor cache. You need to cache the entire dataset, not specific search queries!
            var cacheValue = CheckCacheForValue(cacheKey);
            if (cacheValue != null)
            {
                return cacheValue;
            }

            IList<HackerNewsItem> results = await GetItemsRecursive(itemCount, startIndex, new List<HackerNewsItem>(), filters);

            //Note: arbitrarily, I've decided to keep the cache active for a day. This would de determined by business requirements in a professional setting.
            _Cache.Set(cacheKey, results, new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromDays(1) });

            return results;
        }

        private IList<HackerNewsItem>? CheckCacheForValue(string key)
        {
            if (_Cache.TryGetValue(key, out object? cacheData))
            {
                if (cacheData != null)
                {
                    return cacheData as List<HackerNewsItem>;
                }
            }

            return null;
        }

        //Note: I am aware that this could fail if the recursive call stack exceeds the maximum call stack size, but I do not anticipate that happening.
        //If that were a concern, this would need to be changed to not be recursive using a while loop.
        private async Task<IList<HackerNewsItem>> GetItemsRecursive(int itemCount, int? startIndex, IList<HackerNewsItem> items, List<Func<HackerNewsItem, bool>>? filters = null)
        {
            if (itemCount <= 0)
            {
                throw new Exception("Item Count cannot be 0 or below.");
            }

            if (startIndex == null)
            {
                int maxId = await GetMaxId();
                startIndex = maxId;
            }

            if (startIndex <= 0)
            {
                return items;
            }

            if (items == null)
            {
                items = new List<HackerNewsItem>();
            }

            IList<HackerNewsItem> newItems = new List<HackerNewsItem>();

            //get itemCount number of requests at a time, and then wait for them to finish.
            List<Task<string>> requests = new List<Task<string>>();
            for (int i = 0; i < itemCount; i++)
            {
                string url = String.Format(Constants.HackerNewsItemUrl, startIndex - i);
                Task<string> newsItemMessage = _Client.GetStringAsync(url);
                requests.Add(newsItemMessage);
            }
            Task.WaitAll(requests.ToArray());

            //Deserialize string results into tasks and then ensure the resulting news items meet the criteria.
            foreach (var stringTask in requests)
            {
                string stringResponse = stringTask.Result;
                if (stringResponse != null)
                {
                    HackerNewsItem? item = JsonSerializer.Deserialize<HackerNewsItem?>(stringResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (item != null && item.Type == "story" && item.Url != null && item.Deleted != true)
                    {
                        newItems.Add((HackerNewsItem)item);
                    }
                }
            }

            //apply filters if any.
            if (filters != null)
            {
                IEnumerable<HackerNewsItem> newsItemsEnumerable = newItems.AsEnumerable();
                foreach (var filter in filters)
                {
                    newsItemsEnumerable = newsItemsEnumerable.Where(filter);
                }

                //in order to be performant, we do not convert to a list until the end.
                newItems = newsItemsEnumerable.ToList();
            }

            items = items.Concat(newItems).ToList();

            //check if correct number of items has been achieved. If not, continue searching.
            if (items.Count() >= itemCount)
            {
                return (List<HackerNewsItem>)items;
            }
            else return await GetItemsRecursive(itemCount, startIndex - itemCount, items, filters);
        }

        private async Task<int> GetMaxId()
        {
            string maxIdUrl = Constants.MaxIdUrl;
            string responseBody = await _Client.GetStringAsync(maxIdUrl);

            return Int32.Parse(responseBody);
        }
    }
}

