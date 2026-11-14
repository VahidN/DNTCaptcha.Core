# DNTCaptcha.Core.Audio.Persian

Optional Persian (Iran) female recordings for `DisplayMode.AudioDigits`.
This project owns and embeds ten WAV clips in `Resources/Female/`, named `0.wav`
through `9.wav`. It targets .NET 6 and works
with every framework supported by the core package. No model, TTS engine,
network request, or runtime download is required.

Reference this project (or install its NuGet package once published), then register:

```csharp
services.AddDNTCaptcha(options => options.WithEncryptionKey("your application key"));
services.AddDNTCaptchaPersianAudio();
// Optional: register English alongside Persian.
services.AddDNTCaptchaEnglishAudio();
```

Create a challenge on the server through `IDNTCaptchaApiProvider`:

```csharp
var result = apiProvider.CreateDNTCaptcha(new DNTCaptchaTagHelperHtmlAttributes
{
    DisplayMode = DisplayMode.AudioDigits,
    Language = Language.Persian,
    AudioVoice = CaptchaAudioVoice.Female,
    Min = 1000,
    Max = 9999,
    UseRelativeUrls = true
});
```

Persian has Female recordings only. `AudioVoice = CaptchaAudioVoice.Male`
(including the enum default) always falls back to those same Female recordings.
Unknown voices and other languages are rejected.

The provider accepts one to ten ASCII digits, pronounces them in Persian in
the supplied order, and inserts 150 ms of silence between clips. Output is mono,
24 kHz, 16-bit PCM WAV. The core assembly contains no recordings.

Use the protected `DntCaptchaAudioUrl` and existing hidden form values exactly
as in the [English audio integration](../DNTCaptcha.Core.Audio.English/README.md).
Its expiration, browser binding, refresh, validation, rate limiting, and shared
Data Protection key-ring requirements apply to both languages. The API and
Angular sample now selects Persian Female; Razor playback is not implemented.

See `NOTICE.md` for recording provenance and upstream attribution.
