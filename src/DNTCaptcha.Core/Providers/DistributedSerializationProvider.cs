using System;
using System.Text;
using System.Text.Json;
using DNTCaptcha.Core.Contracts;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DNTCaptcha.Core.Providers
{
    /// <summary>
    /// Distributed serialization provider
    /// </summary>
    public class DistributedSerializationProvider : ISerializationProvider
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ICaptchaProtectionProvider _captchaProtectionProvider;
        private readonly ILogger<DistributedSerializationProvider> _logger;
        private readonly DNTCaptchaOptions _options;

        /// <summary>
        /// Serialization Provider
        /// </summary>
        public DistributedSerializationProvider(
            IDistributedCache distributedCache,
            ICaptchaProtectionProvider captchaProtectionProvider,
            ILogger<DistributedSerializationProvider> logger,
            IOptions<DNTCaptchaOptions> options)
        {
            distributedCache.CheckArgumentNull(nameof(distributedCache));
            _distributedCache = distributedCache;
            _logger = logger;
            _captchaProtectionProvider = captchaProtectionProvider;
            _options = options.Value;
            _logger.LogDebug("Using the DistributedSerializationProvider.");
        }

        /// <summary>
        /// Serialize the given data to an string.
        /// </summary>
        public string Serialize(object data)
        {
            var resultBytes = JsonSerializer.SerializeToUtf8Bytes(data,
                    new JsonSerializerOptions { WriteIndented = false, IgnoreNullValues = true });
            var token = _captchaProtectionProvider.Hash(Encoding.UTF8.GetString(resultBytes));
            _distributedCache.Set(token, resultBytes, new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(_options.AbsoluteExpirationMinutes)
            });
            return token;
        }

        /// <summary>
        /// Deserialize the given string to an object.
        /// </summary>
        public T Deserialize<T>(string token)
        {
            var resultBytes = _distributedCache.Get(token);
            if (resultBytes == null)
            {
                return default;
            }

            _distributedCache.Remove(token);
            return JsonSerializer.Deserialize<T>(new ReadOnlySpan<byte>(resultBytes));
        }
    }
}