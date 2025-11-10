using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace GP_Server.Application.Services
{
    public interface IFileService
    {
        Task<string> SaveProfilePictureAsync(IFormFile file, string userId);
    }

    public class FileService : IFileService
    {
        private readonly string _profilePicturesDirectory = "wwwroot/images/profile";

        public async Task<string> SaveProfilePictureAsync(IFormFile file, string userId)
        {
            if (!Directory.Exists(_profilePicturesDirectory))
                Directory.CreateDirectory(_profilePicturesDirectory);

            var fileExtension = Path.GetExtension(file.FileName);
            var fileName = $"{userId}{fileExtension}";
            var filePath = Path.Combine(_profilePicturesDirectory, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Path.Combine("images/profile", fileName).Replace("\\", "/");
        }
    }
}
