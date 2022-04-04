using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace Stratis.MediaConverterApi.Models;

[SwaggerSchemaFilter(typeof(ConversionStartedResponseSchemaFilter))]
public class ConversionStartedResponse
{
    /// <summary>
    /// Conversion request ID that should be used for retrieving conversion results.
    /// </summary>
    /// <example>
    /// 4b049200-1d6b-42c7-9f87-0f8535c78179
    /// </example>
    [JsonPropertyName("requestId")]
    public string RequestID { get; set; }
}