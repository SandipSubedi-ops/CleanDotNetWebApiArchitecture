using System.IO;
using System.Threading.Tasks;
using CleanDapperApp.Models.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting; // Needed for IWebHostEnvironment if we want to save locally

namespace CleanDapperApp.Repository.Services
{
    public class FileService : IFileService
    {
        private readonly ILogger<FileService> _logger;
        private readonly string _uploadPath;

        public FileService(ILogger<FileService> logger)
        {
            _logger = logger;
            // In a real app, inject IWebHostEnvironment or configuration to get the path.
            // For now, let's use a hardcoded folder "Uploads" in the current directory or temp.
            _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
        {
            _logger.LogInformation($"Uploading file: {fileName}");
            var filePath = Path.Combine(_uploadPath, fileName);
            
            // Ensure unique filename if needed, for now just overwrite or simple implementation
            using (var outputStream = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(outputStream);
            }

            _logger.LogInformation($"File uploaded to: {filePath}");
            return filePath;
        }

        public async Task<Stream> GetFileAsync(string fileName)
        {
            var filePath = Path.Combine(_uploadPath, fileName);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found", fileName);
            }
            
            return new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }
    }
}
