using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.RegularExpressions;
using QLTours.Services;
using QLTours.Models;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using System.Net.Mail;
using System.Net;
using SmtpClient = System.Net.Mail.SmtpClient;
using QLTours.Data;
using BCrypt.Net;

namespace QLTours.Controllers
{
    public class HomeController : Controller
    {
        private readonly IEmailSender _emailSender;

        private readonly QuanLyTourContext _ctx;
        private readonly TourDAO _tourDAO;
        private readonly UserDAO _userDAO;
        private readonly BookingDAO _bookingDAO;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IVnPayService _vnPayService;
        private readonly ContactDAO _contactDAO;

        // Constructor cho HomeController
        public HomeController(QuanLyTourContext ctx,
                              TourDAO tourDAO,
                              UserDAO userDAO,
                              IHttpContextAccessor httpContextAccessor,
                              BookingDAO bookingDAO,
                              IVnPayService vnPayService,
                              IEmailSender emailSender,
                              ContactDAO contactDAO)
        {
            _ctx = ctx;
            _tourDAO = tourDAO;
            _userDAO = userDAO;
            _bookingDAO = bookingDAO;
            _httpContextAccessor = httpContextAccessor;
            _vnPayService = vnPayService;
            _emailSender = emailSender;
            _contactDAO = contactDAO;
        }


        public IActionResult CreatePaymentUrl(Booking newBooking)
        {
            var url = _vnPayService.CreatePaymentUrl(newBooking, HttpContext);

            return Redirect(url);
        }


		// Phương thức gửi email thông báo thanh toán thành công
		private void SendPaymentSuccessEmail(string userEmail, Booking booking)
		{
			var smtpServer = "smtp.gmail.com"; // Thay thế bằng thông tin SMTP server của bạn
			var smtpPort = 587; // Thay thế bằng cổng SMTP của bạn (thông thường là 587 hoặc 465)
			//var smtpUsername = "voquocthang107@gmail.com"; // Thay thế bằng tên đăng nhập SMTP của bạn
			//var smtpPassword = "axmx xdsz bzqi hgst"; // Thay thế bằng mật khẩu SMTP của bạn

            var smtpUsername = "dangthanh112003@gmail.com"; 
            var smtpPassword = "gmwa mxva rloa qnrh"; 

            var smtp = new SmtpClient
			{
				Host = smtpServer,
				Port = smtpPort,
				EnableSsl = true, // Bật kết nối SSL
				DeliveryMethod = SmtpDeliveryMethod.Network,
				UseDefaultCredentials = false,
				Credentials = new NetworkCredential(smtpUsername, smtpPassword)
			};

			var mailMessage = new MailMessage
			{
				//From = new MailAddress("voquocthang107@gmail.com"),
                From = new MailAddress("dangthanh112003@gmail.com"),
                Subject = "Thanh toán thành công", // Tiêu đề email
				Body = $"Chúc mừng! Thanh toán của bạn cho đơn đặt chỗ ID: {booking.BookingId} đã thành công. Cảm ơn bạn đã mua sắm tại chúng tôi!<br>" +
					   $"Tổng số tiền: {booking.Total} VND<br>" +
					   $"Ngày đặt: {booking.BookingDate}<br>" +
					   $"Trạng thái: {booking.Status}",
				IsBodyHtml = true
			};

			mailMessage.To.Add(userEmail); // Thay thế bằng địa chỉ email của người nhận

			try
			{
				smtp.Send(mailMessage);
			}
			catch (Exception ex)
			{
				// Xử lý lỗi khi gửi email
				throw ex;
			}
		}



		public IActionResult PaymentCallback()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            // Lấy booking ID từ Session
            int? bookingId = HttpContext.Session.GetInt32("BookingId");

