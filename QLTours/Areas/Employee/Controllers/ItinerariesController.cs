using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLTours.Models;

namespace QLTours.Areas.Employee.Controllers
{
    [Area("Employee")]
    [Authorize(Roles = "Employee")]
    public class ItinerariesController : Controller
    {
        private readonly QuanLyTourContext _context;
        private readonly ImageService _imageService;  // Inject ImageService

        // Constructor với ImageService
        public ItinerariesController(QuanLyTourContext context, ImageService imageService)
        {
            _context = context;
            _imageService = imageService;
        }

        // GET: Employee/Itineraries
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 6)
        {
            // Lấy danh sách lịch trình từ database và bao gồm bảng Tour
            var itineraries = _context.Itineraries.Include(i => i.Tour);

            // Tính tổng số bản ghi
            var totalItineraries = await itineraries.CountAsync();

            // Tính toán số trang
            var totalPages = (int)Math.Ceiling(totalItineraries / (double)pageSize);

            // Lấy dữ liệu cho trang hiện tại
            var pagedItineraries = await itineraries
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Truyền thông tin phân trang và tổng số lịch trình sang View
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItineraries = totalItineraries; // Tổng số lịch trình

            return View(pagedItineraries);
        }



        // GET: Employee/Itineraries/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Itineraries == null)
            {
                return NotFound();
            }

            var itinerary = await _context.Itineraries
                .Include(i => i.Tour)
                .FirstOrDefaultAsync(m => m.ItineraryId == id);
            if (itinerary == null)
            {
                return NotFound();
            }

            return View(itinerary);
        }

        // GET: Employee/Itineraries/Create
        public IActionResult Create()
        {
            ViewData["TourId"] = new SelectList(_context.Tours, "TourId", "TourName");
            return View();
        }

        // POST: Employee/Itineraries/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ItineraryId,TourId,Ngay,ChiTietLichTrinh")] Itinerary itinerary, List<IFormFile> images)
        {
            if (ModelState.IsValid)
            {
                // Save the itinerary data
                _context.Add(itinerary);
                await _context.SaveChangesAsync();

                // Handle image uploads using ImageService
                if (images != null && images.Count > 0)
                {
                    var imagePaths = new List<string>();

                    foreach (var image in images)
                    {
                        if (image != null && image.Length > 0)
                        {
                            var imagePath = await _imageService.SaveImageAsync(image, "images/itinerary");  // Use ImageService
                            imagePaths.Add(imagePath);

                            // Save image info in the ItineraryImages table
                            var itineraryImage = new ItineraryImage
                            {
                                ItineraryId = itinerary.ItineraryId,
                                ImagePath = imagePath
                            };
                            _context.Add(itineraryImage);
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["TourId"] = new SelectList(_context.Tours, "TourId", "TourName", itinerary.TourId);
            return View(itinerary);
        }

        // GET: Employee/Itineraries/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Itineraries == null)
            {
                return NotFound();
            }

            var itinerary = await _context.Itineraries.FindAsync(id);
            if (itinerary == null)
            {
                return NotFound();
            }
            ViewData["TourId"] = new SelectList(_context.Tours, "TourId", "TourName", itinerary.TourId);
            return View(itinerary);
        }

        // POST: Employee/Itineraries/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ItineraryId,TourId,Ngay,ChiTietLichTrinh")] Itinerary itinerary)
        {
            if (id != itinerary.ItineraryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(itinerary);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItineraryExists(itinerary.ItineraryId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["TourId"] = new SelectList(_context.Tours, "TourId", "TourName", itinerary.TourId);
            return View(itinerary);
        }

        // GET: Employee/Itineraries/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Itineraries == null)
            {
                return NotFound();
            }

            var itinerary = await _context.Itineraries
                .Include(i => i.Tour)
                .FirstOrDefaultAsync(m => m.ItineraryId == id);
            if (itinerary == null)
            {
                return NotFound();
            }

            return View(itinerary);
        }

        // POST: Employee/Itineraries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Itineraries == null)
            {
                return Problem("Entity set 'QuanLyTourContext.Itineraries'  is null.");
            }

            var relatedImages = _context.ItineraryImages.Where(img => img.ItineraryId == id).ToList();

            // Xóa các bản ghi liên quan trong bảng con
            if (relatedImages.Any())
            {
                _context.ItineraryImages.RemoveRange(relatedImages);
            }

            var itinerary = await _context.Itineraries.FindAsync(id);

            if (itinerary != null)
            {
                _context.Itineraries.Remove(itinerary);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ItineraryExists(int id)
        {
            return (_context.Itineraries?.Any(e => e.ItineraryId == id)).GetValueOrDefault();
        }


    }
}
