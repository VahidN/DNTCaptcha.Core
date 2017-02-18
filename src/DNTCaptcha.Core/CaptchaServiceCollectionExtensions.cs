using DNTCaptcha.Core.Contracts;
using DNTCaptcha.Core.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DNTCaptcha.Core
{
    /// <summary>
    ///  Captcha ServiceCollection Extensions
    /// </summary>
    public static class CaptchaServiceCollectionExtensions
    {
        /// <summary>
        /// Adds default DNTCaptcha providers.
        /// </summary>
        public static void AddDNTCaptcha(this IServiceCollection services)
        {
            services.CheckArgumentNull(nameof(services));

            services.TryAddSingleton<ICaptchaStorageProvider, CookieCaptchaStorageProvider>();
            services.TryAddSingleton<IHumanReadableIntegerProvider, HumanReadableIntegerProvider>();
            services.TryAddSingleton<IRandomNumberProvider, RandomNumberProvider>();
            services.TryAddSingleton<ICaptchaImageProvider, CaptchaImageProvider>();
            services.TryAddSingleton<ICaptchaProtectionProvider, CaptchaProtectionProvider>();
            services.AddTransient<DNTCaptchaTagHelper>();
        }
    }
}