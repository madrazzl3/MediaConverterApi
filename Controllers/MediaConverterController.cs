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
    private readonly IMediaConverterAPI mediaConverterAPI;

    public MediaConverterController(ILogger<MediaConverterController> logger, IMediaConverterAPI mediaConverterAPI)
    {
        _logger = logger;
        this.mediaConverterAPI = mediaConverterAPI;
    }

    /// <summary>
    /// Requests conversion of files to the predefined file format.
    /// </summary>
    /// <param name="files">Files to be converted. The file name must be unique within request.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>Request ID</returns>
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
    /// <response code="200">Returns request ID.</response>
    /// <response code="204">If no files were provided.</response>
    [HttpPost("/convertFiles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<ConversionStartedResponse>> ConvertFiles(IFormFileCollection files, CancellationToken cancellationToken = default(CancellationToken))
    {
        var formFiles = HttpContext.Request.Form.Files;

        if (formFiles.Count == 0)
        {
            return NoContent();
        }

        return Ok(new ConversionStartedResponse()
        {
            RequestID = await mediaConverterAPI.RequestFormFilesConversion(formFiles, cancellationToken)
        });
    }


    /// <summary>
    /// Requests conversion from the links to the predefined file format.
    /// </summary>
    /// <param name="request">Links to files to be converted.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>Request ID.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /convertLinks
    ///     {
    ///         "links" : [
    ///             "https://i.ibb.co/s9hbRXz/37-Cryborg.gif",
    ///             "https://domain.com/somevideo.webm"
    ///         ]
    ///     }
    ///
    /// </remarks>
    /// <response code="200">Returns request ID.</response>
    /// <response code="204">If no files were provided.</response>
    [HttpPost("/convertLinks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<ConversionStartedResponse>> ConvertLinks([FromBody] LinksConversionRequest request, CancellationToken cancellationToken = default(CancellationToken))
    {
        var links = request.Links;

        if (links == null || links.Count == 0)
        {
            return NoContent();
        }

        return Ok(new ConversionStartedResponse()
        {
            RequestID = await mediaConverterAPI.RequestLinksConversion(request, cancellationToken)
        });
    }

    /// <summary>
    /// Gets state of file conversion request.
    /// </summary>
    /// <param name="requestID">Request ID</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>Request ID</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /result?requestId=c89574ce-b4ad-4437-9545-f7550ef88a11
    ///
    /// </remarks>
    /// <response code="200">Returns the dictionary of source items and links to converted media files.</response>
    /// <response code="204">If no files were converted yet.</response>
    /// <response code="206">If only part of the files were converted.</response>
    /// <response code="404">If request id wasn't found.</response>
    [HttpGet("/result")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status206PartialContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LinksConversionResult>> GetConversionResult([FromQuery(Name = "requestId")] string requestID, CancellationToken cancellationToken = default(CancellationToken))
    {
        var state = await mediaConverterAPI.AccessConversionState(requestID, cancellationToken);

        if (state == null)
        {
            return NotFound();
        }

        var result = new LinksConversionResult()
        {
            Links = state.ConvertedEntries
        };

        if (state.Status == ConversionState.ConversionStatus.Pending)
        {
            if (result.Links.Count > 0)
            {
                return StatusCode(StatusCodes.Status206PartialContent, result);
            }
            else
            {
                return NoContent();
            }
        }
        else
        {
            return Ok(result);
        }
    }
}

#pragma warning restore CS1591