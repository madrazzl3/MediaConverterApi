using System.Collections.Concurrent;
using MassTransit;
using Stratis.MediaConverterApi.Messages;
using Stratis.MediaConverterApi.Models;

namespace Stratis.MediaConverterApi;

public class MediaConverterAPI : IMediaConverterAPI
{
    private readonly IPublishEndpoint publishEndpoint;
    private readonly IConversionRequestsStorage requestsStorage;

    public MediaConverterAPI(IPublishEndpoint publishEndpoint, IConversionRequestsStorage requestsStorage)
    {
        this.publishEndpoint = publishEndpoint;
        this.requestsStorage = requestsStorage;
    }

    public async Task<string> RequestFormFilesConversion(IFormFileCollection files, CancellationToken cancellationToken)
    {
        var requestID = CreateRequestID();

        var downloadedFiles = new ConcurrentBag<string>();

        await Task.Run(() => Parallel.ForEachAsync(files, cancellationToken, async (file, token) =>
        {
            try
            {
                var filePath = Path.Combine(Path.GetTempPath(), file.FileName);
                await FileUtils.DownloadFormFileAsync(file, filePath, token);

                downloadedFiles.Add(filePath);
            }
            catch (Exception e)
            {
                return;
            }
        }));

        await publishEndpoint.Publish<IFormFilesConversionRequested>(new
        {
            RequestID = requestID,
            Files = downloadedFiles.ToList()
        });

        return requestID;
    }

    public async Task<string> RequestLinksConversion(LinksConversionRequest request, CancellationToken cancellationToken)
    {
        var requestID = CreateRequestID();

        await publishEndpoint.Publish<ILinksConversionRequested>(new
        {
            RequestID = requestID,
            Links = request.Links
        });

        return requestID;
    }

    public async Task<ConversionState?> AccessConversionState(string requestID, CancellationToken cancellationToken)
    {
        return await requestsStorage.GetConversionResults(requestID);
    }

    private string CreateRequestID()
    {
        return Guid.NewGuid().ToString();
    }
}