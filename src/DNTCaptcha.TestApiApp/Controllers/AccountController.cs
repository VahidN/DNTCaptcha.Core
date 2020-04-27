using DNTCaptcha.Core;
using DNTCaptcha.Core.Providers;
using DNTCaptcha.TestApiApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace DNTCaptcha.TestApiApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        [HttpPost("[action]")]
        [ValidateDNTCaptcha(ErrorMessage = "Please enter the security code as a number.",
                    CaptchaGeneratorLanguage = Language.English,
                    CaptchaGeneratorDisplayMode = DisplayMode.SumOfTwoNumbers)]
        public IActionResult Login([FromBody]AccountViewModel data)
        {
            if (ModelState.IsValid) // If `ValidateDNTCaptcha` fails, it will set a `ModelState.AddModelError`.
            {
                //TODO: Save data
                return Ok(new { name = data.Username });
            }
            return BadRequest(ModelState);
        }
    }
}