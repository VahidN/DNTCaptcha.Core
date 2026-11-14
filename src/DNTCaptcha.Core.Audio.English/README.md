# DNTCaptcha.Core.Audio.English

Optional English recordings for the backend `DisplayMode.AudioDigits` pilot.
The core package does not embed these WAV files. No TTS engine, external service,
or runtime download is required. The recording assembly targets .NET 6 and is
compatible with all framework versions supported by the core package, avoiding
duplicate copies of the same recordings for each target framework.

Reference this project (or install its NuGet package once published), then register:

```csharp
services.AddDNTCaptcha(options => options.WithEncryptionKey("your application key"));
services.AddDNTCaptchaEnglishAudio();
```

Create a challenge on the server through `IDNTCaptchaApiProvider`:

```csharp
var result = apiProvider.CreateDNTCaptcha(new DNTCaptchaTagHelperHtmlAttributes
{
    DisplayMode = DisplayMode.AudioDigits,
    Language = Language.English,
    AudioVoice = CaptchaAudioVoice.Female, // Or Male.
    Min = 1000,
    Max = 9999,
    UseRelativeUrls = true
});
```

The result includes `DntCaptchaAudioUrl`, with an empty `DntCaptchaImgUrl`.
Keep the existing hidden text/token values when submitting the answer. Playing
the audio does not consume those values. The answer is still validated by the
existing validator. Audio URLs expire after `AbsoluteExpirationMinutes` and
are bound to their original challenge and browser binding.

On .NET 7+, place `app.UseRateLimiter()` after `app.UseRouting()` to enable the
existing CAPTCHA rate-limit policy on the audio action. On .NET 6, configure
rate limiting in the host or reverse proxy. Keep `no-store` on the application
action that creates CAPTCHA parameters, as in the included API sample.

In a distributed deployment, configure a shared, persistent ASP.NET Core Data
Protection key ring and the same application name on all instances, alongside
the existing shared CAPTCHA storage configuration. Otherwise audio tokens
issued on one instance cannot be opened by another.

For refresh, the API sample accepts the previous audio URL's protected `data`
value as `previousAudioData`. It invalidates the old challenge before issuing a
new one. This is not an answer-storage read and does not regenerate a question.

The Angular example provides listening, replay, stop, refresh, and inline error
handling. Razor playback controls will be added separately; the Razor TagHelper currently rejects
`AudioDigits` rather than displaying the spoken answer as an image.

This provider supports English only. For Persian Female, also register the optional
[DNTCaptcha.Core.Audio.Persian](../DNTCaptcha.Core.Audio.Persian/README.md) package.
Other languages and invalid voices fail explicitly. Audio digits ignore the visual thousands-separator setting and
require a non-negative integer range. The recordings are concatenated with
150 ms of silence between digits. No background noise is added.

See `NOTICE.md` for recording provenance and upstream attribution.
