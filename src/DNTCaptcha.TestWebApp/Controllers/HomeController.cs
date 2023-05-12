using System;
using DNTCaptcha.Core;
using DNTCaptcha.TestWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DNTCaptcha.TestWebApp.Controllers;

public class HomeController : Controller
{
    private readonly DNTCaptchaOptions _captchaOptions;
    private readonly IDNTCaptchaValidatorService _validatorService;

    public HomeController(
        IDNTCaptchaValidatorService validatorService,
        IOptions<DNTCaptchaOptions> options
    )
    {
        _validatorService = validatorService;
        _captchaOptions = options == null ? throw new ArgumentNullException(nameof(options)) : options.Value;
    }

    public IActionResult Index() => View();

    // You can use it as an ActionFilter.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [ValidateDNTCaptcha(ErrorMessage = "Please enter the security code as a number.")]
    public IActionResult Index([FromForm] AccountViewModel data)
    {
        if (ModelState.IsValid) // If `ValidateDNTCaptcha` fails, it will set a `ModelState.AddModelError`.
        {
            //TODO: Save data
            return RedirectToAction(nameof(Thanks), new { name = data.Username });
        }

        return View();
    }

    // Or you can use the `IDNTCaptchaValidatorService` directly.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login2([FromForm] AccountViewModel data)
    {
        if (!_validatorService.HasRequestValidCaptchaEntry())
        {
            ModelState.AddModelError(_captchaOptions.CaptchaComponent.CaptchaInputName,
                                     "Please enter the security code as a number.");
            return View(nameof(Index));
        }

        //TODO: Save data
        return RedirectToAction(nameof(Thanks), new { name = data.Username });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ValidateDNTCaptcha(ErrorMessage = "Please enter the security code as a number.")]
    public IActionResult Login3([FromForm] AccountViewModel data) // For Ajax Forms
    {
        if (ModelState.IsValid) // If `ValidateDNTCaptcha` fails, it will set a `ModelState.AddModelError`.
        {
            //TODO: Save data
            return Ok();
        }

        return BadRequest(ModelState);
    }

    public IActionResult Thanks(string name) => View(nameof(Thanks), name);
}