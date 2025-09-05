using HackerNewsKevinLong.Server.Data;
using Microsoft.Extensions.Caching.Memory;

namespace HackerNewsKevinLong.Server.Wrappers
{
    public interface IMemoryCacheWrapper
    {
        object Set(object key, object value, MemoryCacheEntryOptions? options);
        bool TryGetValue(object key, out object? value);
    }
}