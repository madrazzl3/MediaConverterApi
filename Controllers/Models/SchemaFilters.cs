using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Stratis.MediaConverterApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class LinksConversionResultSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type != typeof(LinksConversionResult)) return;

        schema.Example = new OpenApiObject
        {
            ["links"] = new OpenApiObject
            {
                ["https://i.ibb.co/s9hbRXz/37-Cryborg.gif"] = new OpenApiString("https://stratisstorage.blob.core.windows.net/conversions/6bb3dcf9d9e247248e1bdaf1b27b4e47.mp4"),
                ["https://domain.com/somevideo.webm"] = new OpenApiString("https://stratisstorage.blob.core.windows.net/conversions/28b3dcf9d95647248e1bdaf1b27b4e47.mp4"),
            }
        };
    }
}

public class ConversionStartedResponseSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type != typeof(ConversionStartedResponse)) return;

        schema.Example = new OpenApiObject
        {
            ["requestId"] = new OpenApiString(Guid.NewGuid().ToString())
        };
    }
}

public class LinksConversionRequestSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type != typeof(LinksConversionRequest)) return;

        schema.Example = new OpenApiObject
        {
            ["links"] = new OpenApiArray
            {
                new OpenApiString("https://i.ibb.co/s9hbRXz/37-Cryborg.gif"),
                new OpenApiString("https://domain.com/somevideo.webm")
            }
        };
    }
}