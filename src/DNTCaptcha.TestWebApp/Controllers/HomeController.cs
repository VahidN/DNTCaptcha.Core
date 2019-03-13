using DNTCaptcha.Core;
using DNTCaptcha.Core.Providers;
using DNTCaptcha.TestWebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace DNTCaptcha.TestWebApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        [ValidateDNTCaptcha(ErrorMessage = "Please enter the security code as a number.",
                            IsNumericErrorMessage = "The input value should be a number.",
                            CaptchaGeneratorLanguage = Language.English)]
        public IActionResult Index([FromForm]AccountViewModel data)
        {
            if (ModelState.IsValid)
            {
                //TODO: Save data
                return RedirectToAction(nameof(Thanks), new { name = data.Username });
            }
            return View();
        }


        [HttpPost, ValidateAntiForgeryToken]
        [ValidateDNTCaptcha(ErrorMessage = "Please enter the security code as a number.",
                            IsNumericErrorMessage = "The input value should be a number.",
                            CaptchaGeneratorLanguage = Language.English)]
        public IActionResult Login2([FromForm]AccountViewModel data)
        {
            if (ModelState.IsValid)
            {
                //TODO: Save data
                return RedirectToAction(nameof(Thanks), new { name = data.Username });
            }
            return View(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        [ValidateDNTCaptcha(ErrorMessage = "Please enter the security code as a number.",
                                    IsNumericErrorMessage = "The input value should be a number.",
                                    CaptchaGeneratorLanguage = Language.English)]
        public IActionResult Login3([FromForm]AccountViewModel data) // For Ajax Forms
        {
            if (ModelState.IsValid)
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