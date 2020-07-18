using System;
using System.IO;
using DNTCaptcha.Core.Providers;

namespace DNTCaptcha.Core.Contracts
{
    /// <summary>
    /// Defines DNTCaptcha's Options
    /// </summary>
    public class DNTCaptchaOptions
    {
        /// <summary>
        /// You can introduce a custom ICaptchaStorageProvider to be used as an StorageProvider.
        /// </summary>
        public Type CaptchaStorageProvider { get; set; }

        /// <summary>
        /// You can introduce a custom SerializationProvider here.
        /// </summary>
        public Type CaptchaSerializationProvider { get; set; }

        /// <summary>
        /// You can introduce a custom font here.
        /// </summary>
        public string CustomFontPath { get; set; }

        /// <summary>
        /// Gets or sets an absolute expiration date for the cache entry.
        /// Its default value is 7.
        /// </summary>
        public int AbsoluteExpirationMinutes { get; set; } = 7;

        /// <summary>
        /// Sets an absolute expiration date for the cache entry.
        /// Its default value is 7.
        /// </summary>
        public DNTCaptchaOptions AbsoluteExpiration(int minutes)
        {
            AbsoluteExpirationMinutes = minutes;

            return this;
        }

        /// <summary>
        /// You can introduce a custom font here.
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
        /// You can introduce a custom ICaptchaStorageProvider to be used as an StorageProvider.
        /// </summary>
        /// <typeparam name="T">Implements ICaptchaStorageProvider</typeparam>
        public DNTCaptchaOptions UseCustomStorageProvider<T>() where T : ICaptchaStorageProvider
        {
            CaptchaStorageProvider = typeof(T);
            return this;
        }

        /// <summary>
        /// Using the IDistributedCache
        /// Don't forget to configure your DistributedCache provider such as `services.AddStackExchangeRedisCache()` first.
        /// </summary>
        public DNTCaptchaOptions UseDistributedSerializationProvider()
        {
            CaptchaSerializationProvider = typeof(DistributedSerializationProvider);
            return this;
        }

        /// <summary>
        /// Using the IMemoryCache
        /// </summary>
        public DNTCaptchaOptions UseInMemorySerializationProvider()
        {
            CaptchaSerializationProvider = typeof(InMemorySerializationProvider);
            return this;
        }

        /// <summary>
        /// Introduces the built-in `SessionCaptchaStorageProvider` to be used as an StorageProvider.
        /// Don't forget to add `services.AddSession();` in ConfigureServices() method and `app.UseSession();` in Configure() method.
        /// </summary>
        public DNTCaptchaOptions UseSessionStorageProvider()
        {
            CaptchaStorageProvider = typeof(SessionCaptchaStorageProvider);
            return this;
        }

        /// <summary>
        /// Introduces the built-in `CookieCaptchaStorageProvider` to be used as an StorageProvider.
        /// </summary>
        public DNTCaptchaOptions UseCookieStorageProvider()
        {
            CaptchaStorageProvider = typeof(CookieCaptchaStorageProvider);
            return this;
        }

        /// <summary>
        /// Introduces the built-in `CookieCaptchaStorageProvider` to be used as an StorageProvider.
        /// </summary>
        public DNTCaptchaOptions UseMemoryCacheStorageProvider()
        {
            CaptchaStorageProvider = typeof(MemoryCacheCaptchaStorageProvider);
            return this;
        }

        /// <summary>
        /// Introduces the built-in `DistributedCacheCaptchaStorageProvider` to be used as an StorageProvider.
        /// Don't forget to configure your DistributedCache provider such as `services.AddStackExchangeRedisCache()` first.
        /// </summary>
        public DNTCaptchaOptions UseDistributedCacheStorageProvider()
        {
            CaptchaStorageProvider = typeof(DistributedCacheCaptchaStorageProvider);
            CaptchaSerializationProvider = typeof(DistributedSerializationProvider);
            return this;
        }
    }
}