using System;
using DNTCaptcha.Core.Contracts;
using Microsoft.Extensions.Caching.Memory;
#if NETCOREAPP3_0
using System.Text.Json;
#else
using Newtonsoft.Json;
#endif

namespace DNTCaptcha.Core.Providers
{
    /// <summary>
    /// Serialization Provider
    /// </summary>
    public class SerializationProvider : ISerializationProvider
    {
        private const int LifeTimeInMinutes = 7;
        private readonly IMemoryCache _memoryCache;
        private readonly ICaptchaProtectionProvider _captchaProtectionProvider;

        /// <summary>
        /// Serialization Provider
        /// </summary>
        public SerializationProvider(IMemoryCache memoryCache, ICaptchaProtectionProvider captchaProtectionProvider)
        {
            memoryCache.CheckArgumentNull(nameof(memoryCache));
            _memoryCache = memoryCache;
            _captchaProtectionProvider = captchaProtectionProvider;
        }

        /// <summary>
        /// Serialize the given data to an string.
        /// </summary>
        public string Serialize(object data)
        {
            var result = serialize(data);
            var token = _captchaProtectionProvider.Hash(result);
            _memoryCache.Set(token, result, new MemoryCacheEntryOptions
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
            if (!_memoryCache.TryGetValue(token, out string result))
            {
                return default;
            }

            _memoryCache.Remove(token);
            return deserialize<T>(result);
        }

        private string serialize(object data)
        {
#if NETCOREAPP3_0
            return JsonSerializer.Serialize(data,
                new JsonSerializerOptions
                {
                    WriteIndented = false,
                    IgnoreNullValues = true
                });
#else
            return JsonConvert.SerializeObject(data,
                                                new JsonSerializerSettings
                                                {
                                                    Formatting = Formatting.None,
                                                    NullValueHandling = NullValueHandling.Ignore,
                                                    DefaultValueHandling = DefaultValueHandling.Include
                                                });
#endif
        }

        private T deserialize<T>(string data)
        {
#if NETCOREAPP3_0
            return JsonSerializer.Deserialize<T>(data);
#else
            return JsonConvert.DeserializeObject<T>(data);
#endif
        }
    }
}