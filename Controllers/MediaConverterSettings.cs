namespace Stratis.MediaConverterApi
{
    public class MediaConverterSettings
    {
        public string BlobConnectionString { get; set; }

        public string BlobContainerName { get; set; }

        public string CacheDatabaseFileName { get; set; }

        public string FFmpegExecutablePath { get; set; }

        public double RequestsCacheTimeout { get; set; }
    }
}