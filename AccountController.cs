using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDay222MVC.Models;
using RaceDay222MVC.Services;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RaceDay222MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthApiService _authApi;

        public AccountController(AuthApiService authApi)
        {
            _authApi = authApi;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (response, error) = await _authApi.LoginAsync(model);

            if (response == null)
            {
                ModelState.AddModelError(string.Empty, error ?? "Invalid email or password.");
                return View(model);
            }

            // 1. Create authentication claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, response.UserId.ToString()),
                new Claim(ClaimTypes.Name, response.Name ?? "User"),
                new Claim(ClaimTypes.Role, response.Role ?? "Participant"),
                new Claim("AuthToken", response.Token)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // 2. Sign user into the browser cookie context
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            // 3. Populate backup session strings
            HttpContext.Session.SetString("AuthToken", response.Token);
            HttpContext.Session.SetString("UserRole", response.Role);
            HttpContext.Session.SetString("UserName", response.Name ?? "User");
            HttpContext.Session.SetInt32("UserId", response.UserId);

            TempData["SuccessMessage"] = $"Welcome, {response.Name}!";

            // 4. Secure role-based redirection
            if (string.Equals(response.Role, "Organiser", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "OrganiserEvents");
            }
            else if (string.Equals(response.Role, "Participant", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Participants");
            }

            return RedirectToAction("Index", "Events");
        }
    }
}