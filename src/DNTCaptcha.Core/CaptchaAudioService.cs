using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace DNTCaptcha.Core;

/// <summary>
///     Protects replayable audio payloads and checks their associated challenge without consuming its answer.
/// </summary>
public sealed class CaptchaAudioService(
    IDataProtectionProvider dataProtectionProvider,
    ICaptchaStorageProvider storageProvider,
    ICaptchaCryptoProvider cryptoProvider,
    IEnumerable<ICaptchaAudioProvider> audioProviders,
    IOptions<DNTCaptchaOptions> options)
{
    private readonly ITimeLimitedDataProtector _protector =
        (dataProtectionProvider ?? throw new ArgumentNullException(nameof(dataProtectionProvider)))
        .CreateProtector("DNTCaptcha.Core.AudioDigits.v1").ToTimeLimitedDataProtector();

    private readonly ICaptchaStorageProvider _storageProvider =
        storageProvider ?? throw new ArgumentNullException(nameof(storageProvider));

    private readonly ICaptchaCryptoProvider _cryptoProvider =
        cryptoProvider ?? throw new ArgumentNullException(nameof(cryptoProvider));

    private readonly ICaptchaAudioProvider[] _audioProviders =
        (audioProviders ?? throw new ArgumentNullException(nameof(audioProviders))).ToArray();

    private readonly DNTCaptchaOptions _options =
        options is null ? throw new ArgumentNullException(nameof(options)) : options.Value;

    /// <summary>
    ///     Creates an authenticated payload expiring with the challenge. Does not expose the digits to the client.
    /// </summary>
    public string Protect(HttpContext context, string digits, string captchaToken, Language language,
        CaptchaAudioVoice voice)
    {
        ArgumentNullException.ThrowIfNull(context);
        ValidateDigits(digits);

        if (string.IsNullOrWhiteSpace(captchaToken))
        {
            throw new ArgumentException("A challenge token is required.", nameof(captchaToken));
        }

        if (_options.AbsoluteExpirationMinutes <= 0)
        {
            throw new InvalidOperationException("Audio challenges require a positive expiration time.");
        }

        _ = GetProvider(language, voice);

        var payload = new AudioPayload
        {
            Digits = digits,
            CaptchaToken = captchaToken,
            Binding = context.GetSalt(_cryptoProvider),
            Language = language,
            Voice = voice
        };

        return _protector.Protect(JsonSerializer.Serialize(payload),
            TimeSpan.FromMinutes(_options.AbsoluteExpirationMinutes));
    }

    /// <summary>
    ///     Replays an active challenge. Returns null for invalid, expired, consumed, or browser-mismatched payloads.
    /// </summary>
    public byte[]? CreateAudio(HttpContext context, string data)
    {
        ArgumentNullException.ThrowIfNull(context);

        var payload = GetActivePayload(context, data);

        if (payload is null)
        {
            return null;
        }

        ValidateDigits(payload.Digits);

        return GetProvider(payload.Language, payload.Voice).CreateAudio(payload.Digits, payload.Language, payload.Voice);
    }

    /// <summary>
    ///     Invalidates an audio challenge during refresh, using its authenticated, browser-bound payload.
    /// </summary>
    public void Invalidate(HttpContext context, string data)
    {
        ArgumentNullException.ThrowIfNull(context);
        var payload = GetActivePayload(context, data);

        if (payload is not null)
        {
            _storageProvider.Remove(context, payload.CaptchaToken);
        }
    }

    private AudioPayload? GetActivePayload(HttpContext context, string data)
    {

        if (string.IsNullOrWhiteSpace(data))
        {
            return null;
        }

        AudioPayload? payload;

        try
        {
            payload = JsonSerializer.Deserialize<AudioPayload>(_protector.Unprotect(data));
        }
        catch (CryptographicException)
        {
            return null;
        }
        catch (FormatException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }

        if (payload is null || !string.Equals(payload.Binding, context.GetSalt(_cryptoProvider),
                StringComparison.Ordinal) || !_storageProvider.Contains(context, payload.CaptchaToken))
        {
            return null;
        }

        return payload;
    }

    private ICaptchaAudioProvider GetProvider(Language language, CaptchaAudioVoice voice)
        => _audioProviders.FirstOrDefault(provider => provider.Supports(language, voice)) ??
           throw new InvalidOperationException(
               $"No captcha audio provider supports {language}/{voice}. Install DNTCaptcha.Core.Audio.English and call services.AddDNTCaptchaEnglishAudio() for English recordings.");

    private static void ValidateDigits(string digits)
    {
        if (string.IsNullOrEmpty(digits) || digits.Length > 10 || digits.Any(digit => digit is < '0' or > '9'))
        {
            throw new ArgumentException("An audio challenge must contain between one and ten ASCII digits.",
                nameof(digits));
        }
    }

    private sealed class AudioPayload
    {
        public string Digits { get; set; } = "";
        public string CaptchaToken { get; set; } = "";
        public string Binding { get; set; } = "";
        public Language Language { get; set; }
        public CaptchaAudioVoice Voice { get; set; }
    }
}
