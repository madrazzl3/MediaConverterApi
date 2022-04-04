using System.Collections.Concurrent;
using MassTransit;
using Stratis.MediaConverterApi.Messages;
using Stratis.MediaConverterApi.Models;

namespace Stratis.MediaConverterApi;

public class ConversionRequestsConsumer : IConsumer<IFormFilesConversionRequested>, IConsumer<ILinksConversionRequested>
{
    private readonly ILogger<ConversionRequestsConsumer> logger;
    private readonly IHashProvider hashProvider;
    private readonly IMediaCache mediaCache;
    private readonly IMediaConverter mediaConverter;
    private readonly IMediaStorage mediaStorage;
    private readonly IMediaConverterAPI mediaConverterAPI;
    private readonly IConversionRequestsStorage requestsStorage;

    public ConversionRequestsConsumer(ILogger<ConversionRequestsConsumer> logger, IConversionRequestsStorage requestsStorage, IHashProvider hashProvider, IMediaCache mediaCache, IMediaConverter mediaConverter, IMediaStorage mediaStorage)
    {
        this.logger = logger;
        this.hashProvider = hashProvider;
        this.mediaCache = mediaCache;
        this.mediaConverter = mediaConverter;
        this.mediaStorage = mediaStorage;
        this.requestsStorage = requestsStorage;
    }

    public async Task Consume(ConsumeContext<IFormFilesConversionRequested> context)
    {
        await ConvertAsync(context.Message.RequestID, context.Message.Files, key => Path.GetFileName(key), ConvertFormFile, context.CancellationToken);
    }

    public async Task Consume(ConsumeContext<ILinksConversionRequested> context)
    {
        await ConvertAsync(context.Message.RequestID, context.Message.Links, key => key, ConvertLink, context.CancellationToken);
    }

    private async Task ConvertAsync<KeyType>(
        string requestID,
        IEnumerable<KeyType> keys,
        Func<KeyType, string> keyTransformation,
        Func<KeyType, CancellationToken, Task<string>> mapper,
        CancellationToken cancellationToken)
        where KeyType : notnull
    {
        await requestsStorage.StoreConversionResults(requestID, new ConversionState()
        {
            Status = ConversionState.ConversionStatus.Pending
        });

        await Task.Run(() => Parallel.ForEachAsync(keys, cancellationToken, async (key, token) =>
        {
            try
            {
                var newKey = keyTransformation(key);
                var link = await mapper(key, token);

                var result = await requestsStorage.GetConversionResults(requestID);
                result.ConvertedEntries[newKey] = link;
                await requestsStorage.StoreConversionResults(requestID, result);

            }
            catch (Exception e)
            {
                logger.LogError(0, e, "Error occured during convertion of the key: {key}", key);
                return;
            }
        }));

        var result = await requestsStorage.GetConversionResults(requestID);
        result.Status = ConversionState.ConversionStatus.Complete;
        await requestsStorage.StoreConversionResults(requestID, result);
    }

    private async Task<string> ConvertFormFile(string file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length <= 0)
        {
            throw new ArgumentException("Form file can not be empty.");
        }

        try
        {
            var convertedMediaLink = await ProcessMediaFile(file, cancellationToken);
            return convertedMediaLink;
        }
        finally
        {
            FileUtils.DisposeTemporaryFile(file);
        }
    }

    private async Task<string> ConvertLink(string link, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(link))
        {
            throw new ArgumentException("Link can not be empty.");
        }

        var cachedMedia = await mediaCache.TryFindByLink(link);
        if (cachedMedia != null)
        {
            return cachedMedia;
        }

        var filePath = Path.GetTempFileName();

        try
        {
            await FileUtils.DownloadFileAsync(link, filePath, cancellationToken);
            var convertedMediaLink = await ProcessMediaFile(filePath, cancellationToken);
            await mediaCache.CacheByLink(link, convertedMediaLink);
            return convertedMediaLink;
        }
        finally
        {
            FileUtils.DisposeTemporaryFile(filePath);
        }
    }

    private async Task<string> ProcessMediaFile(string filePath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var fileHash = hashProvider.GetFileHash(filePath);

        var cachedMedia = await mediaCache.TryFindByHash(fileHash);
        if (cachedMedia != null)
        {
            return cachedMedia;
        }

        var convertedMediaFile = await mediaConverter.Convert(filePath, cancellationToken);

        try
        {
            var convertedMediaLink = await mediaStorage.Store(convertedMediaFile, cancellationToken);
            await mediaCache.CacheByHash(fileHash, convertedMediaLink);
            return convertedMediaLink;
        }
        finally
        {
            FileUtils.DisposeTemporaryFile(convertedMediaFile);
        }
    }
}