namespace ReactApiPract.Services
{
    public interface IBlobService
    {
        Task<string> GetBlob(string blobName,string container);
        Task<bool> DeleteBlob(string blobName,string container);
        Task<string> UploadBlob(string blobName,string container,IFormFile file);
    }
}
