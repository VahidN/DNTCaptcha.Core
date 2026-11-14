using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DNTCaptcha.Core;

/// <summary>
///     Registers the optional English digit recordings.
/// </summary>
public static class EnglishAudioServiceCollectionExtensions
{
    /// <summary>
    ///     Adds English Male and Female voices for DisplayMode.AudioDigits.
    /// </summary>
    public static IServiceCollection AddDNTCaptchaEnglishAudio(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ICaptchaAudioProvider, EnglishCaptchaAudioProvider>());

        return services;
    }
}
