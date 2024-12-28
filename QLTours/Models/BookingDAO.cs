using Microsoft.EntityFrameworkCore;

namespace QLTours.Models
{
    public class BookingDAO
    {
        private readonly QuanLyTourContext _context;

        public BookingDAO(QuanLyTourContext context)
        {
            _context = context;
        }
        public List<Booking> GetItemsByUserId(int userId)
        {
            return _context.Bookings
                .Where(c => c.UserId == userId)
                .Include(c => c.Tour)
                .ToList();
        }
    }
}
