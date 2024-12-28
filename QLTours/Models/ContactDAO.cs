using QLTours.Models;

namespace QLTours.Data
{
    public class ContactDAO
    {
        private readonly QuanLyTourContext _context;

        public ContactDAO(QuanLyTourContext context)
        {
            _context = context;
        }

        public void SaveContactMessage(Contact message)
        {
            _context.Contacts.Add(message); 
            _context.SaveChanges();
        }
    }
}
