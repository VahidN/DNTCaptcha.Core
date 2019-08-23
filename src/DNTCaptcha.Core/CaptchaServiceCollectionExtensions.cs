using System;
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
        public static void AddDNTCaptcha(
            this IServiceCollection services,
            Action<DNTCaptchaOptions> options = null)
        {
            services.CheckArgumentNull(nameof(services));

            configOptions(services, options);

            services.TryAddSingleton<IHumanReadableIntegerProvider, HumanReadableIntegerProvider>();
            services.TryAddSingleton<ICaptchaTextProvider, CaptchaTextProvider>();
            services.TryAddSingleton<IShowDigitsProvider, ShowDigitsProvider>();
            services.TryAddSingleton<ISumOfTwoNumbersProvider, SumOfTwoNumbersProvider>();
            services.TryAddSingleton<IRandomNumberProvider, RandomNumberProvider>();
            services.TryAddSingleton<ICaptchaImageProvider, CaptchaImageProvider>();
            services.TryAddSingleton<ICaptchaProtectionProvider, CaptchaProtectionProvider>();
            services.TryAddTransient<DNTCaptchaTagHelper>();
            services.TryAddTransient<IDNTCaptchaValidatorService, DNTCaptchaValidatorService>();
        }

        private static void configOptions(IServiceCollection services, Action<DNTCaptchaOptions> options)
        {
            var captchaOptions = new DNTCaptchaOptions();
            options?.Invoke(captchaOptions);

            if (captchaOptions.CaptchaStorageProvider == null)
            {
                services.TryAddSingleton<ICaptchaStorageProvider, CookieCaptchaStorageProvider>();
            }
            else
            {
                services.TryAddSingleton(typeof(ICaptchaStorageProvider), captchaOptions.CaptchaStorageProvider);
            }
        }
    }
}