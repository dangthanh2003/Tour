using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLTours.Models;

namespace QLTours.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly QuanLyTourContext _context;

        public UsersController(QuanLyTourContext context)
        {
            _context = context;
        }

        // GET: Admin/Users
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 5)
        {
            if (_context.Users == null)
            {
                return Problem("Entity set 'QuanLyTourContext.Users' is null.");
            }

            var users = _context.Users.AsNoTracking(); // Không tracking để tối ưu hiệu suất.
            var totalUsers = await users.CountAsync(); // Tổng số người dùng.
            var totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize); // Tính số trang.

            // Lấy dữ liệu cho trang hiện tại.
            var pagedUsers = await users
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Truyền thông tin phân trang qua ViewBag.
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalUsers = totalUsers;

            return View(pagedUsers);
        }

        // GET: Admin/Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }
    }
}
