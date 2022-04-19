using FFmpeg.NET;

namespace Stratis.MediaConverterApi
{
    public class FFmpegMediaConverter : IMediaConverter
    {
        readonly public string TargetFileExtension = "mp4";

        readonly public double MAX_FPS = 30.0;
        readonly public int MAX_WIDTH = 1600;
        readonly public int MAX_HEIGHT = 900;

        private Engine ConverterEngine { get; set; }
        public FFmpegMediaConverter(MediaConverterSettings settings)
        {
            ConverterEngine = new Engine(settings.FFmpegExecutablePath);
        }

        public async Task<string> Convert(string filePath, CancellationToken cancellationToken)
        {
            var inputFile = new InputFile(filePath);

            var outputFilePath = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetFileName(filePath), TargetFileExtension));
            var outputFile = new OutputFile(outputFilePath);

            var options = await ResolveConversionOptions(inputFile, cancellationToken);
            await ConverterEngine.ConvertAsync(inputFile, outputFile, options, cancellationToken);

            return outputFilePath;
        }

        private async Task<ConversionOptions> ResolveConversionOptions(InputFile inputFile, CancellationToken cancellationToken)
        {
            var metaData = await ConverterEngine.GetMetaDataAsync(inputFile, cancellationToken);

            var (width, height) = GetVideoSize(metaData.VideoData);
            var needResize = IsVideoTooLarge(width, height);
            var (newWidth, newHeight) = needResize ? ResolveDimensions(width, height) : (0, 0);

            var options = new ConversionOptions()
            {
                VideoFormat = FFmpeg.NET.Enums.VideoFormat.mp4,
                VideoFps = (metaData.VideoData.Fps > MAX_FPS) ? ((int)MAX_FPS) : null,
                VideoSize = needResize ? FFmpeg.NET.Enums.VideoSize.Custom : FFmpeg.NET.Enums.VideoSize.Default,
                CustomWidth = needResize ? newWidth : null,
                CustomHeight = needResize ? newHeight : null,
                ExtraArguments = "-c:v libx264 -profile:v baseline -strict -2"
            };

            return options;
        }

        private (int, int) ResolveDimensions(int width, int height)
        {
            double defaultRatio = (double)MAX_WIDTH / MAX_HEIGHT;

            double ratio = (double)width / height;

            int newWidth = 0;
            int newHeight = 0;

            if (ratio > defaultRatio)
            {
                newWidth = Math.Min(width, MAX_WIDTH);
                newHeight = (int)(newWidth / ratio);
            }
            else
            {
                newHeight = Math.Min(height, MAX_HEIGHT);
                newWidth = (int)(newHeight * ratio);
            }

            return (newWidth, newHeight);
        }

        private bool IsVideoTooLarge(int width, int height)
        {
            return width > MAX_WIDTH || height > MAX_HEIGHT;
        }

        private (int, int) GetVideoSize(MetaData.Video videoMetaData)
        {
            var size = videoMetaData.FrameSize.Split("x");
            int width, height;
            int.TryParse(size[0], out width);
            int.TryParse(size[1], out height);
            return (width, height);
        }
    }
}