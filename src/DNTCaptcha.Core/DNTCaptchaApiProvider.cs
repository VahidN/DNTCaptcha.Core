using System;
using DNTCaptcha.Core.Contracts;
using DNTCaptcha.Core.Providers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DNTCaptcha.Core
{
    /// <summary>
    /// ApiProvider Response
    /// </summary>
    public class DNTCaptchaApiResponse
    {
        /// <summary>
        /// The captach's image url
        /// </summary>
        /// <value></value>
        public string DntCaptchaImgUrl { set; get; }

        /// <summary>
        /// Captcha Id
        /// </summary>
        public string DntCaptchaId { set; get; }

        /// <summary>
        /// Captcha's TextValue
        /// </summary>
        public string DntCaptchaTextValue { set; get; }

        /// <summary>
        /// Captcha's TokenValue
        /// </summary>
        public string DntCaptchaTokenValue { set; get; }
    }

    /// <summary>
    /// DNTCaptcha Api
    /// </summary>
    public interface IDNTCaptchaApiProvider
    {
        /// <summary>
        /// Creates DNTCaptcha
        /// </summary>
        /// <param name="captchaAttributes">captcha attributes</param>
        DNTCaptchaApiResponse CreateDNTCaptcha(DNTCaptchaTagHelperHtmlAttributes captchaAttributes);
    }

    /// <summary>
    /// DNTCaptcha Api
    /// </summary>
    public class DNTCaptchaApiProvider : IDNTCaptchaApiProvider
    {
        private readonly ICaptchaProtectionProvider _captchaProtectionProvider;
        private readonly ICaptchaStorageProvider _captchaStorageProvider;
        private readonly Func<DisplayMode, ICaptchaTextProvider> _captchaTextProvider;
        private readonly IRandomNumberProvider _randomNumberProvider;
        private readonly ISerializationProvider _serializationProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUrlHelper _urlHelper;

        /// <summary>
        /// DNTCaptcha Api
        /// </summary>
        public DNTCaptchaApiProvider(
            ICaptchaProtectionProvider captchaProtectionProvider,
            IRandomNumberProvider randomNumberProvider,
            Func<DisplayMode, ICaptchaTextProvider> captchaTextProvider,
            ICaptchaStorageProvider captchaStorageProvider,
            ISerializationProvider serializationProvider,
            IHttpContextAccessor httpContextAccessor,
            IUrlHelper urlHelper)
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
            _httpContextAccessor = httpContextAccessor;
            _urlHelper = urlHelper;
        }

        /// <summary>
        /// Creates DNTCaptcha
        /// </summary>
        /// <param name="captchaAttributes">captcha attributes</param>
        public DNTCaptchaApiResponse CreateDNTCaptcha(DNTCaptchaTagHelperHtmlAttributes captchaAttributes)
        {
            var number = _randomNumberProvider.Next(captchaAttributes.Min, captchaAttributes.Max);
            var randomText = _captchaTextProvider(captchaAttributes.DisplayMode).GetText(number, captchaAttributes.Language);
            var encryptedText = _captchaProtectionProvider.Encrypt(randomText);
            var captchaImageUrl = getCaptchaImageUrl(captchaAttributes, encryptedText);
            var captchaDivId = $"dntCaptcha{Guid.NewGuid():N}{_randomNumberProvider.Next(captchaAttributes.Min, captchaAttributes.Max)}";
            var cookieToken = $".{captchaDivId}";
            var hiddenInputToken = _captchaProtectionProvider.Encrypt(cookieToken);

            _captchaStorageProvider.Add(_httpContextAccessor.HttpContext, cookieToken, randomText);

            return new DNTCaptchaApiResponse
            {
                DntCaptchaImgUrl = captchaImageUrl,
                DntCaptchaId = captchaDivId,
                DntCaptchaTextValue = encryptedText,
                DntCaptchaTokenValue = hiddenInputToken
            };
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
                _urlHelper.Action(action: nameof(DNTCaptchaImageController.Show),
                            controller: nameof(DNTCaptchaImageController).Replace("Controller", string.Empty),
                            values: new { data = encryptSerializedValues, area = "" }) :
                _urlHelper.Action(action: nameof(DNTCaptchaImageController.Show),
                            controller: nameof(DNTCaptchaImageController).Replace("Controller", string.Empty),
                            values: new { data = encryptSerializedValues, area = "" },
                            protocol: _httpContextAccessor.HttpContext.Request.Scheme);

            if (string.IsNullOrWhiteSpace(actionUrl))
            {
                throw new NullReferenceException("It's not possible to determine the URL of the `DNTCaptchaImageController.Show` method. Please register the `services.AddControllers()` and `endpoints.MapControllerRoute(...)`.");
            }

            return actionUrl;
        }
    }
}