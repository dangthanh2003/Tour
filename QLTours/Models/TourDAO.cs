using Microsoft.EntityFrameworkCore;

namespace QLTours.Models
{
    // Trong thư mục Models hoặc Data
    public class TourDAO
    {
        private QuanLyTourContext _context;

        public TourDAO(QuanLyTourContext context)
        {
            _context = context;
        }
        public Tour GetTourById(int id)
        {
            return _context.Tours.FirstOrDefault(u => u.TourId == id);
        }
        public Tour GetTourDetail(int id)
        {
            return _context.Tours
                .Include(t => t.Category)
                .FirstOrDefault(t => t.TourId == id);
        }
    }

}
