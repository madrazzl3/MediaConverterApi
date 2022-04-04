using Microsoft.Extensions.Caching.Memory;
using Stratis.MediaConverterApi;
using Stratis.MediaConverterApi.Models;

public class InMemoryConversionRequestsStorage : IConversionRequestsStorage
{
    private readonly IMemoryCache memoryCache;
    private readonly double requestCacheTimeoutMinutes = 5;

    public InMemoryConversionRequestsStorage(IMemoryCache memoryCache, MediaConverterSettings settings)
    {
        this.memoryCache = memoryCache;
        this.requestCacheTimeoutMinutes = settings.RequestsCacheTimeout;
    }

    public async Task StoreConversionResults(string requestID, ConversionState state)
    {
        memoryCache.Set(requestID, state, new MemoryCacheEntryOptions()
        {
            SlidingExpiration = TimeSpan.FromMinutes(requestCacheTimeoutMinutes)
        });
    }

    public async Task<ConversionState?> GetConversionResults(string requestID)
    {
        return (ConversionState)memoryCache.Get(requestID);
    }
}