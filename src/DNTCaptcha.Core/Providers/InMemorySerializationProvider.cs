using System;
using DNTCaptcha.Core.Contracts;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace DNTCaptcha.Core.Providers
{
    /// <summary>
    /// In-memory serialization provider
    /// </summary>
    public class InMemorySerializationProvider : ISerializationProvider
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ICaptchaCryptoProvider _captchaProtectionProvider;
        private readonly ILogger<InMemorySerializationProvider> _logger;
        private readonly DNTCaptchaOptions _options;

        /// <summary>
        /// Serialization Provider
        /// </summary>
        public InMemorySerializationProvider(
            IMemoryCache memoryCache,
            ICaptchaCryptoProvider captchaProtectionProvider,
            ILogger<InMemorySerializationProvider> logger,
            IOptions<DNTCaptchaOptions> options)
        {
            memoryCache.CheckArgumentNull(nameof(memoryCache));
            _memoryCache = memoryCache;
            _logger = logger;
            _options = options.Value;
            _captchaProtectionProvider = captchaProtectionProvider;
            _logger.LogDebug("Using the InMemorySerializationProvider.");
        }

        /// <summary>
        /// Serialize the given data to an string.
        /// </summary>
        public string Serialize(object data)
        {
            var result = JsonSerializer.Serialize(data,
                        new JsonSerializerOptions { WriteIndented = false, IgnoreNullValues = true });
            var token = _captchaProtectionProvider.Hash(result).HashString;
            _memoryCache.Set(token, result, new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(_options.AbsoluteExpirationMinutes),
                Size = 1 // the size limit is the count of entries
            });
            return token;
        }

        /// <summary>
        /// Deserialize the given string to an object.
        /// </summary>
        public T Deserialize<T>(string token)
        {
            if (!_memoryCache.TryGetValue(token, out string result))
            {
                return default;
            }

            _memoryCache.Remove(token);
            return JsonSerializer.Deserialize<T>(result);
        }
    }
}