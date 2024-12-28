using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLTours.Models;

namespace QLTours.Areas.Employee.Controllers
{
    [Area("Employee")]
    [Authorize(Roles = "Employee")]
    public class ItineraryImagesController : Controller
    {
        private readonly QuanLyTourContext _context;
        private readonly ImageService _imageService;

        public ItineraryImagesController(QuanLyTourContext context, ImageService imageService)
        {
            _context = context;
            _imageService = imageService;
        }

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 5)
        {
            // Lấy danh sách hình ảnh lịch trình
            var itineraryImages = _context.ItineraryImages;

            // Tính tổng số hình ảnh
            var totalImages = await itineraryImages.CountAsync();

            // Tính số trang
            var totalPages = (int)Math.Ceiling(totalImages / (double)pageSize);

            // Lấy dữ liệu cho trang hiện tại
            var pagedItineraryImages = await itineraryImages
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Truyền thông tin phân trang sang View
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalImages = totalImages; // Tổng số hình ảnh

            return View(pagedItineraryImages);
        }



        // GET: ItineraryImages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var itineraryImage = await _context.ItineraryImages.FindAsync(id);
            if (itineraryImage == null)
            {
                return NotFound();
            }
            return View(itineraryImage);
        }

        // POST: ItineraryImages/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ImageId,ItineraryId,ImagePath")] ItineraryImage itineraryImage, IFormFile newImage)
        {
            if (id != itineraryImage.ImageId)
            {
                return NotFound("Lỗi: ID không khớp.");
            }

            if (newImage == null || newImage.Length == 0)
            {
                ModelState.AddModelError("newImage", "Ảnh không hợp lệ. Vui lòng thử lại.");
                return View(itineraryImage);
            }

            try
            {
                if (newImage != null && newImage.Length > 0)
                {
                    // Lưu ảnh mới và xóa ảnh cũ
                    var imagePath = await _imageService.SaveImageAsync(newImage, "itinerary-images", itineraryImage.ImagePath);
                    itineraryImage.ImagePath = imagePath;
                }

                _context.Update(itineraryImage);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần thiết
                ModelState.AddModelError(string.Empty, "Lỗi xảy ra khi cập nhật ảnh. Vui lòng thử lại.");
                return View(itineraryImage); // Quay lại trang Edit
            }
        }


        // GET: ItineraryImages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.ItineraryImages == null)
            {
                return NotFound();
            }

            var itineraryImage = await _context.ItineraryImages
                .FirstOrDefaultAsync(m => m.ImageId == id);
            if (itineraryImage == null)
            {
                return NotFound();
            }

            return View(itineraryImage);
        }

        // POST: ItineraryImages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var itineraryImage = await _context.ItineraryImages.FindAsync(id);
            if (itineraryImage != null)
            {
                // Delete image from storage
                await _imageService.DeleteImageAsync(itineraryImage.ImagePath);

                _context.ItineraryImages.Remove(itineraryImage);  // Remove from DB
                await _context.SaveChangesAsync();  // Save changes
            }

            return RedirectToAction(nameof(Index));  // Redirect to the list of images
        }

        private bool ItineraryImageExists(int id)
        {
            return _context.ItineraryImages.Any(e => e.ImageId == id);
        }
    }
}