            if (bookingId.HasValue)
            {
                // Kiểm tra xem thanh toán có thành công hay không
                if (response.VnPayResponseCode == "00")
                {
                    // Update booking status to "Đã thanh toán" in the database
                    var booking = _ctx.Bookings
                        .Include(b => b.User) // Load thông tin user liên quan đến booking
                        .FirstOrDefault(b => b.BookingId == bookingId);
                    if (booking != null)
                    {
                        booking.Status = "Đã thanh toán";
                        _ctx.SaveChanges();
                        SendPaymentSuccessEmail(booking.User.Email, booking);
                        return View(response);
                    }
                }
                else
                {
                    // Thanh toán thất bại, update booking status to "Giao dịch bị hủy" in the database
                    var booking = _ctx.Bookings
                        .Include(b => b.User) // Load thông tin user liên quan đến booking
                        .FirstOrDefault(b => b.BookingId == bookingId);
                    if (booking != null)
                    {
                        booking.Status = "Giao dịch bị hủy";
                        _ctx.SaveChanges();

                        return RedirectToAction("PaymentFailed");
                    }
                }
            }

            return RedirectToAction("PaymentFailed");
        }

        public IActionResult PaymentFailed()
        {


            return View();
        }



        public IActionResult Index()
        {
            // Truy vấn danh sách category từ cơ sở dữ liệu
            var categories = _ctx.Categories.ToList();

            // Truy vấn danh sách tour từ cơ sở dữ liệu
            var tours = _ctx.Tours
                .Include(t => t.Category)
                .ToList();

            // Truyền danh sách category và danh sách tour đến view
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");
            return View(tours);
        }
        /*public IActionResult List()
        {
            // Lấy danh mục
            var categories = _ctx.Categories.ToList();

            // Lấy danh sách TourId
            var tourNames = _ctx.Tours.Select(t => t.TourId).Distinct().ToList();
            ViewBag.TourNames = new SelectList(tourNames);

            // Lấy tất cả Tour với các liên kết cần thiết
            var tours = _ctx.Tours
                .Include(t => t.Category)
                .Include(t => t.Reviews)
                .Include(t => t.TourDetails)
                    .ThenInclude(td => td.Vehicle) 
                .Include(t => t.TourDetails)
                    .ThenInclude(td => td.Hotel)
                .ToList();

            // Truyền dữ liệu vào ViewBag
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");

            return View(tours); // Truyền danh sách tour đến view
        }*/
        public IActionResult List(int page = 1, int pageSize = 6)
        {
            // Lấy danh mục
            var categories = _ctx.Categories.ToList();

            // Lấy danh sách TourId
            var tourNames = _ctx.Tours.Select(t => t.TourId).Distinct().ToList();
            ViewBag.TourNames = new SelectList(tourNames);

            // Lấy tất cả Tour với các liên kết cần thiết và thực hiện phân trang
            var tours = _ctx.Tours
                .Include(t => t.Category)
                .Include(t => t.Reviews)
                .Include(t => t.TourDetails)
                    .ThenInclude(td => td.Vehicle)
                .Include(t => t.TourDetails)
                    .ThenInclude(td => td.Hotel)

                .OrderBy(t => t.TourId) // Đảm bảo có thứ tự khi phân trang
                .Skip((page - 1) * pageSize) // Bỏ qua các phần tử trước trang hiện tại
                .Take(pageSize) // Lấy số lượng phần tử theo kích thước trang

                .ToList();

            // Lấy tổng số lượng tour để tính số trang
            var totalTours = _ctx.Tours.Count();

            // Tính số trang
            var totalPages = (int)Math.Ceiling(totalTours / (double)pageSize);

            // Truyền vào ViewBag để hiển thị phân trang
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;

            // Truyền dữ liệu vào ViewBag
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");

            return View(tours); // Truyền danh sách tour đã phân trang đến view
        }

        // Thêm action để hiển thị chi tiết tour

