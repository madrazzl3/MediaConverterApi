using FFmpeg.NET;

namespace Stratis.MediaConverterApi
{
    public class FFmpegMediaConverter : IMediaConverter
    {
        private Engine ConverterEngine { get; set; }
        private string TargetFileExtension { get; set; }
        public FFmpegMediaConverter(MediaConverterSettings settings)
        {
            ConverterEngine = new Engine(settings.FFmpegExecutablePath);
            TargetFileExtension = settings.ConverterTargetExtension;
        }

        public async Task<string> Convert(string filePath, CancellationToken cancellationToken)
        {
            var inputFile = new InputFile(filePath);

            var outputFilePath = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(filePath), TargetFileExtension);
            var outputFile = new OutputFile(outputFilePath);

            var options = new ConversionOptions()
            {

            };

            await ConverterEngine.ConvertAsync(inputFile, outputFile, options, cancellationToken);

            return outputFilePath;
        }
    }
}