using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace QLTours.Services
{
    public class ImageTourService
    {
        private readonly string _imageDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "tours");

        // Phương thức lưu ảnh
        public async Task<string> SaveImageAsync(IFormFile image)
        {
            var sanitizedFileName = string.Concat(Path.GetFileNameWithoutExtension(image.FileName)
                                                  .Where(c => !Path.GetInvalidFileNameChars().Contains(c)));

            var extension = Path.GetExtension(image.FileName);
            var fileName = sanitizedFileName + extension;

            var savePath = Path.Combine(_imageDirectory, fileName);

            // Kiểm tra trùng tên file và thêm chỉ số nếu cần
            int count = 1;
            while (File.Exists(savePath))
            {
                fileName = sanitizedFileName + $" ({count})" + extension;
                savePath = Path.Combine(_imageDirectory, fileName);
                count++;
            }

            // Tạo và lưu file ảnh vào thư mục
            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            return "/tours/" + fileName;
        }

        // Phương thức xóa ảnh
        public void DeleteImage(string imagePath)
        {
            var filePath = Path.Combine(_imageDirectory, imagePath.TrimStart('/'));
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
