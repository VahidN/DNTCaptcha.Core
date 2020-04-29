using System;
using DNTCaptcha.Core.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace DNTCaptcha.Core.Providers
{
    /// <summary>
    /// Represents a memory cache storage to save the captcha tokens.
    /// </summary>
    public class MemoryCacheCaptchaStorageProvider : ICaptchaStorageProvider
    {
        private readonly ICaptchaProtectionProvider _captchaProtectionProvider;
        private readonly ILogger<MemoryCacheCaptchaStorageProvider> _logger;
        private const int LifeTimeInMinutes = 7;
        private readonly IMemoryCache _memoryCache;

        /// <summary>
        /// Represents the storage to save the captcha tokens.
        /// </summary>
        public MemoryCacheCaptchaStorageProvider(
            ICaptchaProtectionProvider captchaProtectionProvider,
            IMemoryCache memoryCache,
            ILogger<MemoryCacheCaptchaStorageProvider> logger)
        {
            captchaProtectionProvider.CheckArgumentNull(nameof(captchaProtectionProvider));
            logger.CheckArgumentNull(nameof(logger));
            memoryCache.CheckArgumentNull(nameof(memoryCache));

            _captchaProtectionProvider = captchaProtectionProvider;
            _logger = logger;
            _memoryCache = memoryCache;

            _logger.LogDebug("Using the MemoryCacheCaptchaStorageProvider.");
        }

        /// <summary>
        /// Adds the specified token and its value to the storage.
        /// </summary>
        public void Add(HttpContext context, string token, string value)
        {
            value = _captchaProtectionProvider.Encrypt($"{value}{context.GetSalt(_captchaProtectionProvider)}");
            _memoryCache.Set(token, value, new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(LifeTimeInMinutes),
                Size = 1 // the size limit is the count of entries
            });
        }

        /// <summary>
        /// Determines whether the <see cref="ICaptchaStorageProvider" /> contains a specific token.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="token">The specified token.</param>
        /// <returns>
        /// <c>True</c> if the value is found in the <see cref="ICaptchaStorageProvider" />; otherwise <c>false</c>.
        /// </returns>
        public bool Contains(HttpContext context, string token)
        {
            return _memoryCache.TryGetValue(token, out string _);
        }

        /// <summary>
        /// Gets the value associated with the specified token.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="token">The specified token.</param>
        public string GetValue(HttpContext context, string token)
        {
            if (!_memoryCache.TryGetValue(token, out string cookieValue))
            {
                _logger.LogDebug("Couldn't find the captcha cookie in the request.");
                return null;
            }

            _memoryCache.Remove(token);
            var decryptedValue = _captchaProtectionProvider.Decrypt(cookieValue);
            return decryptedValue?.Replace(context.GetSalt(_captchaProtectionProvider), string.Empty);
        }

        /// <summary>
        /// Removes the specified token from the storage.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="token">The specified token.</param>
        public void Remove(HttpContext context, string token)
        {
            _memoryCache.Remove(token);
        }
    }
}