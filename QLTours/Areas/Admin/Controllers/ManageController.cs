using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLTours.Models;

namespace QLTours.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ManageController : Controller
    {
        private readonly QuanLyTourContext _context;

        public ManageController(QuanLyTourContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 5)
        {
            if (_context.Manages == null)
            {
                return Problem("Entity set 'QuanLyTourContext.Manages' is null.");
            }

            var manages = _context.Manages.AsNoTracking(); // Không tracking để tối ưu hiệu suất.
            var totalManages = await manages.CountAsync(); // Tổng số quản lý.
            var totalPages = (int)Math.Ceiling(totalManages / (double)pageSize); // Tính số trang.

            // Lấy dữ liệu cho trang hiện tại.
            var pagedManages = await manages
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Truyền thông tin phân trang qua ViewBag.
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalManages = totalManages;

            return View(pagedManages);
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdMng,Username,Password,Role,Status")] Manage model)
        {
            if (ModelState.IsValid)
            {
                // Mã hóa mật khẩu trước khi lưu vào cơ sở dữ liệu
                model.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);

                // Đặt trạng thái tài khoản mặc định là "Unlocked"
                model.Status = "Unlocked";

                // Thêm người dùng vào cơ sở dữ liệu
                _context.Manages.Add(model);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index)); // Chuyển hướng về trang danh sách người dùng
            }
            return View(model); // Nếu có lỗi trong quá trình xác thực dữ liệu, trả về trang tạo người dùng
        }

        public async Task<IActionResult> Edit(int id)
        {
            var account = await _context.Manages.FindAsync(id);
            return account == null ? NotFound() : View(account);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdMng,Username,Password,Role,Status")] Manage model)
        {
            if (id != model.IdMng || !ModelState.IsValid) return NotFound();

            var account = await _context.Manages.FindAsync(id);
            if (account == null) return NotFound();

            // Nếu người dùng thay đổi mật khẩu, mã hóa mật khẩu mới
            if (!string.IsNullOrEmpty(model.Password))
            {
                account.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);  // Mã hóa mật khẩu mới
            }

            // Cập nhật các thông tin khác
            account.Username = model.Username;
            account.Role = model.Role;
            account.Status = model.Status;

            _context.Update(account);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // Mở khóa tài khoản
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlock(int id)
        {
            var account = await _context.Manages.FindAsync(id);
            if (account == null) return NotFound();

            // Giả sử Status có thể là "Locked" hoặc "Unlocked"
            // Kiểm tra trạng thái hiện tại và thay đổi nó
            account.Status = (account.Status == "Locked") ? "Unlocked" : "Locked";  // Chuyển đổi giữa hai trạng thái

            _context.Update(account);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // Xóa tài khoản
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var account = await _context.Manages.FindAsync(id);
            if (account == null) return NotFound();

            _context.Manages.Remove(account);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}

