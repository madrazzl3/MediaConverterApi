
namespace Stratis.MediaConverterApi
{
    public interface IMediaStorage
    {
        public Task<string> Store(string filePath, CancellationToken cancellationToken);
    }
}