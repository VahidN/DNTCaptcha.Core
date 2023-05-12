using DNTCaptcha.Core;
using DNTCaptcha.TestWebApp.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace DNTCaptcha.TestWebApp.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("CorsPolicy")]
    public class NgxController : Controller
    {
        private readonly IDNTCaptchaApiProvider _apiProvider;

        public NgxController(IDNTCaptchaApiProvider apiProvider)
        {
            _apiProvider = apiProvider;
        }

        [HttpPost("[action]")]
        [ValidateDNTCaptcha(ErrorMessage = "Please enter the security code as a number.")]
        public IActionResult Login([FromBody] AccountViewModel data)
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
        public ActionResult<DNTCaptchaApiResponse> CreateDNTCaptchaParams()
        {
            // Note: For security reasons, a JavaScript client shouldn't be able to provide these attributes directly.
            // Otherwise an attacker will be able to change them and make them easier!
            return _apiProvider.CreateDNTCaptcha(new DNTCaptchaTagHelperHtmlAttributes
            {
                BackColor = "#f7f3f3",
                FontName = "Tahoma",
                FontSize = 18,
                ForeColor = "#111111",
                Language = Language.English,
                DisplayMode = DisplayMode.SumOfTwoNumbers,
                Max = 90,
                Min = 1
            });
        }
    }
}