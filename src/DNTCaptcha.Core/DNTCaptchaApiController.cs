using System;
using DNTCaptcha.Core.Contracts;
using DNTCaptcha.Core.Providers;
using Microsoft.AspNetCore.Mvc;

namespace DNTCaptcha.Core
{
    /// <summary>
    /// DNTCaptcha Api
    /// </summary>
    [Route("[controller]")]
    public class DNTCaptchaApiController : Controller
    {
        private readonly ICaptchaProtectionProvider _captchaProtectionProvider;
        private readonly ICaptchaStorageProvider _captchaStorageProvider;
        private readonly Func<DisplayMode, ICaptchaTextProvider> _captchaTextProvider;
        private readonly IRandomNumberProvider _randomNumberProvider;
        private readonly ISerializationProvider _serializationProvider;

        /// <summary>
        /// DNTCaptcha TagHelper
        /// </summary>
        public DNTCaptchaApiController(
            ICaptchaProtectionProvider captchaProtectionProvider,
            IRandomNumberProvider randomNumberProvider,
            Func<DisplayMode, ICaptchaTextProvider> captchaTextProvider,
            ICaptchaStorageProvider captchaStorageProvider,
            ISerializationProvider serializationProvider)
        {
            captchaProtectionProvider.CheckArgumentNull(nameof(captchaProtectionProvider));
            randomNumberProvider.CheckArgumentNull(nameof(randomNumberProvider));
            captchaTextProvider.CheckArgumentNull(nameof(captchaTextProvider));
            captchaStorageProvider.CheckArgumentNull(nameof(captchaStorageProvider));
            serializationProvider.CheckArgumentNull(nameof(serializationProvider));

            _captchaProtectionProvider = captchaProtectionProvider;
            _randomNumberProvider = randomNumberProvider;
            _captchaTextProvider = captchaTextProvider;
            _captchaStorageProvider = captchaStorageProvider;
            _serializationProvider = serializationProvider;
        }

        /// <summary>
        /// Creates DNTCaptcha
        /// </summary>
        /// <param name="captchaAttributes">captcha attributes</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true, Duration = 0)]
        public IActionResult CreateDNTCaptcha([FromBody]DNTCaptchaTagHelperHtmlAttributes captchaAttributes)
        {
            if (captchaAttributes == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var number = _randomNumberProvider.Next(captchaAttributes.Min, captchaAttributes.Max);
            var randomText = _captchaTextProvider(captchaAttributes.DisplayMode).GetText(number, captchaAttributes.Language);
            var encryptedText = _captchaProtectionProvider.Encrypt(randomText);
            var captchaImageUrl = getCaptchaImageUrl(captchaAttributes, encryptedText);
            var captchaDivId = $"dntCaptcha{Guid.NewGuid():N}{_randomNumberProvider.Next(captchaAttributes.Min, captchaAttributes.Max)}";
            var cookieToken = $".{captchaDivId}";
            var hiddenInputToken = _captchaProtectionProvider.Encrypt(cookieToken);

            _captchaStorageProvider.Add(HttpContext, cookieToken, randomText);

            return Json(new
            {
                dntCaptchaImgUrl = captchaImageUrl,
                dntCaptchaId = captchaDivId,
                dntCaptchaTextValue = encryptedText,
                dntCaptchaTokenValue = hiddenInputToken
            });
        }

        private string getCaptchaImageUrl(DNTCaptchaTagHelperHtmlAttributes captchaAttributes, string encryptedText)
        {
            var values = new CaptchaImageParams
            {
                Text = encryptedText,
                RndDate = DateTime.Now.Ticks.ToString(),
                ForeColor = captchaAttributes.ForeColor,
                BackColor = captchaAttributes.BackColor,
                FontSize = captchaAttributes.FontSize,
                FontName = captchaAttributes.FontName
            };
            var encryptSerializedValues = _captchaProtectionProvider.Encrypt(_serializationProvider.Serialize(values));
            var actionUrl = captchaAttributes.UseRelativeUrls ?
                Url.Action(action: nameof(DNTCaptchaImageController.Show),
                            controller: nameof(DNTCaptchaImageController).Replace("Controller", string.Empty),
                            values: new { data = encryptSerializedValues, area = "" }) :
                Url.Action(action: nameof(DNTCaptchaImageController.Show),
                            controller: nameof(DNTCaptchaImageController).Replace("Controller", string.Empty),
                            values: new { data = encryptSerializedValues, area = "" },
                            protocol: HttpContext.Request.Scheme);

            if (string.IsNullOrWhiteSpace(actionUrl))
            {
                throw new NullReferenceException("It's not possible to determine the URL of the `DNTCaptchaImageController.Show` method. Please register the `services.AddControllers()` and `endpoints.MapControllerRoute(...)`.");
            }

            return actionUrl;
        }
    }
}