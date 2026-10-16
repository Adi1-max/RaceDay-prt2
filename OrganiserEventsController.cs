using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RaceDay222MVC.Models;

namespace RaceDay222MVC.Controllers
{
    public class OrganiserEventsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public OrganiserEventsController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("AuthToken");
            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!IsOrganiser())
            {
                return RedirectToAction("Index", "Home");
            }

            var baseUrl = _configuration["RaceDayApi:BaseUrl"];

            try
            {
                using var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync("api/Event");

                if (response.IsSuccessStatusCode)
                {
                    var events = await response.Content.ReadFromJsonAsync<IEnumerable<EventViewModel>>();
                    return View(events);
                }

                TempData["ErrorMessage"] = "Could not retrieve organiser events.";
                return View(new List<EventViewModel>());
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "API Connection connection failed.";
                return View(new List<EventViewModel>());
            }
        }

        private bool IsOrganiser()
        {
            return string.Equals(
                HttpContext.Session.GetString("UserRole"),
                "Organiser",
                StringComparison.OrdinalIgnoreCase);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var token = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrWhiteSpace(token))
                return RedirectToAction("Login", "Account");

            if (!IsOrganiser())
                return RedirectToAction("Index", "Home");

            var baseUrl = _configuration["RaceDayApi:BaseUrl"];

            try
            {
                using var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(baseUrl!);

                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue(
                        "Bearer", token);

                var response = await client.GetAsync($"api/Event/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Unable to retrieve the event.";
                    return RedirectToAction(nameof(Index));
                }

                var eventItem = await response.Content
                    .ReadFromJsonAsync<EventViewModel>();

                if (eventItem == null)
                    return RedirectToAction(nameof(Index));

                var model = new EventCreateViewModel
                {
                    EventName = eventItem.EventName,
                    Description = eventItem.Description,
                    EventDate = eventItem.EventDate,
                    Location = eventItem.Location
                };

                ViewBag.EventID = id;

                return View(model);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] = "Could not connect to the API.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
    int id,
    EventCreateViewModel model)
        {
            var token = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrWhiteSpace(token))
                return RedirectToAction("Login", "Account");

            if (!IsOrganiser())
                return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid)
            {
                ViewBag.EventID = id;
                return View(model);
            }

            var baseUrl = _configuration["RaceDayApi:BaseUrl"];

            try
            {
                using var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(baseUrl!);

                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue(
                        "Bearer", token);

                var response = await client.PutAsJsonAsync(
                    $"api/Event/{id}", model);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] =
                        "Event updated successfully.";

                    return RedirectToAction(nameof(Index));
                }

                ViewBag.EventID = id;
                ViewBag.ErrorMessage =
                    $"Could not update the event. API status: {(int)response.StatusCode}.";

                return View(model);
            }
            catch (HttpRequestException)
            {
                ViewBag.EventID = id;
                ViewBag.ErrorMessage = "Could not connect to the API.";
                return View(model);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var token = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrWhiteSpace(token))
                return RedirectToAction("Login", "Account");

            if (!IsOrganiser())
                return RedirectToAction("Index", "Home");

            var baseUrl = _configuration["RaceDayApi:BaseUrl"];

            try
            {
                using var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(baseUrl!);

                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue(
                        "Bearer", token);

                var response = await client.DeleteAsync(
                    $"api/Event/{id}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] =
                        "Event deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] =
                        $"Could not delete the event. API status: {(int)response.StatusCode}.";
                }
            }

            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Could not connect to the API.";
            }

            return RedirectToAction(nameof(Index));
        }
   
    

    // GET: /OrganiserEvents/Create
        [HttpGet]
        public IActionResult Create()
        {
            // 1. Verify user context access
            var token = HttpContext.Session.GetString("AuthToken");
            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!IsOrganiser())
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new EventCreateViewModel());
        }

        // POST: /OrganiserEvents/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventCreateViewModel model)
        {
            var token = HttpContext.Session.GetString("AuthToken");
            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!IsOrganiser())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var baseUrl = _configuration["RaceDayApi:BaseUrl"];

            try
            {
                using var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // 2. Submit the creation model to the backend API endpoint
                var response = await client.PostAsJsonAsync("api/Event", model);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Event created successfully!";
                    return RedirectToAction(nameof(Index));
                }

                TempData["ErrorMessage"] = $"Could not create event. API Status: {(int)response.StatusCode}";
                return View(model);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Could not connect to the API server.";
                return View(model);
            }
        }

    } }