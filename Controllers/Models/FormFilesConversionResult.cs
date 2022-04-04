

using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace Stratis.MediaConverterApi.Models;

[SwaggerSchemaFilter(typeof(FormFilesConversionResultSchemaFilter))]
public class FormFilesConversionResult
{
    [JsonPropertyName("links")]
    public IDictionary<string, string> Links;
}