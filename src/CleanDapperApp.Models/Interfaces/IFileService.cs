using System.IO;
using System.Threading.Tasks;

namespace CleanDapperApp.Models.Interfaces
{
    public interface IFileService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName);
        Task<Stream> GetFileAsync(string fileName);
    }
}
