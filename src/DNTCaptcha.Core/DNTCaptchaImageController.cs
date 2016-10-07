using System;
using DNTCaptcha.Core.Contracts;
using DNTCaptcha.Core.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace DNTCaptcha.Core
{
    /// <summary>
    /// DNTCaptcha Image Controller
    /// </summary>
    [AllowAnonymous]
    public class DNTCaptchaImageController : Controller
    {
        private readonly ICaptchaProtectionProvider _captchaProtectionProvider;
        private readonly ICaptchaImageProvider _captchaImageProvider;

        /// <summary>
        /// DNTCaptcha Image Controller
        /// </summary>
        public DNTCaptchaImageController(
            ICaptchaImageProvider captchaImageProvider,
            ICaptchaProtectionProvider captchaProtectionProvider)
        {
            captchaImageProvider.CheckArgumentNull(nameof(captchaImageProvider));
            captchaProtectionProvider.CheckArgumentNull(nameof(captchaProtectionProvider));

            _captchaImageProvider = captchaImageProvider;
            _captchaProtectionProvider = captchaProtectionProvider;
        }

        /// <summary>
        /// Creates the captcha image.
        /// </summary>
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true, Duration = 0)]
        public IActionResult Show(string text, string rndDate, string foreColor = "#1B0172",
            string backColor = "", float fontSize = 12, string fontName = "Tahoma")
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return BadRequest();
            }

            if (isImageHotlinking())
            {
                return BadRequest();
            }

            var decryptedText = _captchaProtectionProvider.Decrypt(text);
            if (decryptedText == null)
            {
                return BadRequest();
            }

            return new FileContentResult(
                _captchaImageProvider.DrawCaptcha(decryptedText, foreColor, backColor, fontSize, fontName),
                "image/png");
        }

        private bool isImageHotlinking()
        {
            var applicationUrl = $"{Request.Scheme}://{Request.Host.Value}";
            var urlReferrer = (string)Request.Headers[HeaderNames.Referer];
            return string.IsNullOrEmpty(urlReferrer) ||
                   !urlReferrer.StartsWith(applicationUrl, StringComparison.OrdinalIgnoreCase);
        }
    }
}