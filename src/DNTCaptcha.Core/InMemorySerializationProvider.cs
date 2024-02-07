using System;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DNTCaptcha.Core;

/// <summary>
///     In-memory serialization provider
/// </summary>
public class InMemorySerializationProvider : ISerializationProvider
{
    private readonly ICaptchaCryptoProvider _captchaProtectionProvider;

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = false,
        IgnoreNullValues = true
    };

    private readonly ILogger<InMemorySerializationProvider> _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly DNTCaptchaOptions _options;

    /// <summary>
    ///     Serialization Provider
    /// </summary>
    public InMemorySerializationProvider(IMemoryCache memoryCache,
        ICaptchaCryptoProvider captchaProtectionProvider,
        ILogger<InMemorySerializationProvider> logger,
        IOptions<DNTCaptchaOptions> options)
    {
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _options = options == null ? throw new ArgumentNullException(nameof(options)) : options.Value;

        _captchaProtectionProvider = captchaProtectionProvider ??
                                     throw new ArgumentNullException(nameof(captchaProtectionProvider));

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        logger.LogDebug("Using the InMemorySerializationProvider.");
    }

    /// <summary>
    ///     Serialize the given data to an string.
    /// </summary>
    public string Serialize(object data)
    {
        var result = JsonSerializer.Serialize(data, _jsonSerializerOptions);
        var token = _captchaProtectionProvider.Hash(result).HashString;

        _memoryCache.Set(token, result, new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(_options.AbsoluteExpirationMinutes),
            Size = 1 // the size limit is the count of entries
        });

        return token;
    }

    /// <summary>
    ///     Deserialize the given string to an object.
    /// </summary>
    public T? Deserialize<T>(string data)
    {
        if (!_memoryCache.TryGetValue(data, out string? result) || result is null)
        {
            _logger.LogDebug(
                "The registered memory cache provider returned null. Which means your data is expired. Please read the `How to choose a correct storage mode` in the readme file. Probably a local `memory cache` shouldn't be used with your distributed servers.");

            return default;
        }

        _memoryCache.Remove(data);

        return JsonSerializer.Deserialize<T>(result);
    }
}