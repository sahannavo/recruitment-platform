using RecruitmentAPI.Services.Interfaces;

namespace RecruitmentAPI.Services.Implementations
{
    public class LocalBlobStorageService : IBlobStorageService
    {
        private readonly string _uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

        public LocalBlobStorageService()
        {
            if (!Directory.Exists(_uploadFolder))
            {
                Directory.CreateDirectory(_uploadFolder);
            }
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            var uniqueFileName = $"{Guid.NewGuid()}_{fileName.Replace("/", "_").Replace("\\", "_")}";
            var filePath = Path.Combine(_uploadFolder, uniqueFileName);

            using (var fileStreamOutput = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fileStreamOutput);
            }

            return $"/uploads/{uniqueFileName}";
        }

        public Task<Stream> DownloadFileAsync(string fileUrl)
        {
            var fileName = Path.GetFileName(fileUrl);
            var filePath = Path.Combine(_uploadFolder, fileName);
            
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found", filePath);

            return Task.FromResult<Stream>(new FileStream(filePath, FileMode.Open, FileAccess.Read));
        }

        public Task<bool> DeleteFileAsync(string fileUrl)
        {
            var fileName = Path.GetFileName(fileUrl);
            var filePath = Path.Combine(_uploadFolder, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        public Task<string> GetFileUrlAsync(string fileName)
        {
            return Task.FromResult($"/uploads/{fileName}");
        }
    }
}
