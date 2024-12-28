using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using QLTours.Models;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using QLTours.Services;

namespace QLTours.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserDAO _userDAO;

        public AccountController( UserDAO userDAO)
        {
           
            _userDAO = userDAO;
        }
       
        [HttpGet]
        public IActionResult MyProfile()
        {
            var username = HttpContext.Session.GetString("Username");
            var user = _userDAO.GetUserByUsername(username);

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var userProfile = new UserProfileViewModel
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                DateOfBirth = user.DateOfBirth
            };

            return View(userProfile);
        }

        // Cập nhật thông tin cá nhân
        [HttpPost]
        public IActionResult MyProfile(UserProfileViewModel userProfile)
        {
            if (ModelState.IsValid)
            {
                var sessionUserId = HttpContext.Session.GetInt32("UserId");

                if (sessionUserId == null)
                {
                    ViewBag.Message = "Không tìm thấy người dùng.";
                    return RedirectToAction("Login", "Account");
                }

                var user = _userDAO.GetUserById(sessionUserId.Value);

                if (user == null)
                {
                    ViewBag.Message = "Không tìm thấy người dùng.";
                    return View(userProfile);
                }

                user.Username = userProfile.Username;
                user.Email = userProfile.Email;
                user.Phone = userProfile.Phone;
                user.Address = userProfile.Address;
                user.DateOfBirth = userProfile.DateOfBirth;

                _userDAO.UpdateUser(user);

                if (user.Username != HttpContext.Session.GetString("Username"))
                {
                    HttpContext.Session.SetString("Username", user.Username);
                }

                ViewBag.Message = "Cập nhật thông tin thành công!";
                return View(userProfile); 
            }

            ViewBag.Message = "Có lỗi khi cập nhật thông tin.";
            return View(userProfile);
        }


        // Thay đổi mật khẩu
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var sessionUserId = HttpContext.Session.GetInt32("UserId");

            if (sessionUserId == null)
            {
                ViewBag.PasswordMessage = "Vui lòng đăng nhập để thay đổi mật khẩu.";
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                var existingUser = _userDAO.GetUserById(sessionUserId.Value);

                if (existingUser == null)
                {
                    ViewBag.PasswordMessage = "Không tìm thấy người dùng.";
                    return View();
                }

                // So sánh mật khẩu hiện tại với mật khẩu đã mã hóa
                if (!BCrypt.Net.BCrypt.Verify(currentPassword, existingUser.Password))
                {
                    ViewBag.PasswordMessage = "Mật khẩu hiện tại không đúng.";
                    return View();
                }

                // Kiểm tra mật khẩu mới và xác nhận mật khẩu
                if (newPassword != confirmPassword)
                {
                    ViewBag.PasswordMessage = "Mật khẩu mới và xác nhận mật khẩu không khớp.";
                    return View();
                }

                // Mã hóa mật khẩu mới
                string hashedNewPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);

                existingUser.Password = hashedNewPassword;

                try
                {
                    _userDAO.UpdateUser(existingUser);
                    ViewBag.PasswordMessage = "Đổi mật khẩu thành công!";
                }
                catch (Exception ex)
                {
                    ViewBag.PasswordMessage = $"Lỗi khi đổi mật khẩu: {ex.Message}";
                }

                return View();
            }

            ViewBag.PasswordMessage = "Có lỗi khi đổi mật khẩu.";
            return View();
        }



        // Google Login Action
        [HttpGet]
        public IActionResult LoginWithGoogle(string returnUrl = "/")
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse", "Account", new { returnUrl }) };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }


        // Google Login Callback
        [HttpGet]
        public async Task<IActionResult> GoogleResponse(string returnUrl = null)
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (result?.Principal != null)
            {
                var email = result.Principal.FindFirstValue(ClaimTypes.Email);
                var username = result.Principal.FindFirstValue(ClaimTypes.Name);

                var user = _userDAO.GetUserByEmail(email);
                if (user == null)
                {
                    user = new User
                    {
                        Email = email,
                        Username = username,
                        Password = "GoogleLogin",
                        Phone = "0123456789"
                    };
                    _userDAO.AddUser(user);
                }
                else
                {
                    user.Username = username;
                    user.Phone = user.Phone ?? "0123456789";
                    _userDAO.UpdateUser(user);
                }

                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email)
        };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("IsGoogleLogin", "true");

                return Redirect(returnUrl ?? "/Home/Index");
            }

            return RedirectToAction("Login");
        }

        // Hiển thị form nhập email
        [HttpGet]
        public IActionResult ForgotPassword()
        {

            return View();
        }

        // Xử lý form gửi email
        [HttpPost]
        public IActionResult ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Message = "Vui lòng nhập địa chỉ email hợp lệ.";
                return View();
            }

            var user = _userDAO.GetUserByEmail(email);
            if (user != null)
            {
                // Tạo mã reset mật khẩu và thời gian hết hạn
                var resetCode = Guid.NewGuid().ToString();
                var expirationTime = DateTime.Now.AddMinutes(15);

                // Tùy chọn lưu mã và thời gian hết hạn vào cơ sở dữ liệu nếu cần
                // _userDAO.SaveResetCode(user.Id, resetCode, expirationTime);

                // Gửi email chứa liên kết reset mật khẩu
                SendPasswordResetEmail(user.Email, resetCode);

                // Lưu email và mã reset vào TempData để chuyển sang trang reset password
                TempData["Email"] = user.Email;
                TempData["ResetCode"] = resetCode;

                // Thông báo rằng email reset đã được gửi
                ViewBag.Message = "Chúng tôi đã gửi cho bạn một email với hướng dẫn để thiết lập lại mật khẩu.";
            }
            else
            {
                // Nếu không tìm thấy email trong hệ thống
                ViewBag.Message = "Email không tồn tại trong hệ thống của chúng tôi.";
            }

            return View();
        }


        // Phương thức gửi email reset mật khẩu
        private void SendPasswordResetEmail(string userEmail, string resetCode)
        {
            var resetLink = Url.Action("ResetPassword", "Account", new { resetCode = resetCode, email = userEmail }, Request.Scheme);

            var smtpServer = "smtp.gmail.com";
            var smtpPort = 587;
            //var smtpUsername = "voquocthang107@gmail.com";
            //var smtpPassword = "axmx xdsz bzqi hgst";
            var smtpUsername = "dangthanh112003@gmail.com";
            var smtpPassword = "gmwa mxva rloa qnrh";

            var smtp = new System.Net.Mail.SmtpClient
            {
                Host = smtpServer,
                Port = smtpPort,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(smtpUsername, smtpPassword)
            };

            var mailMessage = new System.Net.Mail.MailMessage
            {
                From = new System.Net.Mail.MailAddress("dangthanh112003@gmail.com"),
                Subject = "Chúc bạn một ngày tốt lành - Yêu cầu thiết lập lại mật khẩu",
                Body = $"Chào bạn,<br/><br/>Để thiết lập lại mật khẩu của bạn, vui lòng nhấp vào liên kết dưới đây: <a href='{resetLink}'>Thiết lập lại mật khẩu</a><br/><br/>Chúc bạn một ngày tốt lành!",
                IsBodyHtml = true
            };

            mailMessage.To.Add(userEmail);

            try
            {
                smtp.Send(mailMessage);
            }
            catch (Exception ex)
            {
                // Ghi lại lỗi
                Console.WriteLine($"Lỗi khi gửi email: {ex.Message}");
                throw new Exception("Có lỗi xảy ra khi gửi email yêu cầu thiết lập lại mật khẩu.", ex);
            }
        }

        // Hiển thị form nhập mật khẩu mới
        [HttpGet]
        public IActionResult ResetPassword(string resetCode, string email)
        {
            if (string.IsNullOrEmpty(resetCode) || string.IsNullOrEmpty(email))
            {
                return RedirectToAction("ForgotPassword", "Account");
            }

            // Lấy thông tin người dùng từ UserDAO bằng email
            var user = _userDAO.GetUserByEmail(email);
            if (user == null)
            {
                // Nếu không tìm thấy người dùng, trả về lỗi
                return RedirectToAction("ForgotPassword", "Account");
            }

            var model = new ResetPasswordViewModel
            {
                ResetCode = resetCode,
                Email = email
            };

            return View(model);
        }

        // Xử lý khi người dùng gửi mật khẩu mới
        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra mã resetCode và email trong UserDAO
                var user = _userDAO.GetUserByEmail(model.Email);
                if (user != null)
                {
                    // Kiểm tra mã resetCode 
                    // Nếu hợp lệ, cập nhật mật khẩu mới cho người dùng
                    _userDAO.UpdatePasswordOnly(user.UserId, model.NewPassword); // Chỉ cập nhật mật khẩu

                    // Chuyển hướng đến trang login sau khi reset mật khẩu thành công
                    return RedirectToAction("Login", "Home");
                }

                // Nếu không tìm thấy người dùng, hoặc mã không hợp lệ
                ModelState.AddModelError("", "Invalid email or reset code.");
            }

            return View(model);
        }

    }




}

