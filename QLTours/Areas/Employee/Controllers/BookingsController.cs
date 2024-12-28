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

namespace QLTours.Areas.Employee.Controllers
{
    [Area("Employee")]
    [Authorize(Roles = "Employee")]
    public class BookingsController : Controller
    {
        private readonly QuanLyTourContext _context;

        public BookingsController(QuanLyTourContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 8)
        {
            // Lấy danh sách đặt tour từ database và bao gồm các bảng Tour, User
            var bookings = _context.Bookings
                .Include(b => b.Tour)
                .Include(b => b.User)
                .AsQueryable();

            // Tính tổng số bản ghi và số trang
            var totalBookings = await bookings.CountAsync();
            var totalPages = (int)Math.Ceiling(totalBookings / (double)pageSize);

            // Lấy dữ liệu cho trang hiện tại
            var pagedBookings = await bookings
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Truyền thông tin phân trang
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;

            // Truyền tổng số booking
            ViewBag.TotalBookings = totalBookings;

            return View(pagedBookings);
        }


        // GET: Employee/Bookings/Details/5
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

        // GET: Employee/Bookings/Create
        public IActionResult Create()
        {
            ViewData["TourId"] = new SelectList(_context.Tours, "TourId", "TourName");  
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Username");  
            return View();
        }

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
            ViewData["TourId"] = new SelectList(_context.Tours, "TourId", "TourName", booking.TourId);  
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Username", booking.UserId);  
            return View(booking);
        }

        // GET: Employee/Bookings/Edit/5
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

            // Sử dụng TourName và Username thay vì TourId và UserId
            ViewData["TourId"] = new SelectList(_context.Tours, "TourId", "TourName", booking.TourId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Username", booking.UserId);

            return View(booking);
        }

        // POST: Employee/Bookings/Edit/5
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

        // GET: Employee/Bookings/Delete/5
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

        // POST: Employee/Bookings/Delete/5
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
        // GET: Employee/Bookings/Revenue
        public IActionResult Revenue()
        {
            var bookings = _context.Bookings.ToList();

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

    }
}
