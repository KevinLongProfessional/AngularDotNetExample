using HackerNewsKevinLong.Server.Data;
using HackerNewsKevinLong.Server.Wrappers;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Text.Json;

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

        public async Task<IList<HackerNewsItem>> SearchItems(int itemCount, int startIndex, string? searchText = null)
        {
            string[] searchWords = new string[0];
            List<Func<HackerNewsItem, bool>> filters = new List<Func<HackerNewsItem, bool>>();

            if (searchText != null)
            {
                searchWords = searchText.Split(" ").Where(s => s.Length != 0).ToArray();

                searchText = searchText.Trim().ToLower();

                foreach (string word in searchWords)
                {
                    Func<HackerNewsItem, bool> wordFilter = (Item) => { return Item.Title != null && Item.Title.ToLower().Contains(word); };
                    filters.Add(wordFilter);
                }
            }

            IList<HackerNewsItem> results = await GetItems(itemCount, startIndex, new List<HackerNewsItem>(), filters);

            return results.Take(itemCount).ToList();
        }

        private HackerNewsItem? CheckCacheForValue(string key)
        {
            if (_Cache.TryGetValue(key, out object? cacheData))
            {
                if (cacheData != null)
                {
                    return cacheData as HackerNewsItem;
                }
            }

            return null;
        }

        private async Task<IList<HackerNewsItem>> GetItems(int itemCount, int startIndex, IList<HackerNewsItem> items, List<Func<HackerNewsItem, bool>> filters)
        {
            if (itemCount <= 0)
            {
                throw new Exception("Item Count cannot be 0 or below.");
            }

            if (items == null)
            {
                items = new List<HackerNewsItem>();
            }

            List<int> itemIndicies = JsonSerializer.Deserialize<List<int>>(await  _Client.GetStringAsync(Constants.HackerNewsLatestUrl));

            while (startIndex < itemIndicies.Count && items.Count < itemCount)
            {
                //get itemCount (arbitrary) number of requests at a time, and then wait for them to finish.
                List<Task<string>> requests = new List<Task<string>>();
                for (int i = 0; i < itemCount; i++)
                {
                    int id = itemIndicies[(int)startIndex + i];

                    var cacheValue = CheckCacheForValue(id.ToString());

                    if (cacheValue != null && filters != null)
                    {
                        if(IsItemValid(cacheValue, filters))
                        {
                            items.Add(cacheValue);
                        }
                    }
                    else
                    {
                        string url = String.Format(Constants.HackerNewsItemUrl, id);
                        Task<string> newsItemMessage = _Client.GetStringAsync(url);
                        requests.Add(newsItemMessage);
                    }
                }
                Task.WaitAll(requests.ToArray());

                //Deserialize string results into tasks and then ensure the resulting news items meet the criteria.
                foreach (var stringTask in requests)
                {
                    string stringResponse = stringTask.Result;
                    if (stringResponse != null)
                    {
                        HackerNewsItem? item = JsonSerializer.Deserialize<HackerNewsItem?>(stringResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (item != null)
                        {
                            //Note: arbitrarily, I've decided to keep the cache active for a day. This would de determined by business requirements in a professional setting.
                            _Cache.Set(item.Id.ToString(), item, new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromDays(1) });

                            if (item != null && filters != null)
                            {
                                if (IsItemValid(item, filters))
                                {
                                    items.Add(item);
                                }
                            }
                        }
                    }
                }

                startIndex += itemCount;
            }

            return (List<HackerNewsItem>)items;
        }

        private bool IsItemValid(HackerNewsItem item, List<Func<HackerNewsItem, bool>> filters)
        {

            foreach (var filter in filters)
            {
                //exit loop if a filter is failed.
                if (filter(item) == false)
                {
                    return false;
                }
            }

            return item.Type == "story" && item.Url != null && item.Deleted != true;
        }

        private async Task<int> GetMaxId()
        {
            string maxIdUrl = Constants.MaxIdUrl;
            string responseBody = await _Client.GetStringAsync(maxIdUrl);

            return Int32.Parse(responseBody);
        }
    }
}

