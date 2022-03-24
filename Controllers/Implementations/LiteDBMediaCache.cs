using LiteDB;

namespace Stratis.MediaConverterApi
{
    public class LiteDBMediaCache : IMediaCache
    {
        public readonly string FileLinkCollectionName = "link";
        public readonly string FileHashCollectionName = "hash";

        private string LiteDBFileName { get; set; }

        public LiteDBMediaCache(MediaConverterSettings settings)
        {
            LiteDBFileName = settings.CacheDatabaseFileName;
        }

        public async Task<string?> TryFindByLink(string sourceLink)
        {
            using (var db = new LiteDatabase(LiteDBFileName))
            {
                var collection = db.GetCollection<LinkCacheItem>(FileLinkCollectionName);
                var cacheItem = collection.FindOne(x => x.SourceFileLink == sourceLink);
                return cacheItem?.ConvertedFileLink;
            }
        }

        public async Task CacheByLink(string sourceLink, string resultLink)
        {
            using (var db = new LiteDatabase(LiteDBFileName))
            {
                var collection = db.GetCollection<LinkCacheItem>(FileLinkCollectionName);

                var item = new LinkCacheItem
                {
                    SourceFileLink = sourceLink,
                    ConvertedFileLink = resultLink
                };

                collection.Insert(item);

                collection.EnsureIndex(x => x.SourceFileLink);
            }
        }

        public async Task<string?> TryFindByHash(string hash)
        {
            using (var db = new LiteDatabase(LiteDBFileName))
            {
                var collection = db.GetCollection<HashCacheItem>(FileHashCollectionName);
                var cacheItem = collection.FindOne(x => x.SourceFileHash == hash);
                return cacheItem?.ConvertedFileLink;
            }
        }

        public async Task CacheByHash(string hash, string resultLink)
        {
            using (var db = new LiteDatabase(LiteDBFileName))
            {
                var collection = db.GetCollection<HashCacheItem>(FileHashCollectionName);

                var item = new HashCacheItem
                {
                    SourceFileHash = hash,
                    ConvertedFileLink = resultLink
                };

                collection.Insert(item);

                collection.EnsureIndex(x => x.SourceFileHash);
            }
        }
    }

    public class LinkCacheItem
    {
        public string SourceFileLink { get; set; }
        public string ConvertedFileLink { get; set; }
    }

    public class HashCacheItem
    {
        public string SourceFileHash { get; set; }
        public string ConvertedFileLink { get; set; }
    }
}