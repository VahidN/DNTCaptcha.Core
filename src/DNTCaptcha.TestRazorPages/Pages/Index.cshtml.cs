using System.ComponentModel.DataAnnotations;
using DNTCaptcha.Core;
using DNTCaptcha.Core.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace DNTCaptcha.TestRazorPages.Pages
{
    [BindProperties]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IDNTCaptchaValidatorService _validatorService;

        [Display(Name = "User name")]
        [Required(ErrorMessage = "User name is empty")]
        public string Username { get; set; }

        [Display(Name = "Password")]
        [Required(ErrorMessage = "Password is empty")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public IndexModel(ILogger<IndexModel> logger, IDNTCaptchaValidatorService validatorService)
        {
            _logger = logger;
            _validatorService = validatorService;
        }

        public IActionResult OnPost()
        {
            if (!_validatorService.HasRequestValidCaptchaEntry(Language.English, DisplayMode.SumOfTwoNumbersToWords))
            {
                this.ModelState.AddModelError(DNTCaptchaTagHelper.CaptchaInputName, "Please enter the security code as a number.");
                return Page();
            }

            //TODO: save data

            return RedirectToPage("privacy");
        }

        public void OnGet()
        {

        }
    }
}