using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace Stratis.MediaConverterApi.Models;

[SwaggerSchemaFilter(typeof(LinksConversionResultSchemaFilter))]
public class LinksConversionResult
{
    [JsonPropertyName("links")]
    public IDictionary<string, string> Links { get; set; }
}