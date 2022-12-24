using System;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DNTCaptcha.Core
{
    /// <summary>
    ///     In-memory serialization provider
    /// </summary>
    public class InMemorySerializationProvider : ISerializationProvider
    {
        private readonly ICaptchaCryptoProvider _captchaProtectionProvider;
        private readonly IMemoryCache _memoryCache;
        private readonly DNTCaptchaOptions _options;

        /// <summary>
        ///     Serialization Provider
        /// </summary>
        public InMemorySerializationProvider(
            IMemoryCache memoryCache,
            ICaptchaCryptoProvider captchaProtectionProvider,
            ILogger<InMemorySerializationProvider> logger,
            IOptions<DNTCaptchaOptions> options)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            _options = options == null ? throw new ArgumentNullException(nameof(options)) : options.Value;
            _captchaProtectionProvider = captchaProtectionProvider ??
                                         throw new ArgumentNullException(nameof(captchaProtectionProvider));
            logger.LogDebug("Using the InMemorySerializationProvider.");
        }

        /// <summary>
        ///     Serialize the given data to an string.
        /// </summary>
        public string Serialize(object data)
        {
            var result = JsonSerializer.Serialize(data,
                                                  new JsonSerializerOptions
                                                  { WriteIndented = false, IgnoreNullValues = true });
            var token = _captchaProtectionProvider.Hash(result).HashString;
            _memoryCache.Set(token, result, new MemoryCacheEntryOptions
                                            {
                                                AbsoluteExpiration =
                                                    DateTimeOffset.UtcNow
                                                                  .AddMinutes(_options.AbsoluteExpirationMinutes),
                                                Size = 1, // the size limit is the count of entries
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
                return default;
            }

            _memoryCache.Remove(data);
            return JsonSerializer.Deserialize<T>(result);
        }
    }
}