using Stratis.MediaConverterApi.Models;

namespace Stratis.MediaConverterApi;

public interface IMediaConverterAPI
{
    public Task<string> RequestFormFilesConversion(IFormFileCollection files, CancellationToken cancellationToken);
    public Task<string> RequestLinksConversion(LinksConversionRequest request, CancellationToken cancellationToken);
    public Task<ConversionState?> AccessConversionState(string requestID, CancellationToken cancellationToken);
};