using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace Stratis.MediaConverterApi.Controllers;

[ApiController]
[Route("[controller]")]
public class MediaConverterController : ControllerBase
{
    private readonly ILogger<MediaConverterController> _logger;
    private readonly IHashProvider hashProvider;
    private readonly IMediaCache mediaCache;
    private readonly IMediaConverter mediaConverter;
    private readonly IMediaStorage mediaStorage;

    public MediaConverterController(ILogger<MediaConverterController> logger, IHashProvider hashProvider, IMediaCache mediaCache, IMediaConverter mediaConverter, IMediaStorage mediaStorage)
    {
        _logger = logger;
        this.hashProvider = hashProvider;
        this.mediaCache = mediaCache;
        this.mediaConverter = mediaConverter;
        this.mediaStorage = mediaStorage;
    }

    [HttpPost("/convertFiles")]
    public async Task<ActionResult<IDictionary<string, string>>> ConvertFiles(IFormFileCollection files, CancellationToken cancellationToken)
    {
        return await MapAsync(files, key => key.FileName, ConvertFormFile, cancellationToken);
    }

    [HttpPost("/convertLinks")]
    public async Task<ActionResult<IDictionary<string, string>>> ConvertLinks(IEnumerable<string> links, CancellationToken cancellationToken)
    {
        return await MapAsync(links, key => key, ConvertLink, cancellationToken);
    }

    private async Task<ConcurrentDictionary<KeyMapType, ValueType>> MapAsync<KeyType, KeyMapType, ValueType>(IEnumerable<KeyType> keys, Func<KeyType, KeyMapType> keyTransformation, Func<KeyType, CancellationToken, Task<ValueType>> mapper, CancellationToken cancellationToken)
    {
        ConcurrentDictionary<KeyMapType, ValueType> results = new ConcurrentDictionary<KeyMapType, ValueType>();

        await Task.Run(() => Parallel.ForEachAsync(keys, cancellationToken, async (key, token) =>
        {
            results[keyTransformation(key)] = await mapper(key, token);
        }));

        return results;
    }

    private async Task<string> ConvertFormFile(IFormFile formFile, CancellationToken cancellationToken)
    {
        if (formFile == null || formFile.Length <= 0)
        {
            throw new ArgumentException("Form file can not be empty.");
        }

        var filePath = Path.GetTempFileName();

        try
        {
            await FileUtils.DownloadFormFileAsync(formFile, filePath, cancellationToken);
            var convertedMediaLink = await ProcessMediaFile(filePath, cancellationToken);
            return convertedMediaLink;
        }
        finally
        {
            FileUtils.DisposeTemporaryFile(filePath);
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
