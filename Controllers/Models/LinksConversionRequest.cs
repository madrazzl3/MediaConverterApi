using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace Stratis.MediaConverterApi.Models;

[SwaggerSchemaFilter(typeof(LinksConversionRequestSchemaFilter))]
public class LinksConversionRequest
{
    [JsonPropertyName("links")]
    public IList<string> Links;
}