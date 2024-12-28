using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLTours.Models;

namespace QLTours.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ContactController : Controller
    {
        private readonly QuanLyTourContext _context;

        public ContactController(QuanLyTourContext context)
        {
            _context = context;
        }

        // Phương thức Index để hiển thị danh sách liên hệ
        public async Task<IActionResult> Index()
        {
            var contactList = await _context.Contacts.ToListAsync(); // Lấy danh sách liên hệ từ database
            return View(contactList); // Truyền danh sách liên hệ vào View
        }

        // Phương thức để xem chi tiết một liên hệ
        public async Task<IActionResult> Details(int? contactId)
        {
            if (contactId == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .FirstOrDefaultAsync(c => c.ContactId == contactId); // Tìm kiếm liên hệ dựa vào contactId
            if (contact == null)
            {
                return NotFound();
            }

            // Cập nhật trạng thái thành "Đã xem" khi vào trang chi tiết
            contact.Status = "Đã xem";
            _context.Update(contact);
            await _context.SaveChangesAsync();

            return View(contact); // Truyền liên hệ vào View để hiển thị chi tiết
        }
    }
}
