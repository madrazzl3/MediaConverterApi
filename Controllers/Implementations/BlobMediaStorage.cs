using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Stratis.MediaConverterApi
{
    public class BlobMediaStorage : IMediaStorage
    {

        private BlobContainerClient BlobContainerClientInstance { get; set; }

        public BlobMediaStorage(MediaConverterSettings settings)
        {
            var serviceClient = new BlobServiceClient(settings.BlobConnectionString);
            BlobContainerClientInstance = serviceClient.GetBlobContainerClient(settings.BlobContainerName);
        }

        public async Task<string> Store(string filePath, CancellationToken cancellationToken)
        {
            await BlobContainerClientInstance.CreateIfNotExistsAsync(PublicAccessType.Blob, null, null, cancellationToken);
            var blobClient = BlobContainerClientInstance.GetBlobClient(Path.GetFileName(filePath));
            await blobClient.UploadAsync(filePath, cancellationToken);
            return blobClient.Uri.ToString();
        }
    }
}