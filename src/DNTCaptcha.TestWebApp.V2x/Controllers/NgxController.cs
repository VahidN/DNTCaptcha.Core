using DNTCaptcha.Core;
using DNTCaptcha.Core.Providers;
using DNTCaptcha.TestWebApp.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace DNTCaptcha.TestWebApp.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("CorsPolicy")]
    public class NgxController : Controller
    {
        [HttpPost("[action]")]
        [ValidateDNTCaptcha(ErrorMessage = "Please enter the security code as a number.",
                            CaptchaGeneratorLanguage = Language.English)]
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