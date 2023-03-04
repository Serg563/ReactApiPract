using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace ReactApiPract.Services
{
    public class BlobService : IBlobService
    {
        BlobServiceClient _blobclient;
        public BlobService(BlobServiceClient blobServiceClient)
        {
            _blobclient = blobServiceClient;
        }
        public async Task<bool> DeleteBlob(string blobName, string container)
        {
            BlobContainerClient blobContainerClient = _blobclient.GetBlobContainerClient(container);
            BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);
            return await blobClient.DeleteIfExistsAsync();
        }
        public async Task<string> GetBlob(string blobName, string container)
        {
            BlobContainerClient blobContainerClient = _blobclient.GetBlobContainerClient(container);
            BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);
            return blobClient.Uri.AbsoluteUri;
        }

        public async Task<string> UploadBlob(string blobName, string container, IFormFile file)
        {
            BlobContainerClient blobContainerClient = _blobclient.GetBlobContainerClient(container);
            BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);
            var httpheader = new BlobHttpHeaders()
            {
                ContentType = file.ContentType
            };
            var result = await blobClient.UploadAsync(file.OpenReadStream(),httpheader);
            if(result != null)
            {
                return await GetBlob(blobName, container);
            }
            return "";
        }
    }
}
