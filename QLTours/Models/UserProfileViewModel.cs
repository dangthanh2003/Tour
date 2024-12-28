namespace QLTours.Models
{
    public class UserProfileViewModel
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public DateOnly? DateOfBirth { get; set; }
        public string? Address { get; set; }
    }
}
