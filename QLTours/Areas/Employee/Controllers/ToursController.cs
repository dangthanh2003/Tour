using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLTours.Models;
using QLTours.Services;

namespace QLTours.Areas.Employee.Controllers
{
    [Area("Employee")]
    [Authorize(Roles = "Employee")]
    public class ToursController : Controller
    {
        private readonly QuanLyTourContext _context;
        private readonly ImageTourService _imageTourService;

        public ToursController(QuanLyTourContext context, ImageTourService imageTourService)
        {
            _context = context;
            _imageTourService = imageTourService;
        }

        // GET: Employee/Tours
        public async Task<IActionResult> Index(string search, int pageNumber = 1, int pageSize = 5)
        {
            var tours = from t in _context.Tours.Include(t => t.Category)
                        select t;

            // Kiểm tra nếu có từ khóa tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                // Dùng EF.Functions.Like để tìm kiếm trong SQL
                tours = tours.Where(t => EF.Functions.Like(t.TourName, search + "%"));
            }

            // Tính tổng số bản ghi
            var totalTours = await tours.CountAsync();

            // Tính số trang
            var totalPages = (int)Math.Ceiling(totalTours / (double)pageSize);

            // Lấy dữ liệu cho trang hiện tại
            var pagedTours = await tours
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Truyền thông tin phân trang qua ViewBag
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalTours = totalTours;
            ViewBag.Search = search;

            return View(pagedTours);
        }



        //GET: Employee/Tours/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Tours == null)
            {
                return NotFound();
            }

            var tour = await _context.Tours
                .Include(t => t.Category)
                .FirstOrDefaultAsync(m => m.TourId == id);
            if (tour == null)
            {
                return NotFound();
            }

            return View(tour);
        }
        // GET: Employee/Tours/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName");
            return View();
        }

        // POST: Employee/Tours/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TourId,TourName,Description,Price,CategoryId,Quantity,StartDate,EndDate,Img")] Tour tour, IFormFile img)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra và lưu hình ảnh nếu có
                if (img != null && img.Length > 0)
                {
                    tour.Img = await _imageTourService.SaveImageAsync(img); // Lưu ảnh và gán đường dẫn
                }

                _context.Add(tour);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId", tour.CategoryId);
            return View(tour);
        }

        // GET: Employee/Tours/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Tours == null)
            {
                return NotFound();
            }

            var tour = await _context.Tours.FindAsync(id);
            if (tour == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", tour.CategoryId);
            return View(tour);
        }

        // POST: Employee/Tours/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TourId,TourName,Description,Price,CategoryId,Quantity,StartDate,EndDate,Img")] Tour tour, IFormFile img)
        {
            if (id != tour.TourId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra và lưu hình ảnh nếu có
                    if (img != null && img.Length > 0)
                    {
                        // Nếu có ảnh mới, cập nhật lại hình ảnh
                        tour.Img = await _imageTourService.SaveImageAsync(img);
                    }

                    _context.Update(tour);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TourExists(tour.TourId))
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
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId", tour.CategoryId);
            return View(tour);
        }

        // GET: Employee/Tours/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Tours == null)
            {
                return NotFound();
            }

            var tour = await _context.Tours
                .Include(t => t.Category)
                .FirstOrDefaultAsync(m => m.TourId == id);
            if (tour == null)
            {
                return NotFound();
            }

            return View(tour);
        }

        // POST: Employee/Tours/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Tours == null)
            {
                return Problem("Entity set 'QuanLyTourContext.Tours'  is null.");
            }
            var tour = await _context.Tours.FindAsync(id);
            if (tour != null)
            {
                _context.Tours.Remove(tour);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TourExists(int id)
        {
            return (_context.Tours?.Any(e => e.TourId == id)).GetValueOrDefault();
        }
    }
}
