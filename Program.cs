var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<Stratis.MediaConverterApi.MediaConverterSettings>(
    new Stratis.MediaConverterApi.MediaConverterSettings()
    {
        BlobConnectionString = Environment.GetEnvironmentVariable("BLOB_CONNECTION") ?? "",
        BlobContainerName = "nftwallet",
        CacheDatabaseFileName = Environment.GetEnvironmentVariable("CACHE_DB") ?? "~/images-cache.db",
        FFmpegExecutablePath = Environment.GetEnvironmentVariable("FFMPEG_EXECUTABLE_PATH") ?? "/usr/bin/ffmpeg",
        ConverterTargetExtension = "mp4"
    }
);

builder.Services.AddScoped<Stratis.MediaConverterApi.IHashProvider, Stratis.MediaConverterApi.SHA256HashProvider>();
builder.Services.AddScoped<Stratis.MediaConverterApi.IMediaCache, Stratis.MediaConverterApi.LiteDBMediaCache>();
builder.Services.AddScoped<Stratis.MediaConverterApi.IMediaConverter, Stratis.MediaConverterApi.FFmpegMediaConverter>();
builder.Services.AddScoped<Stratis.MediaConverterApi.IMediaStorage, Stratis.MediaConverterApi.BlobMediaStorage>();

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
