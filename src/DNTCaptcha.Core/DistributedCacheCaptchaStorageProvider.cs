using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DNTCaptcha.Core;

/// <summary>
///     Represents a distributed cache storage to save the captcha tokens.
/// </summary>
/// <remarks>
///     Represents the storage to save the captcha tokens.
/// </remarks>
public class DistributedCacheCaptchaStorageProvider(
    ICaptchaCryptoProvider captchaProtectionProvider,
    IDistributedCache distributedCache,
    ILogger<DistributedCacheCaptchaStorageProvider> logger,
    IOptions<DNTCaptchaOptions> options) : ICaptchaStorageProvider
{
    private readonly ICaptchaCryptoProvider _captchaProtectionProvider = captchaProtectionProvider ??
                                                                         throw new ArgumentNullException(
                                                                             nameof(captchaProtectionProvider));

    private readonly IDistributedCache _distributedCache =
        distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));

    private readonly ILogger<DistributedCacheCaptchaStorageProvider> _logger =
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

        _distributedCache.Set(token, Encoding.UTF8.GetBytes(value), new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(_options.AbsoluteExpirationMinutes)
        });
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
        => !string.IsNullOrWhiteSpace(token) && _distributedCache.Get(token) != null;

    /// <summary>
    ///     Gets the value associated with the specified token.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="token">The specified token.</param>
    public string? GetValue(HttpContext context, string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var cookieValueBytes = _distributedCache.Get(token);

        if (cookieValueBytes == null)
        {
            _logger.LogDebug(message: "Couldn't find the captcha cookie in the request.");

            return null;
        }

        _distributedCache.Remove(token);
        var decryptedValue = _captchaProtectionProvider.Decrypt(Encoding.UTF8.GetString(cookieValueBytes));

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
        if (!string.IsNullOrWhiteSpace(token))
        {
            _distributedCache.Remove(token);
        }
    }
}