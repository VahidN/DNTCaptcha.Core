using System;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace DNTCaptcha.Core;

/// <summary>
///     Defines DNTCaptcha's Options
/// </summary>
public class DNTCaptchaOptions
{
    /// <summary>
    ///     Its default value is `DNTCaptchaImage/[action]`
    /// </summary>
    public string? CaptchaImageControllerRouteTemplate { set; get; } = "DNTCaptchaImage/[action]";

    /// <summary>
    ///     Maximum number of permit counters that can be allowed in a minute. Must be set to a value > 0
    ///     Its default value is 10.
    /// </summary>
    public int PermitLimit { set; get; } = 10;

    /// <summary>
    ///     Defines options of the captcha's noise
    /// </summary>
    public DNTCaptchaNoise CaptchaNoise { get; set; } = new();

    /// <summary>
    ///     Gets or sets the value for the SameSite attribute of the cookie.
    ///     Its default value is `SameSiteMode.Strict`. If you are using CORS, set it to `None`.
    /// </summary>
    public SameSiteMode SameSiteMode { get; set; } = SameSiteMode.Strict;

    /// <summary>
    ///     You can introduce a custom ICaptchaStorageProvider to be used as an StorageProvider.
    /// </summary>
    public Type? CaptchaStorageProvider { get; set; }

    /// <summary>
    ///     You can introduce a custom SerializationProvider here.
    /// </summary>
    public Type? CaptchaSerializationProvider { get; set; }

    /// <summary>
    ///     You can introduce a custom font here.
    /// </summary>
    public string? CustomFontPath { get; set; }

    /// <summary>
    ///     The encryption key
    /// </summary>
    public string? EncryptionKey { get; set; }

    /// <summary>
    ///     Gets or sets an absolute expiration date for the cache entry.
    ///     Its default value is 7.
    /// </summary>
    public int AbsoluteExpirationMinutes { get; set; } = 7;

    /// <summary>
    ///     Shows thousands separators such as 100,100,100 in ShowDigits mode.
    ///     Its default value is true.
    /// </summary>
    public bool AllowThousandsSeparators { get; set; } = true;

    /// <summary>
    ///     Defines DNTCaptcha's input names
    /// </summary>
    public DNTCaptchaComponent CaptchaComponent { get; set; } = new();

    /// <summary>
    ///     The CSS class name of the captcha's DIV.
    ///     Its default value is `dntCaptcha`.
    /// </summary>
    public string CaptchaClass { get; set; } = "dntCaptcha";

    /// <summary>
    ///     Display exceptions in the response
    /// </summary>
    public bool ShowExceptions { set; get; }

    /// <summary>
    ///     Its default value is `NETESCAPADES_NONCE`
    ///     It's coming from https://github.com/andrewlock/NetEscapades.AspNetCore.SecurityHeaders
    ///     builder.AddScriptSrc().Self().WithNonce()
    /// </summary>
    public string NonceKey { set; get; } = "NETESCAPADES_NONCE";

    /// <summary>
    ///     Its default value is `NETESCAPADES_NONCE`.
    ///     It's coming from https://github.com/andrewlock/NetEscapades.AspNetCore.SecurityHeaders
    ///     builder.AddScriptSrc().Self().`WithNonce()` settings.
    ///     `WithNonce()` will generate the HttpContext.Items["NETESCAPADES_NONCE"] automatically.
    /// </summary>
    public DNTCaptchaOptions WithNonceKey(string key)
    {
        NonceKey = key;

        return this;
    }

    /// <summary>
    ///     Maximum number of permit counters that can be allowed in a minute for a given IP.
    ///     Must be set to a value > 0.
    ///     Its default value is 10.
    ///     Also you need to call app.UseRateLimiter() after calling app.UseRouting().
    /// </summary>
    public DNTCaptchaOptions RateLimiterPermitLimit(int permitLimit)
    {
        PermitLimit = permitLimit;

        return this;
    }

    /// <summary>
    ///     The CSS class name of the captcha's DIV.
    ///     Its default value is `dntCaptcha`.
    /// </summary>
    public DNTCaptchaOptions Identifier(string className)
    {
        CaptchaClass = className;

        return this;
    }

    /// <summary>
    ///     Defines DNTCaptcha's input names.
    /// </summary>
    public DNTCaptchaOptions InputNames(DNTCaptchaComponent component)
    {
        CaptchaComponent = component;

        return this;
    }

    /// <summary>
    ///     Shows thousands separators such as 100,100,100 in ShowDigits mode.
    ///     Its default value is true.
    /// </summary>
    public DNTCaptchaOptions ShowThousandsSeparators(bool show)
    {
        AllowThousandsSeparators = show;

        return this;
    }

    /// <summary>
    ///     Sets an absolute expiration date for the cache entry.
    ///     Its default value is 7.
    /// </summary>
    public DNTCaptchaOptions AbsoluteExpiration(int minutes)
    {
        AbsoluteExpirationMinutes = minutes;

        return this;
    }

