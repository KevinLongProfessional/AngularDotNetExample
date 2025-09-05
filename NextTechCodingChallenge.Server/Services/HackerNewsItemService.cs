using Microsoft.Extensions.Caching.Memory;
using HackerNewsKevinLong.Server.Data;
using System.Text.Json;

namespace HackerNewsKevinLong.Server.Services
{
    //to do; extract interface and use dependency injection for it.
    public class HackerNewsItemService : IHackerNewsItemService
    {
        private HttpClient _Client;
        private IMemoryCache _Cache;

        public HackerNewsItemService(HttpClient Client, IMemoryCache Cache)
        {
            _Client = Client;
            _Cache = Cache;
        }

        public async Task<IList<HackerNewsItem>> GetLatestItems(int itemCount, int? startIndex, IList<HackerNewsItem> items)
        {
            string cacheKey = itemCount + "_" + startIndex;
            var cacheValue = CheckCacheForValue(cacheKey);
            if (cacheValue != null)
            {
                return cacheValue;
            }

            IList<HackerNewsItem> results = await GetItemsRecursive(itemCount, startIndex, new List<HackerNewsItem>());
            _Cache.Set(cacheKey, results, new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromDays(1) });
            return results;
        }


        public async Task<IList<HackerNewsItem>> SearchItems(int itemCount, int? startIndex, IList<HackerNewsItem> items, string searchText)
        {
            string cacheKey = itemCount + "_" + startIndex + "_" + searchText.Trim().ToLower();
            var cacheValue = CheckCacheForValue(cacheKey);
            if (cacheValue != null)
            {
                return cacheValue;
            }

            string[] searchWords = searchText.Split(" ").Where(s => s.Length != 0).ToArray();

            //add all conditions to array which will then be processed in GetItemsRecursive.
            searchText = searchText.Trim().ToLower();
            List<Func<HackerNewsItem, bool>> filters = new List<Func<HackerNewsItem, bool>>();
            Func<HackerNewsItem, bool> searchFilter = (Item) => { return true; };
            filters.Add(searchFilter);

            foreach (string word in searchWords)
            {
                Func<HackerNewsItem, bool> wordFilter = (Item) => { return true; };
                filters.Add(wordFilter);
            }

            IList<HackerNewsItem> results = await GetItemsRecursive(itemCount, startIndex, new List<HackerNewsItem>(), filters);

            //Note: arbitrarily, I've decided to keep the cache active for a day. This would de determined by business requirements in a professional setting.
            _Cache.Set(cacheKey, results, new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromDays(1) });

            return results;
        }

        private IList<HackerNewsItem>? CheckCacheForValue(string key)
        {
            if (_Cache.TryGetValue(key, out IList<HackerNewsItem>? cacheData))
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
            List<Task<HttpResponseMessage>> requests = new List<Task<HttpResponseMessage>>();
            for (int i = 0; i < itemCount; i++)
            {
                string url = String.Format("https://hacker-news.firebaseio.com/v0/item/{0}.json", startIndex - i);
                Task<HttpResponseMessage> newsItemMessage = _Client.GetAsync(url);
                requests.Add(newsItemMessage);
            }
            Task.WaitAll(requests.ToArray());

            //Convert the previous requests to strings and wait for those operations to finish.
            List<Task<string>> readAsStringTasks = new List<Task<string>>();
            for (int i = 0; i < requests.Count; i++)
            {
                readAsStringTasks.Add(requests[i].Result.Content.ReadAsStringAsync());
            }
            Task.WaitAll(readAsStringTasks.ToArray());

            //Deserialize string results into tasks and then ensure the resulting news items meet the criteria.
            foreach (var stringTask in readAsStringTasks)
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
            else return await GetItemsRecursive(itemCount, startIndex - itemCount, items);
        }

        private async Task<int> GetMaxId()
        {
            string maxIdUrl = "https://hacker-news.firebaseio.com/v0/maxitem.json";
            using var client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(maxIdUrl);

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();

            return Int32.Parse(responseBody);
        }
    }
}

