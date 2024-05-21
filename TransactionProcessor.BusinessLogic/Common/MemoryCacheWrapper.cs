using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionProcessor.BusinessLogic.Common
{
    public interface IMemoryCacheWrapper
    {
        Boolean TryGetValue<T>(Object Key, out T cache);
        void Set<T>(Object key, T cache, MemoryCacheEntryOptions entryOptions);
    }

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
