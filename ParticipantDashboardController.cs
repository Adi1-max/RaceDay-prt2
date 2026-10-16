using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace RaceDay222MVC.Controllers
{
    public class ParticipantDashboardController : Controller
    {
        public IActionResult Index()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!string.Equals(
                role,
                "Participant",
                StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }
    }
}