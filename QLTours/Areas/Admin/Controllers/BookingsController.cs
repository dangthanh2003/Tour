using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLTours.Areas.Admin.Models;
using QLTours.Models;

namespace QLTours.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BookingsController : Controller
    {
        private readonly QuanLyTourContext _context;

        public BookingsController(QuanLyTourContext context)
        {
            _context = context;
        }
        public IActionResult Revenue()
        {
            var bookings = _context.Bookings.Where(b => b.Status == "Đã thanh toán").ToList();

            var revenueByMonthAndYear = from booking in bookings
                                        group booking by new { booking.BookingDate.Year, booking.BookingDate.Month } into grouped
                                        select new
                                        {
                                            Year = grouped.Key.Year,
                                            Month = grouped.Key.Month,
                                            TotalRevenue = grouped.Sum(b => b.Total)
                                        };

            ViewBag.RevenueByMonthAndYear = revenueByMonthAndYear;

            return View();
        }
        public IActionResult QuarterlyRevenue()
        {
            var bookings = _context.Bookings
                .Where(b => b.Status == "Đã thanh toán")
                .ToList();

            // Kiểm tra nếu bookings không có
            if (bookings == null || !bookings.Any())
            {
                ViewBag.RevenueByQuarter = new List<RevenueData>();
                return View();
            }

            // Nhóm doanh thu theo quý và năm, và hiển thị chi tiết tháng
            var revenueByQuarter = from booking in bookings
                                   group booking by new
                                   {
                                       Year = booking.BookingDate.Year,
                                       Quarter = (booking.BookingDate.Month - 1) / 3 + 1 // Xác định quý
                                   } into grouped
                                   select new RevenueData
                                   {
                                       Quarter = grouped.Key.Quarter switch
                                       {
                                           1 => $"Q1 (Jan-Mar) - {grouped.Key.Year}",  // Tháng 1 đến tháng 3
                                           2 => $"Q2 (Apr-Jun) - {grouped.Key.Year}",  // Tháng 4 đến tháng 6
                                           3 => $"Q3 (Jul-Sep) - {grouped.Key.Year}",  // Tháng 7 đến tháng 9
                                           4 => $"Q4 (Oct-Dec) - {grouped.Key.Year}",  // Tháng 10 đến tháng 12
                                           _ => $"Unknown Quarter - {grouped.Key.Year}"
                                       },
                                       TotalRevenue = grouped.Sum(b => b.Total) // Tính tổng doanh thu
                                   };

            ViewBag.RevenueByQuarter = revenueByQuarter.ToList();

            return View();
        }


        // GET: Admin/Bookings
        public async Task<IActionResult> Index()
        {
            var quanLyTourContext = _context.Bookings.Include(b => b.Tour).Include(b => b.User);
            return View(await quanLyTourContext.ToListAsync());
        }

        // GET: Admin/Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Bookings == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Tour)
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // GET: Admin/Bookings/Create
        public IActionResult Create()
        {
            ViewData["TourId"] = new SelectList(_context.Tours, "TourId", "TourId");
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId");
            return View();
        }

        // POST: Admin/Bookings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingId,UserId,TourId,Total,BookingDate,Status")] Booking booking)
        {
            if (ModelState.IsValid)
            {
                _context.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TourId"] = new SelectList(_context.Tours, "TourId", "TourId", booking.TourId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", booking.UserId);
            return View(booking);
        }

        // GET: Admin/Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Bookings == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }
            ViewData["TourId"] = new SelectList(_context.Tours, "TourId", "TourId", booking.TourId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", booking.UserId);
            return View(booking);
        }

        // POST: Admin/Bookings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingId,UserId,TourId,Total,BookingDate,Status")] Booking booking)
        {
            if (id != booking.BookingId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.BookingId))
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
            ViewData["TourId"] = new SelectList(_context.Tours, "TourId", "TourId", booking.TourId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", booking.UserId);
            return View(booking);
        }

        // GET: Admin/Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Bookings == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Tour)
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // POST: Admin/Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Bookings == null)
            {
                return Problem("Entity set 'QuanLyTourContext.Bookings'  is null.");
            }
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
          return (_context.Bookings?.Any(e => e.BookingId == id)).GetValueOrDefault();
        }
    }
}
