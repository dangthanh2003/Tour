using System;
using System.Collections.Generic;
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
    public class TourDetailsController : Controller
    {
        private readonly QuanLyTourContext _context;

        public TourDetailsController(QuanLyTourContext context)
        {
            _context = context;
        }

        // GET: Employee/TourDetails
        public async Task<IActionResult> Index()
        {
            var tourDetailsContext = _context.TourDetails
                .Include(td => td.Tour)
                .Include(td => td.Hotel)
                .Include(td => td.Vehicle);               
            var tourDetails = await tourDetailsContext.ToListAsync();

            return View(tourDetails);
        }

            // GET: Employee/TourDetails/Details/5
            public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.TourDetails == null)
            {
                return NotFound();
            }

            var tourDetail = await _context.TourDetails
                .Include(td => td.Tour)
                .Include(td => td.Hotel)
                .Include(td => td.Vehicle)
                .FirstOrDefaultAsync(m => m.TourDetailId == id);
            if (tourDetail == null)
            {
                return NotFound();
            }

            return View(tourDetail);
        }

        // GET: Employee/TourDetails/Create
        public IActionResult Create()
        {
            ViewData["TourId"] = new SelectList(_context.Tours, "TourId", "TourName");
            ViewData["HotelId"] = new SelectList(_context.Hotels, "HotelId", "HotelName");
            ViewData["VehicleId"] = new SelectList(_context.Vehicles, "VehicleId", "VehicleName");
            return View();
        }

        // POST: Employee/TourDetails/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TourDetailId,TourId,VehicleId,HotelId,")] TourDetail tourDetail)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tourDetail);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TourId"] = new SelectList(_context.Tours, "TourId", "TourName", tourDetail.TourId);
            ViewData["HotelId"] = new SelectList(_context.Hotels, "HotelId", "HotelName", tourDetail.HotelId);
            ViewData["VehicleId"] = new SelectList(_context.Vehicles, "VehicleId", "VehicleName", tourDetail.VehicleId);
            return View(tourDetail);
        }

        // GET: Employee/TourDetails/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.TourDetails == null)
            {
                return NotFound();
            }

            var tourDetail = await _context.TourDetails.FindAsync(id);
            if (tourDetail == null)
            {
                return NotFound();
            }
            ViewData["TourId"] = new SelectList(_context.Tours, "TourId", "TourName", tourDetail.TourId);
            ViewData["HotelId"] = new SelectList(_context.Hotels, "HotelId", "HotelName", tourDetail.HotelId);
            ViewData["VehicleId"] = new SelectList(_context.Vehicles, "VehicleId", "VehicleName", tourDetail.VehicleId);
            return View(tourDetail);
        }


        // POST: Employee/TourDetails/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TourDetailId,TourId,VehicleId,HotelId")] TourDetail tourDetail)
        {
            if (id != tourDetail.TourDetailId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tourDetail);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TourDetailExists(tourDetail.TourDetailId))
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

            // Cập nhật lại ViewData nếu ModelState không hợp lệ
            ViewData["TourId"] = new SelectList(_context.Tours, "TourId", "TourName", tourDetail.TourId);
            ViewData["HotelId"] = new SelectList(_context.Hotels, "HotelId", "HotelName", tourDetail.HotelId);
            ViewData["VehicleId"] = new SelectList(_context.Vehicles, "VehicleId", "VehicleName", tourDetail.VehicleId);
            return View(tourDetail);
        }


        // GET: Employee/TourDetails/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.TourDetails == null)
            {
                return NotFound();
            }

            var tourDetail = await _context.TourDetails
                .Include(td => td.Tour)
                .Include(td => td.Hotel)
                .Include(td => td.Vehicle)
                .FirstOrDefaultAsync(m => m.TourDetailId == id);
            if (tourDetail == null)
            {
                return NotFound();
            }

            return View(tourDetail);
        }

        // POST: Employee/TourDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.TourDetails == null)
            {
                return Problem("Entity set 'QuanLyTourContext.TourDetails' is null.");
            }
            var tourDetail = await _context.TourDetails.FindAsync(id);
            if (tourDetail != null)
            {
                _context.TourDetails.Remove(tourDetail);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TourDetailExists(int id)
        {
            return _context.TourDetails.Any(e => e.TourDetailId == id);
        }
    }
}
