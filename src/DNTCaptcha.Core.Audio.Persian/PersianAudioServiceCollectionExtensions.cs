using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DNTCaptcha.Core;

/// <summary>
///     Registers the optional Persian digit recordings.
/// </summary>
public static class PersianAudioServiceCollectionExtensions
{
    /// <summary>
    ///     Adds Persian Female recordings for DisplayMode.AudioDigits, also used for Male requests.
    /// </summary>
    public static IServiceCollection AddDNTCaptchaPersianAudio(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ICaptchaAudioProvider, PersianCaptchaAudioProvider>());

        return services;
    }
}
