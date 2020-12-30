using System;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DNTCaptcha.Core
{
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
        private readonly DNTCaptchaOptions _captchaOptions;

        /// <summary>
        /// Validates the input number.
        /// </summary>
        public DNTCaptchaValidatorService(
            IHttpContextAccessor contextAccessor,
            ILogger<DNTCaptchaValidatorService> logger,
            ICaptchaCryptoProvider captchaProtectionProvider,
            ICaptchaStorageProvider captchaStorageProvider,
            Func<DisplayMode, ICaptchaTextProvider> captchaTextProvider,
            IOptions<DNTCaptchaOptions> options
        )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _captchaProtectionProvider = captchaProtectionProvider ?? throw new ArgumentNullException(nameof(captchaProtectionProvider));
            _captchaStorageProvider = captchaStorageProvider ?? throw new ArgumentNullException(nameof(captchaStorageProvider));
            _captchaTextProvider = captchaTextProvider ?? throw new ArgumentNullException(nameof(captchaTextProvider));
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
            _captchaOptions = options == null ? throw new ArgumentNullException(nameof(options)) : options.Value;
        }

        /// <summary>
        /// Validates the input number.
        /// </summary>
        /// <returns></returns>
        public bool HasRequestValidCaptchaEntry(
            Language captchaGeneratorLanguage,
            DisplayMode captchaGeneratorDisplayMode)
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

            var (captchaText, inputText, cookieToken) = getFormValues(httpContext);

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

            return isValidCookie(httpContext, decryptedText, cookieToken);
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

        private (string captchaText, string inputText, string cookieToken) getFormValues(HttpContext httpContext)
        {
            var captchaHiddenInputName = _captchaOptions.CaptchaComponent.CaptchaHiddenInputName;
            var captchaInputName = _captchaOptions.CaptchaComponent.CaptchaInputName;
            var captchaHiddenTokenName = _captchaOptions.CaptchaComponent.CaptchaHiddenTokenName;

            if (!httpContext.Request.HasFormContentType)
            {
                throw new InvalidOperationException(
                    $"DNTCaptcha expects `{captchaHiddenInputName}`, `{captchaInputName}` & `{captchaHiddenTokenName}` fields to be present in the posted `form` data." +
                    " Also don't post it as a JSON data. Its `Content-Type` should be `application/x-www-form-urlencoded`.");
            }

            var form = httpContext.Request.Form ?? throw new InvalidOperationException("`httpContext.Request.Form` is null.");
            var captchaTextForm = (string)form[captchaHiddenInputName];
            var inputTextForm = (string)form[captchaInputName];
            var cookieTokenForm = (string)form[captchaHiddenTokenName];
            return (captchaTextForm, inputTextForm, cookieTokenForm);
        }

        private static bool shouldValidate(HttpContext context)
        {
            return string.Equals("POST", context.Request.Method, StringComparison.OrdinalIgnoreCase);
        }
    }
}