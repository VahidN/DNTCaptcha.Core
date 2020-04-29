using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using DNTCaptcha.Core.Contracts;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace DNTCaptcha.Core.Providers
{
    /// <summary>
    /// Distributed serialization provider
    /// </summary>
    public class DistributedSerializationProvider : ISerializationProvider
    {
        private const int LifeTimeInMinutes = 7;
        private readonly IDistributedCache _distributedCache;
        private readonly ICaptchaProtectionProvider _captchaProtectionProvider;
        private readonly ILogger<DistributedSerializationProvider> _logger;

        /// <summary>
        /// Serialization Provider
        /// </summary>
        public DistributedSerializationProvider(
            IDistributedCache distributedCache,
            ICaptchaProtectionProvider captchaProtectionProvider,
            ILogger<DistributedSerializationProvider> logger)
        {
            distributedCache.CheckArgumentNull(nameof(distributedCache));
            _distributedCache = distributedCache;
            _logger = logger;
            _captchaProtectionProvider = captchaProtectionProvider;
            _logger.LogDebug("Using the DistributedSerializationProvider.");
        }

        /// <summary>
        /// Serialize the given data to an string.
        /// </summary>
        public string Serialize(object data)
        {
            var resultBytes = serialize(data);
            var token = _captchaProtectionProvider.Hash(Encoding.UTF8.GetString(resultBytes));
            _distributedCache.Set(token, resultBytes, new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(LifeTimeInMinutes)
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
            return deserialize<T>(resultBytes);
        }

        private byte[] serialize(object value)
        {
            var serializer = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, value);
                return stream.ToArray();
            }
        }

        private T deserialize<T>(byte[] value)
        {
            var serializer = new BinaryFormatter();
            using (var stream = new MemoryStream(value))
            {
                return (T)serializer.Deserialize(stream);
            }
        }
    }
}