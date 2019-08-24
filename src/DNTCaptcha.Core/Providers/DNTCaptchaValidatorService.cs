using System;
using System.Globalization;
using DNTCaptcha.Core.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DNTCaptcha.Core.Providers
{
    /// <summary>
    ///
    /// </summary>
    public class DNTCaptchaValidatorResult
    {
        /// <summary>
        ///
        /// </summary>
        public bool IsValid { set; get; }

        /// <summary>
        ///
        /// </summary>
        public string ErrorMessage { set; get; }
    }

    /// <summary>
    ///
    /// </summary>
    public interface IDNTCaptchaValidatorService
    {
        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        DNTCaptchaValidatorResult Validate(
            HttpContext httpContext,
            string captchaText,
            string inputText,
            string cookieToken,
            Language captchaGeneratorLanguage,
            DisplayMode captchaGeneratorDisplayMode,
            string errorMessage,
            string isNumericErrorMessage,
            bool deleteCookieAfterValidation);
    }

    /// <summary>
    ///
    /// </summary>
    public class DNTCaptchaValidatorService : IDNTCaptchaValidatorService
    {
        private readonly ILogger<DNTCaptchaValidatorService> _logger;
        private readonly ICaptchaProtectionProvider _captchaProtectionProvider;
        private readonly ICaptchaStorageProvider _captchaStorageProvider;
        private readonly Func<DisplayMode, ICaptchaTextProvider> _captchaTextProvider;

        /// <summary>
        ///
        /// </summary>
        public DNTCaptchaValidatorService(
            ILogger<DNTCaptchaValidatorService> logger,
            ICaptchaProtectionProvider captchaProtectionProvider,
            ICaptchaStorageProvider captchaStorageProvider,
            Func<DisplayMode, ICaptchaTextProvider> captchaTextProvider
        )
        {
            logger.CheckArgumentNull(nameof(logger));
            _logger = logger;

            captchaProtectionProvider.CheckArgumentNull(nameof(captchaProtectionProvider));
            _captchaProtectionProvider = captchaProtectionProvider;

            captchaStorageProvider.CheckArgumentNull(nameof(captchaStorageProvider));
            _captchaStorageProvider = captchaStorageProvider;

            captchaTextProvider.CheckArgumentNull(nameof(captchaTextProvider));
            _captchaTextProvider = captchaTextProvider;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public DNTCaptchaValidatorResult Validate(
            HttpContext httpContext,
            string captchaText,
            string inputText,
            string cookieToken,
            Language captchaGeneratorLanguage,
            DisplayMode captchaGeneratorDisplayMode,
            string errorMessage,
            string isNumericErrorMessage,
            bool deleteCookieAfterValidation)
        {
            if (!shouldValidate(httpContext))
            {
                _logger.LogInformation($"Ignoring ValidateDNTCaptcha during `{httpContext.Request.Method}`.");
                return new DNTCaptchaValidatorResult { IsValid = true };
            }

            if (string.IsNullOrEmpty(captchaText))
            {
                _logger.LogInformation("CaptchaHiddenInput is empty.");
                return new DNTCaptchaValidatorResult { IsValid = false, ErrorMessage = errorMessage };
            }

            if (string.IsNullOrEmpty(inputText))
            {
                _logger.LogInformation("CaptchaInput is empty.");
                return new DNTCaptchaValidatorResult { IsValid = false, ErrorMessage = errorMessage };
            }

            inputText = inputText.ToEnglishNumbers();

            if (!long.TryParse(
                inputText,
                NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands,
                CultureInfo.InvariantCulture,
                 out long inputNumber))
            {
                _logger.LogInformation("inputText is not a number.");
                return new DNTCaptchaValidatorResult { IsValid = false, ErrorMessage = isNumericErrorMessage };
            }

            var decryptedText = _captchaProtectionProvider.Decrypt(captchaText);

            var numberToText = _captchaTextProvider(captchaGeneratorDisplayMode).GetText(inputNumber, captchaGeneratorLanguage);
            if (decryptedText == null || !decryptedText.Equals(numberToText))
            {
                _logger.LogInformation($"{decryptedText} != {numberToText}");
                return new DNTCaptchaValidatorResult { IsValid = false, ErrorMessage = errorMessage };
            }

            if (!isValidCookie(httpContext, decryptedText, cookieToken, deleteCookieAfterValidation))
            {
                return new DNTCaptchaValidatorResult { IsValid = false, ErrorMessage = errorMessage };
            }

            return new DNTCaptchaValidatorResult { IsValid = true };
        }

        private bool isValidCookie(HttpContext httpContext, string decryptedText, string cookieToken, bool deleteCookieAfterValidation)
        {
            if (string.IsNullOrEmpty(cookieToken))
            {
                _logger.LogInformation("CaptchaHiddenTokenName is empty.");
                return false;
            }

            cookieToken = _captchaProtectionProvider.Decrypt(cookieToken);
            if (string.IsNullOrEmpty(cookieToken))
            {
                _logger.LogInformation("CaptchaHiddenTokenName is invalid.");
                return false;
            }

            var cookieValue = _captchaStorageProvider.GetValue(httpContext, cookieToken);
            if (string.IsNullOrWhiteSpace(cookieValue))
            {
                _logger.LogInformation("isValidCookie:: cookieValue IsNullOrWhiteSpace.");
                return false;
            }

            var areEqual = cookieValue.Equals(decryptedText);
            if (!areEqual)
            {
                _logger.LogInformation($"isValidCookie:: {cookieValue} != {decryptedText}");
            }

            if (areEqual && deleteCookieAfterValidation)
            {
                _captchaStorageProvider.Remove(httpContext, cookieToken);
            }
            return areEqual;
        }

        private static bool shouldValidate(HttpContext context)
        {
            return string.Equals("POST", context.Request.Method, StringComparison.OrdinalIgnoreCase);
        }
    }
}