        public IActionResult Search(int? categoryId, DateTime? startDate, DateTime? endDate, decimal? price, string tourName)
        {
            var tours = _ctx.Tours
                .Include(t => t.Category)
                .Include(t => t.Reviews) // Load thông tin đánh giá
                .Include(t => t.TourDetails)
                    .ThenInclude(td => td.Vehicle)
                .Include(t => t.TourDetails)
                    .ThenInclude(td => td.Hotel)
                .ToList();
            var categories = _ctx.Categories.ToList();
            var toursQuery = _ctx.Tours.Include(t => t.Category).AsQueryable();
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");

            if (categoryId.HasValue)
            {
                toursQuery = toursQuery.Where(t => t.CategoryId == categoryId);
            }

            if (startDate.HasValue)
            {
                toursQuery = toursQuery.Where(t => t.StartDate == startDate);
            }

            if (endDate.HasValue)
            {
                toursQuery = toursQuery.Where(t => t.EndDate == endDate);
            }

            if (price.HasValue)
            {
                toursQuery = toursQuery.Where(t => t.Price <= price);
            }

            if (!string.IsNullOrEmpty(tourName))
            {
                toursQuery = toursQuery.Where(t => t.TourName.ToLower().Contains(tourName.ToLower()));
            }

            var searchedTours = toursQuery.ToList();

            return View(searchedTours);
        }

         // Phương thức GET cho trang liên hệ
    [HttpGet]
    public IActionResult SendMessage()
    {
        return View();
    }

        // Phương thức POST cho form liên hệ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SendMessage(string username, string email, string subject, string messageContent)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var message = new Contact
                    {
                        Username = username,
                        Email = email,
                        Subject = subject,
                        Message = messageContent,
                        Status = "Chưa xem",
                    };

                    _contactDAO.SaveContactMessage(message);

