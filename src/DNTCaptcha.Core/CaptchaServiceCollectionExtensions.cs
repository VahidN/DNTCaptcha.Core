using System;
using DNTCaptcha.Core.Contracts;
using DNTCaptcha.Core.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
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

            services.AddMemoryCache();
            services.AddHttpContextAccessor();
            services.AddDataProtection();
            services.AddAntiforgery();
            services.AddMvcCore().AddCookieTempDataProvider();

            services.TryAddSingleton<HumanReadableIntegerProvider>();
            services.TryAddSingleton<ShowDigitsProvider>();
            services.TryAddSingleton<SumOfTwoNumbersProvider>();
            services.TryAddSingleton<SumOfTwoNumbersToWordsProvider>();
            services.TryAddSingleton<Func<DisplayMode, ICaptchaTextProvider>>(serviceProvider => key =>
            {
                switch (key)
                {
                    case DisplayMode.NumberToWord:
                        return serviceProvider.GetRequiredService<HumanReadableIntegerProvider>();
                    case DisplayMode.ShowDigits:
                        return serviceProvider.GetRequiredService<ShowDigitsProvider>();
                    case DisplayMode.SumOfTwoNumbers:
                        return serviceProvider.GetRequiredService<SumOfTwoNumbersProvider>();
                    case DisplayMode.SumOfTwoNumbersToWords:
                        return serviceProvider.GetRequiredService<SumOfTwoNumbersToWordsProvider>();
                    default:
                        throw new NotImplementedException($"Service of type {key} is not implemented.");
                }
            });

            services.TryAddSingleton<IRandomNumberProvider, RandomNumberProvider>();
            services.TryAddSingleton<ICaptchaImageProvider, CaptchaImageProvider>();
            services.TryAddSingleton<ICaptchaProtectionProvider, CaptchaProtectionProvider>();
            services.TryAddTransient<DNTCaptchaTagHelper>();
            services.TryAddTransient<IDNTCaptchaValidatorService, DNTCaptchaValidatorService>();
            services.TryAddScoped<IDNTCaptchaApiProvider, DNTCaptchaApiProvider>();
            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.TryAddScoped<IUrlHelper>(serviceProvider =>
            {
                var actionContext = serviceProvider.GetRequiredService<IActionContextAccessor>().ActionContext;
                var factory = serviceProvider.GetRequiredService<IUrlHelperFactory>();
                return factory.GetUrlHelper(actionContext);
            });
        }

        private static void configOptions(IServiceCollection services, Action<DNTCaptchaOptions> options)
        {
            var captchaOptions = new DNTCaptchaOptions();
            options?.Invoke(captchaOptions);
            setCaptchaStorageProvider(services, captchaOptions);
            setSerializationProvider(services, captchaOptions);
        }

        private static void setSerializationProvider(IServiceCollection services, DNTCaptchaOptions captchaOptions)
        {
            if (captchaOptions.CaptchaSerializationProvider == null)
            {
                services.TryAddSingleton<ISerializationProvider, InMemorySerializationProvider>();
            }
            else
            {
                services.TryAddSingleton(typeof(ISerializationProvider), captchaOptions.CaptchaSerializationProvider);
            }
        }

        private static void setCaptchaStorageProvider(IServiceCollection services, DNTCaptchaOptions captchaOptions)
        {
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