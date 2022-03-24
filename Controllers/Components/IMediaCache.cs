namespace Stratis.MediaConverterApi
{
    public interface IMediaCache
    {
        public Task<string?> TryFindByLink(string sourceLink);
        public Task CacheByLink(string sourceLink, string resultLink);

        public Task<string?> TryFindByHash(string hash);
        public Task CacheByHash(string hash, string resultLink);

    }
}