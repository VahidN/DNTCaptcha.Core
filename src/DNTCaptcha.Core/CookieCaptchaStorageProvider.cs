using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DNTCaptcha.Core;

/// <summary>
///     Represents the default storage to save the captcha tokens.
/// </summary>
/// <remarks>
///     Represents the storage to save the captcha tokens.
/// </remarks>
public class CookieCaptchaStorageProvider(
    ICaptchaCryptoProvider captchaProtectionProvider,
    ILogger<CookieCaptchaStorageProvider> logger,
    IOptions<DNTCaptchaOptions> options) : ICaptchaStorageProvider
{
    private readonly ICaptchaCryptoProvider _captchaProtectionProvider = captchaProtectionProvider ??
                                                                         throw new ArgumentNullException(
                                                                             nameof(captchaProtectionProvider));

    private readonly ILogger<CookieCaptchaStorageProvider> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    private readonly DNTCaptchaOptions _options =
        options == null ? throw new ArgumentNullException(nameof(options)) : options.Value;

    /// <summary>
    ///     Adds the specified token and its value to the storage.
    /// </summary>
    public void Add(HttpContext context, string? token, string value)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentNullException(nameof(token));
        }

        value = _captchaProtectionProvider.Encrypt($"{value}{context.GetSalt(_captchaProtectionProvider)}");
        context.Response.Cookies.Append(token, value, GetCookieOptions(context));
    }

    /// <summary>
    ///     Determines whether the <see cref="ICaptchaStorageProvider" /> contains a specific token.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="token">The specified token.</param>
    /// <returns>
    ///     <c>True</c> if the value is found in the <see cref="ICaptchaStorageProvider" />; otherwise <c>false</c>.
    /// </returns>
    public bool Contains(HttpContext context, [NotNullWhen(returnValue: true)] string? token)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        return !string.IsNullOrWhiteSpace(token) && context.Request.Cookies.ContainsKey(token);
    }

    /// <summary>
    ///     Gets the value associated with the specified token.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="token">The specified token.</param>
    public string? GetValue(HttpContext context, string? token)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (string.IsNullOrWhiteSpace(token) || !context.Request.Cookies.TryGetValue(token, out var cookieValue))
        {
            _logger.LogDebug(message: "Couldn't find the captcha cookie in the request.");

            return null;
        }

        Remove(context, token);

        if (string.IsNullOrWhiteSpace(cookieValue))
        {
            _logger.LogDebug(message: "Couldn't find the captcha cookie's value in the request.");

            return null;
        }

        var decryptedValue = _captchaProtectionProvider.Decrypt(cookieValue);

        return decryptedValue?.Replace(context.GetSalt(_captchaProtectionProvider), string.Empty,
            StringComparison.Ordinal);
    }

    /// <summary>
    ///     Removes the specified token from the storage.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="token">The specified token.</param>
    public void Remove(HttpContext context, string? token)
    {
        if (Contains(context, token))
        {
            context.Response.Cookies.Delete(token, GetCookieOptions(context));
        }
    }

    /// <summary>
    ///     Expires at the end of the browser's session.
    /// </summary>
    private CookieOptions GetCookieOptions(HttpContext context)
        => new()
        {
            HttpOnly = true,
            Path = context.Request.PathBase.HasValue ? context.Request.PathBase.ToString() : "/",
            Secure = context.Request.IsHttps,
            Expires = DateTimeOffset.UtcNow.AddMinutes(_options.AbsoluteExpirationMinutes),
            IsEssential = true,
            SameSite = _options.SameSiteMode
        };
}