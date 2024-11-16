using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace DNTCaptcha.Core;

/// <summary>
///     Captcha ServiceCollection Extensions
/// </summary>
public static class CaptchaServiceCollectionExtensions
{
    /// <summary>
    ///     Adds default DNTCaptcha providers.
    /// </summary>
    public static void AddDNTCaptcha(this IServiceCollection services, Action<DNTCaptchaOptions>? options = null)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        var configuredOptions = ConfigOptions(services, options);

        services.AddControllers(opts =>
        {
            opts.Conventions.Add(new ControllerRoutingConvention(typeof(DNTCaptchaImageController),
                configuredOptions.CaptchaImageControllerRouteTemplate,
                configuredOptions.CaptchaImageControllerNameTemplate));
        });

        services.AddMemoryCache();
        services.AddHttpContextAccessor();
        services.AddAntiforgery();
        services.AddMvcCore().AddCookieTempDataProvider();

#if NET7_0 || NET8_0 || NET9_0

        //  Also we need to have app.UseRateLimiter() after app.UseRouting()
        services.AddRateLimiter(limiterOptions
            => limiterOptions.AddPolicy<string, DNTCaptchaRateLimiterPolicy>(DNTCaptchaRateLimiterPolicy.Name));
#endif

        services.TryAddSingleton<HumanReadableIntegerProvider>();
        services.TryAddSingleton<ShowDigitsProvider>();
        services.TryAddSingleton<SumOfTwoNumbersProvider>();
        services.TryAddSingleton<SumOfTwoNumbersToWordsProvider>();

        services.TryAddSingleton<Func<DisplayMode, ICaptchaTextProvider>>(serviceProvider
            => key => GetCaptchaTextProvider(key, serviceProvider));

        services.TryAddSingleton<IRandomNumberProvider, RandomNumberProvider>();
        services.TryAddSingleton<ICaptchaImageProvider, CaptchaImageProvider>();
        services.TryAddSingleton<ICaptchaCryptoProvider, CaptchaCryptoProvider>();
        services.TryAddTransient<DNTCaptchaTagHelper>();
        services.TryAddTransient<IDNTCaptchaValidatorService, DNTCaptchaValidatorService>();
        services.TryAddScoped<IDNTCaptchaApiProvider, DNTCaptchaApiProvider>();
        services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

        services.TryAddScoped<IUrlHelper>(serviceProvider =>
        {
            var actionContext = serviceProvider.GetRequiredService<IActionContextAccessor>().ActionContext ??
                                throw new InvalidOperationException(message: "actionContext is null");

            var factory = serviceProvider.GetRequiredService<IUrlHelperFactory>();

            return factory.GetUrlHelper(actionContext);
        });
    }

    private static ICaptchaTextProvider GetCaptchaTextProvider(DisplayMode key, IServiceProvider serviceProvider)
        => key switch
        {
            DisplayMode.NumberToWord => serviceProvider.GetRequiredService<HumanReadableIntegerProvider>(),
            DisplayMode.ShowDigits => serviceProvider.GetRequiredService<ShowDigitsProvider>(),
            DisplayMode.SumOfTwoNumbers => serviceProvider.GetRequiredService<SumOfTwoNumbersProvider>(),
            DisplayMode.SumOfTwoNumbersToWords => serviceProvider.GetRequiredService<SumOfTwoNumbersToWordsProvider>(),
            _ => throw new InvalidOperationException($"Service of type {key} is not implemented.")
        };

    private static DNTCaptchaOptions ConfigOptions(IServiceCollection services, Action<DNTCaptchaOptions>? options)
    {
        var captchaOptions = new DNTCaptchaOptions();
        options?.Invoke(captchaOptions);
        SetCaptchaStorageProvider(services, captchaOptions);
        SetSerializationProvider(services, captchaOptions);
        services.TryAddSingleton(Options.Create(captchaOptions));

        return captchaOptions;
    }

    private static void SetSerializationProvider(IServiceCollection services, DNTCaptchaOptions captchaOptions)
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

    private static void SetCaptchaStorageProvider(IServiceCollection services, DNTCaptchaOptions captchaOptions)
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