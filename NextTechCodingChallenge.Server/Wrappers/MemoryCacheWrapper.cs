using Microsoft.Extensions.Caching.Memory;

namespace HackerNewsKevinLong.Server.Wrappers
{
    public class MemoryCacheWrapper : IMemoryCacheWrapper
    {
        public IMemoryCache _Cache;

        public MemoryCacheWrapper(IMemoryCache Cache)
        {
            _Cache = Cache;
        }

        public object Set(object key, object value, MemoryCacheEntryOptions? options)
        {
            return _Cache.Set(key, value, options);
        }

        public bool TryGetValue(object key, out dynamic? value)
        {
            bool result = _Cache.TryGetValue(key, out object? returnedValue);
            value = returnedValue;
            return result;
        }

    }
}
