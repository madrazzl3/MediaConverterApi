namespace Stratis.MediaConverterApi
{

    public class FileUtils
    {
        public static async Task DownloadFormFileAsync(IFormFile formFile, string filePath, CancellationToken cancellationToken)
        {
            using (var fs = new FileStream(filePath, FileMode.OpenOrCreate))
            {
                await formFile.CopyToAsync(fs, cancellationToken);
            }
        }

        public static async Task DownloadFileAsync(string link, string filePath, CancellationToken cancellationToken)
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(link, cancellationToken);
                using (var fs = new FileStream(filePath, FileMode.OpenOrCreate))
                {
                    await response.Content.CopyToAsync(fs, cancellationToken);
                }
            }
        }

        public static void DisposeTemporaryFile(string fileName)
        {
            try
            {
                if (System.IO.File.Exists(fileName))
                {
                    System.IO.File.Delete(fileName);
                }
            }
            catch (Exception)
            {
            }
        }
    }
}