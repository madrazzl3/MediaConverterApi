using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Stratis.MediaConverterApi.Models;

namespace Stratis.MediaConverterApi.Controllers;

#pragma warning disable CS1591

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
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

    /// <summary>
    /// Converts files to the predefined file format.
    /// </summary>
    /// <param name="files">Files to be converted. The file name must be unique within request.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>A dictionary, where key is a file name and value is a link to the converted file.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /convertFiles
    ///     Content-Type: multipart/form-data; boundary=---------------------------735323031399963166993862150
    ///     -----------------------------735323031399963166993862150
    ///     Content-Disposition: form-data; name="GoldenBox"; filename="goldenbox.gif"
    ///     Content-Type: image/gif
    ///
    ///     ###########Content of goldenbox.gif.###########
    ///
    ///     -----------------------------735323031399963166993862150
    ///     Content-Disposition: form-data; name="SwordPromo"; filename="sword.webm"
    ///     Content-Type: video/webm
    ///
    ///     ###########Content of sword.webm.###########
    ///
    ///     -----------------------------735323031399963166993862150--
    ///
    /// </remarks>
    /// <response code="200">Returns the dictionary of source filenames and links to converted media files.</response>
    /// <response code="204">If no files were provided.</response>
    [HttpPost("/convertFiles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<FormFilesConversionResult>> ConvertFiles(IFormFileCollection files, CancellationToken cancellationToken = default(CancellationToken))
    {
        if (files.Count == 0)
        {
            return NoContent();
        }

        return Ok(new FormFilesConversionResult()
        {
            Links = await MapAsync(files, key => key.FileName, ConvertFormFile, cancellationToken)
        });
    }


    /// <summary>
    /// Converts files from links to the predefined file format.
    /// </summary>
    /// <param name="request">Links to files to be converted.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>A dictionary, where key is a link to the source file and value is a link to the converted file.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /convertLinks
    ///     {
    ///         [
    ///             "https://i.ibb.co/s9hbRXz/37-Cryborg.gif",
    ///             "https://domain.com/somevideo.webm"
    ///         ]
    ///     }
    ///
    /// </remarks>
    /// <response code="200">Returns the dictionary of the source filenames and links to the converted media files.</response>
    /// <response code="204">If no files were provided.</response>
    [HttpPost("/convertLinks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<LinksConversionResult>> ConvertLinks([FromBody] LinksConversionRequest request, CancellationToken cancellationToken = default(CancellationToken))
    {
        var links = request.Links;

        if (links.Count == 0)
        {
            return NoContent();
        }

        return Ok(new LinksConversionResult()
        {
            Links = await MapAsync(links, key => key, ConvertLink, cancellationToken)
        });
    }

    private async Task<ConcurrentDictionary<KeyMapType, ValueType>> MapAsync<KeyType, KeyMapType, ValueType>(
        IEnumerable<KeyType> keys,
        Func<KeyType, KeyMapType> keyTransformation,
        Func<KeyType, CancellationToken,
        Task<ValueType>> mapper,
        CancellationToken cancellationToken)
        where KeyType : notnull
        where KeyMapType : notnull
    {
        ConcurrentDictionary<KeyMapType, ValueType> results = new ConcurrentDictionary<KeyMapType, ValueType>();

        await Task.Run(() => Parallel.ForEachAsync(keys, cancellationToken, async (key, token) =>
        {
            try
            {
                var newKey = keyTransformation(key);
                results[newKey] = await mapper(key, token);
            }
            catch (Exception e)
            {
                _logger.LogError(0, e, "Error occured during convertion of the key: {key}", key);
                return;
            }
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

#pragma warning restore CS1591