using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLTours.Models;
using System.Linq;
using System.Threading.Tasks;

namespace QLTours.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PromotionsController : Controller
    {
        private readonly QuanLyTourContext _context;

        public PromotionsController(QuanLyTourContext context)
        {
            _context = context;
        }

        // GET: Promotion
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 5)
        {
            if (_context.Promotions == null)
            {
                return Problem("Entity set 'QuanLyTourContext.Promotion' is null.");
            }

            var promotions = _context.Promotions.AsNoTracking(); // Không tracking để tối ưu hiệu suất.
            var totalPromotions = await promotions.CountAsync(); // Tổng số quản lý.
            var totalPages = (int)Math.Ceiling(totalPromotions / (double)pageSize); // Tính số trang.

            // Lấy dữ liệu cho trang hiện tại.
            var pagedManages = await promotions
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Truyền thông tin phân trang qua ViewBag.
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalPromotions = totalPromotions;

            return View(pagedManages);
        }

        // GET: Promotion/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var promotion = await Task.Run(() =>
                _context.Promotions.FirstOrDefault(p => p.PromotionId == id));

            if (promotion == null) return NotFound();

            return View(promotion);
        }

        // GET: Promotion/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Promotion/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PromotionId,Code,Discount,StartDate,EndDate,IsActive")] Promotion promotion)
        {
            if (ModelState.IsValid)
            {
                _context.Add(promotion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(promotion);
        }

        // GET: Promotion/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null) return NotFound();

            return View(promotion);
        }

        // POST: Promotion/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PromotionId,Code,Discount,StartDate,EndDate,IsActive")] Promotion promotion)
        {
            if (id != promotion.PromotionId)
            {
                // Kiểm tra nếu id không khớp với PromotionId
                return NotFound("Lỗi: ID không khớp.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(promotion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PromotionExists(promotion.PromotionId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(promotion);
        }

        // GET: Promotion/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var promotion = await Task.Run(() =>
                _context.Promotions.FirstOrDefault(p => p.PromotionId == id));
            if (promotion == null) return NotFound();

            return View(promotion);
        }

        // POST: Promotion/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var promotion = await Task.Run(() => _context.Promotions.Find(id));
            if (promotion != null)
            {
                _context.Promotions.Remove(promotion);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool PromotionExists(int id)
        {
            return _context.Promotions.Any(e => e.PromotionId == id);
        }
    }
}
