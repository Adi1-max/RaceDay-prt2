using Microsoft.AspNetCore.Mvc;
using RaceDay222MVC.Models;
using RaceDay222MVC.Services;

namespace RaceDay222MVC.Controllers
{
    public class ParticipantsController : Controller
    {
        private readonly ParticipantApiService _participantService;

        public ParticipantsController(
            ParticipantApiService participantService)
        {
            _participantService = participantService;
        }

        private string? GetToken()
        {
            return HttpContext.Session.GetString("JwtToken");
        }

        public async Task<IActionResult> Index()
        {
            var token = GetToken();

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var participants =
                    await _participantService
                        .GetAllAsync<ParticipantViewModel>(token);

                return View(participants);
            }
            catch (HttpRequestException)
            {
                TempData["Error"] =
                    "Unable to retrieve participants from the API.";

                return View(new List<ParticipantViewModel>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            var token = GetToken();

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            if (id <= 0)
            {
                return BadRequest();
            }

            try
            {
                var participant =
                    await _participantService
                        .GetByIdAsync<ParticipantViewModel>(id, token);

                if (participant == null)
                {
                    return NotFound();
                }

                return View(participant);
            }
            catch (HttpRequestException)
            {
                TempData["Error"] =
                    "Unable to retrieve the participant.";

                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (string.IsNullOrEmpty(GetToken()))
            {
                return RedirectToAction("Login", "Account");
            }

            return View(new ParticipantCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ParticipantCreateViewModel model)
        {
            var token = GetToken();

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var participant =
                    await _participantService
                        .CreateAsync<
                            ParticipantCreateViewModel,
                            ParticipantViewModel>(model, token);

                if (participant == null)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "The API did not return the created participant.");

                    return View(model);
                }

                TempData["Success"] =
                    "Participant created successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to create the participant. Check the API.");

                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var token = GetToken();

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            if (id <= 0)
            {
                return BadRequest();
            }

            try
            {
                var success =
                    await _participantService.DeleteAsync(id, token);

                TempData[success ? "Success" : "Error"] = success
                    ? "Participant deleted successfully."
                    : "Unable to delete the participant.";

                return RedirectToAction(nameof(Index));
            }
            catch (HttpRequestException)
            {
                TempData["Error"] =
                    "An error occurred while deleting the participant.";

                return RedirectToAction(nameof(Index));
            }
        }
    }
}