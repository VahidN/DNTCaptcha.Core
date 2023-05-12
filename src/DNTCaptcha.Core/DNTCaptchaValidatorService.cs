using System;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DNTCaptcha.Core;

/// <summary>
///     Validates the input number.
/// </summary>
public class DNTCaptchaValidatorService : IDNTCaptchaValidatorService
{
    private readonly DNTCaptchaOptions _captchaOptions;
    private readonly ICaptchaCryptoProvider _captchaProtectionProvider;
    private readonly ICaptchaStorageProvider _captchaStorageProvider;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly ILogger<DNTCaptchaValidatorService> _logger;

    /// <summary>
    ///     Validates the input number.
    /// </summary>
    public DNTCaptchaValidatorService(
        IHttpContextAccessor contextAccessor,
        ILogger<DNTCaptchaValidatorService> logger,
        ICaptchaCryptoProvider captchaProtectionProvider,
        ICaptchaStorageProvider captchaStorageProvider,
        IOptions<DNTCaptchaOptions> options
    )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _captchaProtectionProvider = captchaProtectionProvider ??
                                     throw new ArgumentNullException(nameof(captchaProtectionProvider));
        _captchaStorageProvider =
            captchaStorageProvider ?? throw new ArgumentNullException(nameof(captchaStorageProvider));
        _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        _captchaOptions = options == null ? throw new ArgumentNullException(nameof(options)) : options.Value;
    }

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

        if (!int.TryParse(inputText,
                          NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands,
                          CultureInfo.InvariantCulture,
                          out var inputNumber))
        {
            _logger.LogDebug("inputText is not a number.");
            return false;
        }

        var decryptedText = _captchaProtectionProvider.Decrypt(captchaText);
        var numberToText = inputNumber.ToString(CultureInfo.InvariantCulture);
        if (decryptedText?.Equals(numberToText, StringComparison.Ordinal) != true)
        {
            _logger.LogDebug($"decryptedText:{decryptedText} != numberToText:{numberToText}");
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

        _captchaStorageProvider.Remove(httpContext, cookieToken);
        return areEqual;
    }

    private (string? captchaText, string? inputText, string? cookieToken) getFormValues(HttpContext httpContext)
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
                   throw new InvalidOperationException("`httpContext.Request.Form` is null.");
        var captchaTextForm = form[captchaHiddenInputName];
        var inputTextForm = form[captchaInputName];
        var cookieTokenForm = form[captchaHiddenTokenName];
        return (captchaTextForm, inputTextForm, cookieTokenForm);
    }

    private static bool shouldValidate(HttpContext context) =>
        string.Equals("POST", context.Request.Method, StringComparison.OrdinalIgnoreCase);
}