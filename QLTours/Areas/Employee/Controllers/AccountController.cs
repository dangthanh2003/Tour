using Microsoft.AspNetCore.Mvc;

namespace QLTours.Areas.Employee.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }
    }
}
