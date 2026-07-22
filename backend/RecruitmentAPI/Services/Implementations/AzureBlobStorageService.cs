using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RecruitmentAPI.Services.Interfaces;

namespace RecruitmentAPI.Services.Implementations
{
    public class AzureBlobStorageService : IBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ILogger<AzureBlobStorageService> _logger;
        private readonly string _containerName;

        public AzureBlobStorageService(IConfiguration configuration, ILogger<AzureBlobStorageService> logger)
        {
            _logger = logger;
            var connectionString = configuration.GetConnectionString("AzureBlobStorage") 
                ?? "UseDevelopmentStorage=true"; // fallback to azurite if not set
            
            _blobServiceClient = new BlobServiceClient(connectionString);
            _containerName = configuration["AzureStorage:ContainerName"] ?? "recruitment-cvs";
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                try {
                    await containerClient.CreateIfNotExistsAsync();
                } catch (Azure.RequestFailedException ex) when (ex.ErrorCode == "PublicAccessNotPermitted" || ex.ErrorCode == "ContainerAlreadyExists") {
                    // Ignore container creation errors if the storage account restricts access policies.
                    // We assume the container either exists or the subsequent UploadAsync will catch the true error.
                }

                var blobClient = containerClient.GetBlobClient(fileName);

                var uploadOptions = new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
                };

                await blobClient.UploadAsync(fileStream, uploadOptions);
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file to Azure Blob Storage: {FileName}", fileName);
                throw;
            }
        }

        public async Task<Stream> DownloadFileAsync(string fileUrl)
        {
            try
            {
                var uri = new Uri(fileUrl);
                var blobName = uri.Segments[^1]; // basic extraction, might need adjustment based on folder structure
                
                // Better extraction for folder structure if full path is in the URL:
                var uriBuilder = new UriBuilder(uri);
                blobName = uriBuilder.Path.Replace($"/{_containerName}/", "");

                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                var blobClient = containerClient.GetBlobClient(blobName);

                var response = await blobClient.DownloadAsync();
                return response.Value.Content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file from Azure Blob Storage: {FileUrl}", fileUrl);
                throw;
            }
        }

        public async Task<bool> DeleteFileAsync(string fileUrl)
        {
            try
            {
                var uri = new Uri(fileUrl);
                var uriBuilder = new UriBuilder(uri);
                var blobName = uriBuilder.Path.Replace($"/{_containerName}/", "");

                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                var blobClient = containerClient.GetBlobClient(blobName);

                var response = await blobClient.DeleteIfExistsAsync();
                return response.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file from Azure Blob Storage: {FileUrl}", fileUrl);
                return false;
            }
        }

        public Task<string> GetFileUrlAsync(string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            return Task.FromResult(blobClient.Uri.ToString());
        }
    }
}
