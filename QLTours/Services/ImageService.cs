using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public class ImageService
{
    public async Task<string> SaveImageAsync(IFormFile image, string folder, string? existingImagePath = null)
    {
        // Xóa ảnh cũ nếu có
        if (!string.IsNullOrEmpty(existingImagePath))
        {
            var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingImagePath.TrimStart('/'));
            if (File.Exists(oldImagePath))
            {
                File.Delete(oldImagePath);
            }
        }

        // Tạo tên file an toàn
        var sanitizedFileName = string.Concat(Path.GetFileNameWithoutExtension(image.FileName)
                                                .Where(c => !Path.GetInvalidFileNameChars().Contains(c)));
        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
        var fileName = sanitizedFileName + extension;

        // Đường dẫn lưu ảnh
        var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folder, fileName);

        // Xử lý tên file trùng lặp
        int count = 1;
        while (File.Exists(savePath))
        {
            fileName = sanitizedFileName + $" ({count})" + extension;
            savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folder, fileName);
            count++;
        }

        // Tạo thư mục nếu chưa tồn tại
        if (!Directory.Exists(Path.Combine("wwwroot", folder)))
        {
            Directory.CreateDirectory(Path.Combine("wwwroot", folder));
        }

        // Lưu ảnh mới
        using (var fileStream = new FileStream(savePath, FileMode.Create))
        {
            await image.CopyToAsync(fileStream);
        }

        return $"/{folder}/{fileName}";
    }
    public async Task DeleteImageAsync(string imagePath)
    {
        var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imagePath.TrimStart('/'));
        if (File.Exists(oldImagePath))
        {
            File.Delete(oldImagePath);
        }
    }}
