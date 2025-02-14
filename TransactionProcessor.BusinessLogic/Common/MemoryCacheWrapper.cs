using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Caching.Memory;

namespace TransactionProcessor.BusinessLogic.Common
{
    public interface IMemoryCacheWrapper
    {
        Boolean TryGetValue<T>(Object Key, out T cache);
        void Set<T>(Object key, T cache, MemoryCacheEntryOptions entryOptions);
    }

    [ExcludeFromCodeCoverage]
    public class MemoryCacheWrapper : IMemoryCacheWrapper
    {
        private readonly IMemoryCache MemoryCache;

        public MemoryCacheWrapper(IMemoryCache memoryCache)
        {
            MemoryCache = memoryCache;
        }

        public void Set<T>(Object key, T cache, MemoryCacheEntryOptions entryOptions)
        {
            MemoryCache.Set(key, cache, entryOptions);
        }

        public Boolean TryGetValue<T>(Object Key, out T cache)
        {
            if (MemoryCache.TryGetValue(Key, out T cachedItem))
            {
                cache = cachedItem;
                return true;
            }
            cache = default(T);
            return false;
        }
    }
}