    /// <summary>
    ///     The encryption key.
    ///     If you don't specify it, a random value will be used.
    /// </summary>
    public DNTCaptchaOptions WithEncryptionKey(string key)
    {
        EncryptionKey = key;

        return this;
    }

    /// <summary>
    ///     Defines options of the captcha's noise in SKShader.CreatePerlinNoiseTurbulence
    /// </summary>
    /// <param name="baseFrequencyX">
    ///     The frequency in the x-direction in the range of 0..1.
    ///     Its default value is 0.015f
    /// </param>
    /// <param name="baseFrequencyY">
    ///     The frequency in the y-direction in the range of 0..1.
    ///     Its default value is 0.015f
    /// </param>
    /// <param name="numOctaves">
    ///     The number of octaves, usually fairly small.
    ///     Its default value is 1
    /// </param>
    /// <param name="seed">
    ///     The randomization seed.
    ///     Its default value is 0
    /// </param>
    /// <returns></returns>
    public DNTCaptchaOptions WithNoise(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed)
    {
        CaptchaNoise = new DNTCaptchaNoise
        {
            BaseFrequencyX = baseFrequencyX,
            BaseFrequencyY = baseFrequencyY,
            NumOctaves = numOctaves,
            Seed = seed
        };

        return this;
    }

    /// <summary>
    ///     You can introduce a custom font here.
    /// </summary>
    public DNTCaptchaOptions UseCustomFont(string fullPath)
    {
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"`{fullPath}` file not found!");
        }

        CustomFontPath = fullPath;

        return this;
    }

    /// <summary>
    ///     You can introduce a custom ICaptchaStorageProvider to be used as an StorageProvider.
    /// </summary>
    /// <typeparam name="T">Implements ICaptchaStorageProvider</typeparam>
    public DNTCaptchaOptions UseCustomStorageProvider<T>()
        where T : ICaptchaStorageProvider
    {
        CaptchaStorageProvider = typeof(T);

        return this;
    }

    /// <summary>
    ///     Using the IDistributedCache
    ///     Don't forget to configure your DistributedCache provider such as `services.AddStackExchangeRedisCache()` first.
    /// </summary>
    public DNTCaptchaOptions UseDistributedSerializationProvider()
    {
        CaptchaSerializationProvider = typeof(DistributedSerializationProvider);

        return this;
    }

    /// <summary>
    ///     Using the IMemoryCache
    /// </summary>
    public DNTCaptchaOptions UseInMemorySerializationProvider()
    {
        CaptchaSerializationProvider = typeof(InMemorySerializationProvider);

        return this;
    }

    /// <summary>
    ///     Introduces the built-in `SessionCaptchaStorageProvider` to be used as an StorageProvider.
    ///     Don't forget to add `services.AddSession();` in ConfigureServices() method and `app.UseSession();` in Configure()
    ///     method.
    /// </summary>
    public DNTCaptchaOptions UseSessionStorageProvider()
    {
        CaptchaStorageProvider = typeof(SessionCaptchaStorageProvider);

        return this;
    }

    /// <summary>
    ///     Introduces the built-in `CookieCaptchaStorageProvider` to be used as an StorageProvider.
    /// </summary>
    /// <param name="sameSite">
    ///     Sets the value for the SameSite attribute of the cookie.
    ///     Its default value is `SameSiteMode.Strict`. If you are using CORS, set it to `None`.
    /// </param>
    public DNTCaptchaOptions UseCookieStorageProvider(SameSiteMode sameSite = SameSiteMode.Strict)
    {
        SameSiteMode = sameSite;
        CaptchaStorageProvider = typeof(CookieCaptchaStorageProvider);

        return this;
    }

    /// <summary>
    ///     Introduces the built-in `CookieCaptchaStorageProvider` to be used as an StorageProvider.
    /// </summary>
    public DNTCaptchaOptions UseMemoryCacheStorageProvider()
    {
        CaptchaStorageProvider = typeof(MemoryCacheCaptchaStorageProvider);

        return this;
    }

    /// <summary>
    ///     It will simplify the debugging by showing the exceptions in the response body.
    /// </summary>
    public DNTCaptchaOptions ShowExceptionsInResponse(bool enable)
    {
        ShowExceptions = enable;

        return this;
    }

    /// <summary>
    ///     Introduces the built-in `DistributedCacheCaptchaStorageProvider` to be used as an StorageProvider.
    ///     Don't forget to configure your DistributedCache provider such as `services.AddStackExchangeRedisCache()` first.
    /// </summary>
    public DNTCaptchaOptions UseDistributedCacheStorageProvider()
    {
        CaptchaStorageProvider = typeof(DistributedCacheCaptchaStorageProvider);
        CaptchaSerializationProvider = typeof(DistributedSerializationProvider);

        return this;
    }

    /// <summary>
    ///     Allow changing the route's template dynamically at run-time.
    ///     Its default value is `DNTCaptchaImage/[action]`
    /// </summary>
    /// <param name="template"></param>
    /// <returns></returns>
    public DNTCaptchaOptions WithCaptchaImageControllerRouteTemplate(string template)
    {
        CaptchaImageControllerRouteTemplate = template;

        return this;
    }
}