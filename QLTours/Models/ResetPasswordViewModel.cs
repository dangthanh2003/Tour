using System.ComponentModel.DataAnnotations;

namespace QLTours.Models
{
    public class ResetPasswordViewModel
    {
        public string ResetCode { get; set; } // Mã reset
        public string Email { get; set; } // Email người dùng
        public string NewPassword { get; set; } // Mật khẩu mới
    }
}
