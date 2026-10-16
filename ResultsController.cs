using Microsoft.AspNetCore.Mvc;
using RaceDay222MVC.Models;
using RaceDay222MVC.Services;

namespace RaceDay222MVC.Controllers
{
    public class ResultsController : Controller
    {
        private readonly ResultApiService _resultService;

        public ResultsController(ResultApiService resultService)
        {
            _resultService = resultService;
        }

        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("JwtToken");

            try
            {
                var results =
                    await _resultService
                        .GetAllAsync<ResultsViewModel>(token);

                var orderedResults = results
                    .OrderBy(result => result.Position)
                    .ToList();

                return View(orderedResults);
            }
            catch (HttpRequestException)
            {
                TempData["Error"] =
                    "Unable to load results. Please try again later.";

                return View(new List<ResultsViewModel>());
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
                var result =
                    await _resultService
                        .GetByIdAsync<ResultsViewModel>(id, token);

                if (result == null)
                {
                    return NotFound();
                }

                return View(result);
            }
            catch (HttpRequestException)
            {
                TempData["Error"] =
                    "Unable to retrieve the selected result.";

                return RedirectToAction(nameof(Index));
            }
        }
    }
}