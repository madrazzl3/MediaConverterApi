using System.Reflection;
using Microsoft.OpenApi.Models;
using Stratis.MediaConverterApi;
using Stratis.MediaConverterApi.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Media converter API",
        Description = "An ASP.NET Core Web API for converting media files to the appropriate format",
        Contact = new OpenApiContact
        {
            Name = "Stratis Platform",
            Url = new Uri("https://www.stratisplatform.com/contact/")
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://github.com/stratisproject/MediaConverterApi/blob/master/LICENSE")
        }
    });

    // using System.Reflection;
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    options.SchemaFilter<LinksConversionResultSchemaFilter>();
    options.SchemaFilter<FormFilesConversionResultSchemaFilter>();
    options.SchemaFilter<LinksConversionRequestSchemaFilter>();
});

builder.Services.AddSingleton<MediaConverterSettings>(
    new MediaConverterSettings()
    {
        BlobConnectionString = Environment.GetEnvironmentVariable("BLOB_CONNECTION") ?? "",
        BlobContainerName = Environment.GetEnvironmentVariable("BLOB_CONTAINER") ?? "nftwallet",
        CacheDatabaseFileName = Environment.GetEnvironmentVariable("CACHE_DB") ?? "media-cache.db",
        FFmpegExecutablePath = Environment.GetEnvironmentVariable("FFMPEG_EXECUTABLE_PATH") ?? "/usr/bin/ffmpeg"
    }
);

builder.Services.AddScoped<IHashProvider, SHA256HashProvider>();
builder.Services.AddScoped<IMediaCache, LiteDBMediaCache>();
builder.Services.AddScoped<IMediaConverter, FFmpegMediaConverter>();
builder.Services.AddScoped<IMediaStorage, BlobMediaStorage>();

builder.Services.AddMvc(options =>
{
    options.Filters.Add<OperationCancelledExceptionFilter>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