                    // Lưu thông báo thành công vào ViewBag
                    ViewBag.Message = "Your message has been sent successfully!";
                }
                catch (Exception ex)
                {
                    // Lưu thông báo lỗi vào ViewBag
                    ViewBag.ErrorMessage = $"Error sending message: {ex.Message}";
                }
            }
            else
            {
                // Thông báo khi model không hợp lệ
                ViewBag.ErrorMessage = "Failed to send message. Please try again.";
            }

            
            return View("SendMessage");
        }


        public IActionResult TourDetail(int id)
        {
            var tour = _ctx.Tours
                .Include(t => t.Category)
                .Include(t => t.TourDetails)
                    .ThenInclude(td => td.Hotel)
                .Include(t => t.TourDetails)
                    .ThenInclude(td => td.Vehicle)
                .Include(t => t.Itineraries)
                    .ThenInclude(i => i.DetailItineraries)
                .Include(t => t.Itineraries)
                    .ThenInclude(i => i.ItineraryImages)  
                .Include(t => t.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefault(t => t.TourId == id);

            if (tour == null)
            {
                return NotFound();
            }

            // Tính trung bình sao
            double? averageRating = 0.0;
            if (tour.Reviews != null && tour.Reviews.Any())
            {
                averageRating = tour.Reviews.Average(r => r.Rating);
            }

            // Truyền trung bình sao vào ViewBag để sử dụng trong view
            ViewBag.AverageRating = averageRating;
            return View(tour);
        }




        private bool IsValidEmail(string email)
        {
            // Sử dụng regular expression để kiểm tra định dạng email
            string pattern = @"^[A-Za-z0-9._%+-]+@gmail\.com$";
            return Regex.IsMatch(email, pattern);
        }
        // Mã hóa mật khẩu khi người dùng đăng ký
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        // Kiểm tra mật khẩu khi người dùng đăng nhập
        public bool VerifyPassword(string enteredPassword, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(enteredPassword, storedHash);
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User user, string ConfirmPassword)
        {
            if (user.Password != ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Mật khẩu và xác nhận mật khẩu không trùng nhau");
                return View(user);
            }

            if (_userDAO.GetUserByEmail(user.Email) != null)
            {
                ModelState.AddModelError("Email", "Email đã tồn tại");
                return View(user);
            }

            if (string.IsNullOrWhiteSpace(user.Username))
            {
                ModelState.AddModelError("Username", "Tên đăng nhập là bắt buộc");
                return View(user);
            }

            if (string.IsNullOrWhiteSpace(user.Phone))
            {
                ModelState.AddModelError("Phone", "Số điện thoại là bắt buộc");
                return View(user);
            }

            if (!IsValidEmail(user.Email))
            {
                ModelState.AddModelError("Email", "Định dạng email không hợp lệ");
                return View(user);
            }

            // Mã hóa mật khẩu trước khi lưu
            user.Password = HashPassword(user.Password);

            // Thêm người dùng mới vào cơ sở dữ liệu
            _userDAO.AddUser(user);

            return RedirectToAction("Login");
        }



        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        //Dang nhap
        [HttpPost]
        public IActionResult Login(User user)
        {
            User existingUser = _userDAO.GetUserByEmail(user.Email);

            if (existingUser == null || !VerifyPassword(user.Password, existingUser.Password))
            {
                ModelState.AddModelError("LoginError", "Email hoặc mật khẩu không hợp lệ");
                return View();
            }

            // Đăng nhập và chuyển hướng
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, existingUser.UserId.ToString()),
        new Claim(ClaimTypes.Name, existingUser.Username)
    };

            HttpContext.SignInAsync(new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme))).Wait();
            HttpContext.Session.SetInt32("UserId", existingUser.UserId);
            HttpContext.Session.SetString("Username", existingUser.Username);

            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            // Xóa session khi người dùng đăng xuất
            HttpContext.Session.Remove("Username");
            HttpContext.Session.Remove("IsGoogleLogin");

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }

        private int GetUserId()
        {
            if (_httpContextAccessor.HttpContext.User.Identity is ClaimsIdentity identity && identity.IsAuthenticated)
            {

                var userIdClaim = identity.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    return userId;
                }
            }

            return 0;
        }

        [HttpGet]
        public IActionResult BookTour(int id)
        {
            // Kiểm tra người dùng đã đăng nhập chưa
            if (!_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            // Lấy thông tin tour từ database
            Tour tour = _tourDAO.GetTourDetail(id);

            if (tour == null)
            {
                return NotFound();
            }

            // Chuyển đổi dữ liệu sang BookTourViewModel
            var viewModel = new BookTourViewModel
            {
                TourId = tour.TourId,
                TourName = tour.TourName,
                Price = tour.Price,
                Img = tour.Img,
                Quantity = 1,  // Mặc định số lượng vé là 1
                DiscountCode = string.Empty  // Mặc định không có mã giảm giá
            };

            return View(viewModel);  // Trả về ViewModel cho View
        }


        // Phương thức hiển thị danh sách mã khuyến mãi
        [HttpGet]
        public IActionResult Promotions()
        {
            // Lấy danh sách các khuyến mãi đang hoạt động
            var activePromotions = _ctx.Promotions
                .Where(p => p.IsActive && p.StartDate <= DateTime.Now && p.EndDate >= DateTime.Now)
                .ToList();

            // Truyền danh sách khuyến mãi đến view
            return View(activePromotions);
        }
        [HttpPost]
        public IActionResult BookTour2(int tourId, int quantity, string discountCode)
        {
            int userId = GetUserId();
            Tour tour = _tourDAO.GetTourDetail(tourId);

            if (tour == null)
            {
                return NotFound();
            }

            decimal totalCost = tour.Price * quantity;

            // Kiểm tra mã giảm giá (chỉ kiểm tra khi có giá trị)
            if (!string.IsNullOrEmpty(discountCode))
            {
                var promotion = _ctx.Promotions.FirstOrDefault(p => p.Code == discountCode && p.IsActive);

                if (promotion != null && promotion.StartDate <= DateTime.Now && promotion.EndDate >= DateTime.Now)
                {
                    totalCost *= (1 - promotion.Discount / 100); // Áp dụng giảm giá
                }
                else
                {
                    ModelState.AddModelError("discountCode", "Mã giảm giá không hợp lệ hoặc đã hết hạn!");
                }
            }

            // Xóa lỗi nếu discountCode trống
            if (string.IsNullOrEmpty(discountCode))
            {
                ModelState.Remove("DiscountCode");
            }

            // Nếu ModelState không hợp lệ (lỗi mã giảm giá), trả về View với viewModel
            if (!ModelState.IsValid)
            {
                // Truyền lại model phù hợp để view sử dụng
                var viewModel = new BookTourViewModel
                {
                    TourId = tour.TourId,
                    TourName = tour.TourName,
                    Price = tour.Price,
                    Img = tour.Img,
                    Quantity = quantity,
                    DiscountCode = discountCode
                };

                return View("BookTour", viewModel); // Trả lại view với viewModel
            }

            // Tạo đối tượng đặt tour
            Booking newBooking = new Booking
            {
                TourId = tourId,
                UserId = userId,
                Total = totalCost,
                BookingDate = DateTime.Now,
                Status = "Đang xác nhận"
            };

            _ctx.Bookings.Add(newBooking);
            _ctx.SaveChanges();
            HttpContext.Session.SetInt32("BookingId", newBooking.BookingId);

            // Tạo URL thanh toán
            var paymentUrl = _vnPayService.CreatePaymentUrl(newBooking, HttpContext);
            return Redirect(paymentUrl);
        }









        // Action hiển thị thông tin booking của người dùng
        public IActionResult BookingDetails()
        {
            // Kiểm tra xem người dùng đã đăng nhập hay chưa
            if (!_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                // Người dùng chưa đăng nhập, chuyển hướng đến trang đăng nhập
                return RedirectToAction("Login");
            }
            int userId = GetUserId();
            List<Booking> cartItems = _bookingDAO.GetItemsByUserId(userId);

            // Passing data
            return View(cartItems);
        }

        // Action hủy booking
        [HttpPost]
        public IActionResult CancelBooking(int bookingId)
        {


            // Lấy thông tin người dùng hiện tại
            int userId = GetUserId();

            // Kiểm tra xem booking có thuộc về người dùng hiện tại không
            Booking booking = _ctx.Bookings.FirstOrDefault(b => b.BookingId == bookingId && b.UserId == userId);

            if (booking == null)
            {
                // Xử lý trường hợp booking không tồn tại hoặc không thuộc về người dùng hiện tại
                return NotFound();
            }

            // Thực hiện logic hủy booking ở đây (ví dụ: đặt trạng thái booking thành "Cancelled")
            booking.Status = "Đã hủy";
            _ctx.SaveChanges();

            // Chuyển hướng đến trang thông báo hoặc trang lịch sử đặt tour của người dùng
            return RedirectToAction("BookingDetails", "Home");
        }

        public IActionResult AddReview(int id)
        {
            // Lấy thông tin tour dựa trên id được truyền vào
            Tour tour = _tourDAO.GetTourDetail(id);

            if (tour == null)
            {
                return NotFound();
            }

            // Hiển thị trang đặt tour với thông tin của tour
            return View(tour);
        }

        [HttpPost]
        public IActionResult AddReview(Review review)
        {
            // Get the user ID of the currently logged-in user
            int userId = GetUserId();

            // Check if the user has booked the tour
            bool hasBooked = _ctx.Bookings.Any(b => b.TourId == review.TourId && b.UserId == userId);

            if (!hasBooked)
            {
                // Set TempData with the message
                TempData["BookingMessage"] = "Bạn chưa đặt tour này!";
                return RedirectToAction("TourDetail", new { id = review.TourId });
            }

            // Set the user ID for the review
            review.UserId = userId;

            // Add the review to the database
            _ctx.Reviews.Add(review);
            _ctx.SaveChanges();

            // Redirect to the tour details page after adding the review
            return RedirectToAction("TourDetail", new { id = review.TourId });
        }





        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}