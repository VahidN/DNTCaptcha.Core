using DNTCaptcha.Core;
using DNTCaptcha.Core.Providers;
using DNTCaptcha.TestWebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace DNTCaptcha.TestWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDNTCaptchaValidatorService _validatorService;

        public HomeController(IDNTCaptchaValidatorService validatorService)
        {
            _validatorService = validatorService;
        }

        public IActionResult Index()
        {
            return View();
        }

        // You can use it as an ActionFilter.
        [HttpPost, ValidateAntiForgeryToken]
        [ValidateDNTCaptcha(ErrorMessage = "Please enter the security code as a number.",
                            CaptchaGeneratorLanguage = Language.English,
                            CaptchaGeneratorDisplayMode = DisplayMode.SumOfTwoNumbersToWords)]
        public IActionResult Index([FromForm]AccountViewModel data)
        {
            if (ModelState.IsValid) // If `ValidateDNTCaptcha` fails, it will set a `ModelState.AddModelError`.
            {
                //TODO: Save data
                return RedirectToAction(nameof(Thanks), new { name = data.Username });
            }
            return View();
        }

        // Or you can use the `IDNTCaptchaValidatorService` directly.
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Login2([FromForm]AccountViewModel data)
        {
            if (!_validatorService.HasRequestValidCaptchaEntry(Language.English, DisplayMode.SumOfTwoNumbersToWords))
            {
                this.ModelState.AddModelError(DNTCaptchaTagHelper.CaptchaInputName, "Please enter the security code as a number.");
                return View(nameof(Index));
            }

            //TODO: Save data
            return RedirectToAction(nameof(Thanks), new { name = data.Username });
        }

        [HttpPost, ValidateAntiForgeryToken]
        [ValidateDNTCaptcha(ErrorMessage = "Please enter the security code as a number.",
                            CaptchaGeneratorLanguage = Language.English,
                            CaptchaGeneratorDisplayMode = DisplayMode.SumOfTwoNumbersToWords)]
        public IActionResult Login3([FromForm]AccountViewModel data) // For Ajax Forms
        {
            if (ModelState.IsValid) // If `ValidateDNTCaptcha` fails, it will set a `ModelState.AddModelError`.
            {
                //TODO: Save data
                return Ok();
            }
            return BadRequest(ModelState);
        }

        public IActionResult Thanks(string name)
        {
            return View(nameof(Thanks), name);
        }
    }
}