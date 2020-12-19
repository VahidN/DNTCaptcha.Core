using System;
using System.Globalization;
using DNTCaptcha.Core.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DNTCaptcha.Core.Providers
{
    /// <summary>
    /// Validates the input number.
    /// </summary>
    public interface IDNTCaptchaValidatorService
    {
        /// <summary>
        /// Validates the input number.
        /// </summary>
        /// <param name="captchaGeneratorLanguage">The Number to word language.</param>
        /// <param name="captchaGeneratorDisplayMode">Display mode of the captcha's text.</param>
        /// <param name="model">
        /// Set this parameter if your form is a JSON one and not a `httpContext.Request.HasFormContentType`.
        /// This method will try to parse the `Request.Form` first to find (captchaText, inputText, cookieToken) automatically,
        /// Otherwise you should provide these values directly.
        /// </param>
        bool HasRequestValidCaptchaEntry(Language captchaGeneratorLanguage,
            DisplayMode captchaGeneratorDisplayMode,
            DNTCaptchaBase? model = null);
    }

    /// <summary>
    /// Validates the input number.
    /// </summary>
    public class DNTCaptchaValidatorService : IDNTCaptchaValidatorService
    {
        private readonly ILogger<DNTCaptchaValidatorService> _logger;
        private readonly ICaptchaCryptoProvider _captchaProtectionProvider;
        private readonly ICaptchaStorageProvider _captchaStorageProvider;
        private readonly Func<DisplayMode, ICaptchaTextProvider> _captchaTextProvider;
        private readonly IHttpContextAccessor _contextAccessor;

        /// <summary>
        /// Validates the input number.
        /// </summary>
        public DNTCaptchaValidatorService(
            IHttpContextAccessor contextAccessor,
            ILogger<DNTCaptchaValidatorService> logger,
            ICaptchaCryptoProvider captchaProtectionProvider,
            ICaptchaStorageProvider captchaStorageProvider,
            Func<DisplayMode, ICaptchaTextProvider> captchaTextProvider
        )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _captchaProtectionProvider = captchaProtectionProvider ?? throw new ArgumentNullException(nameof(captchaProtectionProvider));
            _captchaStorageProvider = captchaStorageProvider ?? throw new ArgumentNullException(nameof(captchaStorageProvider));
            _captchaTextProvider = captchaTextProvider ?? throw new ArgumentNullException(nameof(captchaTextProvider));
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        }

        /// <summary>
        /// Validates the input number.
        /// </summary>
        /// <returns></returns>
        public bool HasRequestValidCaptchaEntry(
            Language captchaGeneratorLanguage,
            DisplayMode captchaGeneratorDisplayMode,
            DNTCaptchaBase? model = null)
        {
            var httpContext = _contextAccessor.HttpContext;
            if (httpContext == null)
            {
                return false;
            }

            if (!shouldValidate(httpContext))
            {
                _logger.LogDebug($"Ignoring ValidateDNTCaptcha during `{httpContext.Request.Method}`.");
                return true;
            }

            var (captchaText, inputText, cookieToken) = getFormValues(httpContext, model);

            if (string.IsNullOrEmpty(captchaText))
            {
                _logger.LogDebug("CaptchaHiddenInput is empty.");
                return false;
            }

            if (string.IsNullOrEmpty(inputText))
            {
                _logger.LogDebug("CaptchaInput is empty.");
                return false;
            }

            inputText = inputText.ToEnglishNumbers();

            if (!long.TryParse(
                inputText,
                NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands,
                CultureInfo.InvariantCulture,
                out long inputNumber))
            {
                _logger.LogDebug("inputText is not a number.");
                return false;
            }

            var decryptedText = _captchaProtectionProvider.Decrypt(captchaText);

            var numberToText = _captchaTextProvider(captchaGeneratorDisplayMode).GetText(inputNumber, captchaGeneratorLanguage);
            if (decryptedText?.Equals(numberToText, StringComparison.Ordinal) != true)
            {
                _logger.LogDebug($"{decryptedText} != {numberToText}");
                return false;
            }

            if (!isValidCookie(httpContext, decryptedText, cookieToken))
            {
                return false;
            }

            return true;
        }

        private bool isValidCookie(HttpContext httpContext, string decryptedText, string? cookieToken)
        {
            if (string.IsNullOrEmpty(cookieToken))
            {
                _logger.LogDebug("CaptchaHiddenTokenName is empty.");
                return false;
            }

            cookieToken = _captchaProtectionProvider.Decrypt(cookieToken);
            if (string.IsNullOrEmpty(cookieToken))
            {
                _logger.LogDebug("CaptchaHiddenTokenName is invalid.");
                return false;
            }

            var cookieValue = _captchaStorageProvider.GetValue(httpContext, cookieToken);
            if (string.IsNullOrWhiteSpace(cookieValue))
            {
                _logger.LogDebug("isValidCookie:: cookieValue IsNullOrWhiteSpace.");
                return false;
            }

            var areEqual = cookieValue.Equals(decryptedText, StringComparison.Ordinal);
            if (!areEqual)
            {
                _logger.LogDebug($"isValidCookie:: {cookieValue} != {decryptedText}");
            }

            if (areEqual)
            {
                _captchaStorageProvider.Remove(httpContext, cookieToken);
            }
            return areEqual;
        }

        private static (string captchaText, string inputText, string cookieToken) getFormValues(HttpContext httpContext, DNTCaptchaBase? model)
        {
            if (httpContext.Request.HasFormContentType)
            {
                var form = httpContext.Request.Form ?? throw new InvalidOperationException("`httpContext.Request.Form` is null.");
                var captchaText = (string)form[DNTCaptchaTagHelper.CaptchaHiddenInputName];
                var inputText = (string)form[DNTCaptchaTagHelper.CaptchaInputName];
                var cookieToken = (string)form[DNTCaptchaTagHelper.CaptchaHiddenTokenName];

                return (captchaText, inputText, cookieToken);
            }

            if (model == null)
            {
                throw new InvalidOperationException("Your ViewModel should implement the DNTCaptchaBase class (public class AccountViewModel : DNTCaptchaBase {}).");
            }
            return (model.DNTCaptchaText, model.DNTCaptchaInputText, model.DNTCaptchaToken);
        }

        private static bool shouldValidate(HttpContext context)
        {
            return string.Equals("POST", context.Request.Method, StringComparison.OrdinalIgnoreCase);
        }
    }
}