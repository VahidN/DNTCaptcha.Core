using System;
using System.ComponentModel.DataAnnotations;
using DNTCaptcha.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DNTCaptcha.TestRazorPages.Pages
{
    [BindProperties]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IDNTCaptchaValidatorService _validatorService;
        private readonly DNTCaptchaOptions _captchaOptions;

        [Display(Name = "User name")]
        [Required(ErrorMessage = "User name is empty")]
        public string Username { get; set; }

        [Display(Name = "Password")]
        [Required(ErrorMessage = "Password is empty")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public IndexModel(
            ILogger<IndexModel> logger,
            IDNTCaptchaValidatorService validatorService,
            IOptions<DNTCaptchaOptions> options
            )
        {
            _logger = logger;
            _validatorService = validatorService;
            _captchaOptions = options == null ? throw new ArgumentNullException(nameof(options)) : options.Value;
        }

        public IActionResult OnPost()
        {
            if (!_validatorService.HasRequestValidCaptchaEntry(Language.English, DisplayMode.NumberToWord))
            {
                this.ModelState.AddModelError(_captchaOptions.CaptchaComponent.CaptchaInputName, "Please enter the security code as a number.");
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