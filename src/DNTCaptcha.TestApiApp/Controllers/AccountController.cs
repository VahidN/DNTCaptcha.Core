using DNTCaptcha.Core;
using DNTCaptcha.TestApiApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DNTCaptcha.TestApiApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IDNTCaptchaApiProvider _apiProvider;

    public AccountController(IDNTCaptchaApiProvider apiProvider) => _apiProvider = apiProvider;

    [HttpPost("[action]")]
    [ValidateDNTCaptcha(ErrorMessage = "Please enter the security code as a number.")]
    public IActionResult
        Login([FromForm] AccountViewModel data) //NOTE: It's a [FromForm] data or `application/x-www-form-urlencoded` data.
    {
        if (ModelState.IsValid) // If `ValidateDNTCaptcha` fails, it will set a `ModelState.AddModelError`.
        {
            //TODO: Save data
            return Ok(new { name = data.Username });
        }

        return BadRequest(ModelState);
    }

    [HttpGet("[action]")]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true, Duration = 0)]
    [EnableRateLimiting(DNTCaptchaRateLimiterPolicy.Name)] // don't forget this one!
    public ActionResult<DNTCaptchaApiResponse> CreateDNTCaptchaParams() =>
        // Note: For security reasons, a JavaScript client shouldn't be able to provide these attributes directly.
        // Otherwise an attacker will be able to change them and make them easier!
        _apiProvider.CreateDNTCaptcha(new DNTCaptchaTagHelperHtmlAttributes
                                      {
                                          BackColor = "#f7f3f3",
                                          FontName = "Tahoma",
                                          FontSize = 33,
                                          ForeColor = "#111111",
                                          Language = Language.English,
                                          DisplayMode = DisplayMode.SumOfTwoNumbers,
                                          Max = 90,
                                          Min = 1,
                                      });
}