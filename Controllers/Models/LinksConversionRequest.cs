using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace Stratis.MediaConverterApi.Models;

[SwaggerSchemaFilter(typeof(LinksConversionRequestSchemaFilter))]
public class LinksConversionRequest
{
    [JsonPropertyName("links")]
    public List<string> Links { get; set; }
}