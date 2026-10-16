using Microsoft.AspNetCore.Mvc;
using RaceDay222MVC.Models;
using RaceDay222MVC.Services;

namespace RaceDay222MVC.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly PaymentApiService _paymentService;

        public PaymentsController(PaymentApiService paymentService)
        {
            _paymentService = paymentService;
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
                var payments =
                    await _paymentService
                        .GetAllAsync<PaymentViewModel>(token);

                return View(payments);
            }
            catch (HttpRequestException)
            {
                TempData["Error"] =
                    "Unable to retrieve payment records.";

                return View(new List<PaymentViewModel>());
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
                var payment =
                    await _paymentService
                        .GetByIdAsync<PaymentViewModel>(id, token);

                if (payment == null)
                {
                    return NotFound();
                }

                return View(payment);
            }
            catch (HttpRequestException)
            {
                TempData["Error"] =
                    "Unable to retrieve the payment.";

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

            return View(new PaymentCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            PaymentCreateViewModel model)
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
                var payment =
                    await _paymentService
                        .CreateAsync<
                            PaymentCreateViewModel,
                            PaymentViewModel>(model, token);

                if (payment == null)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "The API did not return a payment record.");

                    return View(model);
                }

                TempData["Success"] =
                    "Payment request submitted successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to submit the payment request.");

                return View(model);
            }
        }
    }
}
