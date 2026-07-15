namespace RecruitmentAPI.Services.Interfaces
{
    public interface IBlobStorageService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
        Task<Stream> DownloadFileAsync(string fileUrl);
        Task<bool> DeleteFileAsync(string fileUrl);
        Task<string> GetFileUrlAsync(string fileName);
    }
}