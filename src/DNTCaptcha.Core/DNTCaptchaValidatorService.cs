using System;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DNTCaptcha.Core;

/// <summary>
///     Validates the input number.
/// </summary>
/// <remarks>
///     Validates the input number.
/// </remarks>
public class DNTCaptchaValidatorService(
    IHttpContextAccessor contextAccessor,
    ILogger<DNTCaptchaValidatorService> logger,
    ICaptchaCryptoProvider captchaProtectionProvider,
    ICaptchaStorageProvider captchaStorageProvider,
    IOptions<DNTCaptchaOptions> options) : IDNTCaptchaValidatorService
{
    private readonly DNTCaptchaOptions _captchaOptions =
        options == null ? throw new ArgumentNullException(nameof(options)) : options.Value;

    private readonly ICaptchaCryptoProvider _captchaProtectionProvider = captchaProtectionProvider ??
                                                                         throw new ArgumentNullException(
                                                                             nameof(captchaProtectionProvider));

    private readonly ICaptchaStorageProvider _captchaStorageProvider =
        captchaStorageProvider ?? throw new ArgumentNullException(nameof(captchaStorageProvider));

    private readonly IHttpContextAccessor _contextAccessor =
        contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));

    private readonly ILogger<DNTCaptchaValidatorService> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    ///     Validates the input number.
    /// </summary>
    /// <returns></returns>
    public bool HasRequestValidCaptchaEntry()
    {
        var httpContext = _contextAccessor.HttpContext;

        if (httpContext == null)
        {
            return false;
        }

        if (!ShouldValidate(httpContext))
        {
            _logger.LogDebug(message: "Ignoring ValidateDNTCaptcha during `{Method}`.", httpContext.Request.Method);

            return true;
        }

        var (captchaText, inputText, cookieToken) = GetFormValues(httpContext);

        if (string.IsNullOrEmpty(captchaText))
        {
            _logger.LogDebug(message: "CaptchaHiddenInput is empty.");

            return false;
        }

        if (string.IsNullOrEmpty(inputText))
        {
            _logger.LogDebug(message: "CaptchaInput is empty.");

            return false;
        }

        inputText = inputText.ToEnglishNumbers();

        if (!int.TryParse(inputText, NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands,
                CultureInfo.InvariantCulture, out var inputNumber))
        {
            _logger.LogDebug(message: "inputText is not a number.");

            return false;
        }

        var decryptedText = _captchaProtectionProvider.Decrypt(captchaText);
        var numberToText = inputNumber.ToString(CultureInfo.InvariantCulture);

        if (decryptedText?.Equals(numberToText, StringComparison.Ordinal) != true)
        {
            _logger.LogDebug(message: "decryptedText:{DecryptedText} != numberToText:{NumberToText}", decryptedText,
                numberToText);

            return false;
        }

        return IsValidCookie(httpContext, decryptedText, cookieToken);
    }

    private bool IsValidCookie(HttpContext httpContext, string decryptedText, string? cookieToken)
    {
        if (string.IsNullOrEmpty(cookieToken))
        {
            _logger.LogDebug(message: "CaptchaHiddenTokenName is empty.");

            return false;
        }

        cookieToken = _captchaProtectionProvider.Decrypt(cookieToken);

        if (string.IsNullOrEmpty(cookieToken))
        {
            _logger.LogDebug(message: "CaptchaHiddenTokenName is invalid.");

            return false;
        }

        var cookieValue = _captchaStorageProvider.GetValue(httpContext, cookieToken);

        if (string.IsNullOrWhiteSpace(cookieValue))
        {
            _logger.LogDebug(message: "isValidCookie:: cookieValue IsNullOrWhiteSpace.");

            return false;
        }

        var areEqual = cookieValue.Equals(decryptedText, StringComparison.Ordinal);

        if (!areEqual)
        {
            _logger.LogDebug(message: "isValidCookie:: {CookieValue} != {DecryptedText}", cookieValue, decryptedText);
        }

        _captchaStorageProvider.Remove(httpContext, cookieToken);

        return areEqual;
    }

    private (string? captchaText, string? inputText, string? cookieToken) GetFormValues(HttpContext httpContext)
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

        var form = httpContext.Request.Form ??
                   throw new InvalidOperationException(message: "`httpContext.Request.Form` is null.");

        var captchaTextForm = form[captchaHiddenInputName];
        var inputTextForm = form[captchaInputName];
        var cookieTokenForm = form[captchaHiddenTokenName];

        return (captchaTextForm, inputTextForm, cookieTokenForm);
    }

    private static bool ShouldValidate(HttpContext context)
        => string.Equals(a: "POST", context.Request.Method, StringComparison.OrdinalIgnoreCase);
}