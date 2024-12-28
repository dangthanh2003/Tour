namespace QLTours.Models
{
    public class UserDAO
    {
        private readonly QuanLyTourContext _context;

        public UserDAO(QuanLyTourContext context)
        {
            _context = context;
        }

        public void AddUser(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public User GetUserByEmail(string email)
        {
            return _context.Users.FirstOrDefault(u => u.Email == email);
        }
        public User GetUserById(int userId)
        {
            return _context.Users.FirstOrDefault(u => u.UserId == userId);
        }
        public User GetUserByUsername(string username)
        {
            return _context.Users.FirstOrDefault(u => u.Username == username);
        }
        public void UpdateUser(User user)
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.UserId == user.UserId);

            if (existingUser == null)
            {
                throw new Exception("User not found.");
            }

            if (_context.Users.Any(u => u.Username == user.Username && u.UserId != user.UserId))
            {
                throw new Exception("Username already exists.");
            }

            existingUser.Username = user.Username;
            existingUser.Email = user.Email;
            existingUser.Phone = user.Phone;
            existingUser.Address = user.Address;
            existingUser.DateOfBirth = user.DateOfBirth;
            existingUser.Password = user.Password;

            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving to database: {ex.Message}");
            }
        }
        public void UpdatePasswordOnly(int userId, string newPassword)
        {
            // Tìm người dùng bằng userId
            var existingUser = _context.Users.FirstOrDefault(u => u.UserId == userId);

            if (existingUser == null)
            {
                throw new Exception("User not found.");
            }

            // Mã hóa mật khẩu mới bằng BCrypt
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);

            // Cập nhật mật khẩu đã mã hóa
            existingUser.Password = hashedPassword;

            try
            {
                // Lưu thay đổi vào cơ sở dữ liệu
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                // Xử lý lỗi khi lưu vào cơ sở dữ liệu
                throw new Exception($"Error saving to database: {ex.Message}");
            }
        }



    }
}
