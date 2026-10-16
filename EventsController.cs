using Microsoft.AspNetCore.Mvc;
using RaceDay222MVC.Models;
using RaceDay222MVC.Services;

namespace RaceDay222MVC.Controllers
{
    public class EventsController : Controller
    {
        private readonly EventApiService _eventService;

        public EventsController(EventApiService eventService)
        {
            _eventService = eventService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var events =
                    await _eventService
                        .GetAllAsync<EventViewModel>();

                var upcomingEvents = events
                    .OrderBy(e => e.EventDate)
                    .ToList();

                return View(upcomingEvents);
            }
            catch (HttpRequestException)
            {
                TempData["Error"] =
                    "Unable to load events. Please try again later.";

                return View(new List<EventViewModel>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var token = HttpContext.Session.GetString("JwtToken");

            try
            {
                var eventItem =
                    await _eventService
                        .GetByIdAsync<EventViewModel>(id, token);

                if (eventItem == null)
                {
                    return NotFound();
                }

                return View(eventItem);
            }
            catch (HttpRequestException)
            {
                TempData["Error"] =
                    "Unable to retrieve the selected event.";

                return RedirectToAction(nameof(Index));
            }
        }
    }
}