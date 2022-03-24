namespace Stratis.MediaConverterApi
{
    public interface IMediaConverter
    {
        public Task<string> Convert(string filePath, CancellationToken cancellationToken);
    }
}