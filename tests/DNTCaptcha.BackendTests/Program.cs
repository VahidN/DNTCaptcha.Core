using System.Buffers.Binary;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using DNTCaptcha.Core;
using DNTCaptcha.BackendTests;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

// A dependency-free executable regression suite: any failed check exits nonzero.
var checks = 0;
Console.WriteLine("Checking WAV recordings and invalid inputs...");
var provider = new EnglishCaptchaAudioProvider();
foreach (var voice in Enum.GetValues<CaptchaAudioVoice>())
{
    var result = provider.CreateAudio("0123456789", Language.English, voice);
    Check(Encoding.ASCII.GetString(result, 0, 4) == "RIFF", $"{voice}: RIFF header");
    Check(BinaryPrimitives.ReadInt32LittleEndian(result.AsSpan(4)) == result.Length - 8, $"{voice}: RIFF length");
    Check(BinaryPrimitives.ReadInt32LittleEndian(result.AsSpan(40)) == result.Length - 44, $"{voice}: data length");
    Check(BinaryPrimitives.ReadInt32LittleEndian(result.AsSpan(24)) == 24000, $"{voice}: sample rate");
    var offset = 44;
    foreach (var digit in "0123456789")
    {
        var single = provider.CreateAudio(digit.ToString(), Language.English, voice).AsSpan(44);
        Check(result.AsSpan(offset, single.Length).SequenceEqual(single), $"{voice}: digit {digit} order and PCM");
        offset += single.Length;
        if (digit != '9')
        {
            const int pauseLength = 24000 * 2 * 150 / 1000;
            Check(result.AsSpan(offset, pauseLength).IndexOfAnyExcept((byte)0) < 0, $"{voice}: silence after {digit}");
            offset += pauseLength;
        }
    }
    Check(offset == result.Length, $"{voice}: complete assembly without extra bytes");
}
Check(!typeof(DNTCaptchaOptions).Assembly.GetManifestResourceNames().Any(name => name.EndsWith(".wav", StringComparison.OrdinalIgnoreCase)), "core does not embed recordings");
Check(typeof(EnglishCaptchaAudioProvider).Assembly.GetManifestResourceNames().Count(name => name.EndsWith(".wav", StringComparison.OrdinalIgnoreCase)) == 20, "optional assembly embeds exactly 20 recordings");
foreach (var invalid in new[] { "", "12+3", "1,234", "-1", "۱۲", "12345678901" })
{
    Throws<ArgumentException>(() => provider.CreateAudio(invalid, Language.English, CaptchaAudioVoice.Male), $"reject invalid digits: {invalid}");
}
Throws<ArgumentException>(() => provider.CreateAudio("1", Language.Persian, CaptchaAudioVoice.Male), "reject unsupported language");
Throws<ArgumentException>(() => provider.CreateAudio("1", Language.English, (CaptchaAudioVoice)99), "reject unsupported voice");

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.WebHost.UseUrls("http://127.0.0.1:0");
builder.Services.AddDNTCaptcha(options => options.UseMemoryCacheStorageProvider()
    .WithEncryptionKey("backend-regression-test-key").RateLimiterPermitLimit(30));
builder.Services.AddDNTCaptchaEnglishAudio();
builder.Services.AddSingleton<IDataProtectionProvider>(new EphemeralDataProtectionProvider());
builder.Services.AddControllers().AddApplicationPart(typeof(BackendTestController).Assembly);
await using var app = builder.Build();
app.UsePathBase("/example");
app.UseRouting();
app.UseRateLimiter();
app.MapControllers();
Console.WriteLine("Starting HTTP regression server...");
await app.StartAsync();
Console.WriteLine("Checking protected audio playback over HTTP...");

var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
using var client = new HttpClient();
client.BaseAddress = new Uri(address + "/example/");
client.Timeout = TimeSpan.FromSeconds(15);
client.DefaultRequestHeaders.UserAgent.ParseAdd("DNTCaptcha-BackendTests/1.0");

foreach (var voice in Enum.GetValues<CaptchaAudioVoice>())
{
    var captcha = await Create("AudioDigits", voice);
    Check(captcha.DntCaptchaImgUrl.Length == 0 && captcha.DntCaptchaAudioUrl is not null, "audio mode has no image answer");
    Check(captcha.DntCaptchaAudioUrl!.StartsWith("/example/DNTCaptchaImage/Audio?", StringComparison.Ordinal), "relative audio URL respects PathBase");
    using var first = await client.GetAsync(captcha.DntCaptchaAudioUrl);
    using var replay = await client.GetAsync(captcha.DntCaptchaAudioUrl);
    Check(first.IsSuccessStatusCode && replay.IsSuccessStatusCode, "audio can be fetched repeatedly");
    Check(first.Content.Headers.ContentType?.MediaType == "audio/wav", "audio content type");
    Check(first.Headers.CacheControl?.NoStore == true, "successful audio is no-store");
    Check((await first.Content.ReadAsByteArrayAsync()).SequenceEqual(provider.CreateAudio("2107", Language.English, voice)), "URL plays the same answer using the selected voice");

    client.DefaultRequestHeaders.UserAgent.Clear();
    client.DefaultRequestHeaders.UserAgent.ParseAdd("Different-Browser/1.0");
    using var foreignResult = await client.GetAsync(captcha.DntCaptchaAudioUrl);
    Check(foreignResult.StatusCode == HttpStatusCode.BadRequest, "reject mismatched browser binding");
    client.DefaultRequestHeaders.UserAgent.Clear();
    client.DefaultRequestHeaders.UserAgent.ParseAdd("DNTCaptcha-BackendTests/1.0");

    var bytes = WebEncoders.Base64UrlDecode(Data(captcha));
    bytes[bytes.Length / 2] ^= 1;
    using var tampered = await client.GetAsync("/example/DNTCaptchaImage/Audio?data=" + WebEncoders.Base64UrlEncode(bytes));
    Check(tampered.StatusCode == HttpStatusCode.BadRequest && tampered.Headers.CacheControl?.NoStore == true, "reject tampered token with no-store");

    using var form = new FormUrlEncodedContent(new Dictionary<string, string>
    {
        ["DNTCaptchaText"] = captcha.DntCaptchaTextValue,
        ["DNTCaptchaToken"] = captcha.DntCaptchaTokenValue,
        ["DNTCaptchaInputText"] = "2107"
    });
    using var validation = await client.PostAsync("test/validate", form);
    Check(await validation.Content.ReadFromJsonAsync<bool>(), "repeated playback leaves memory-cache answer valid");
    using var consumed = await client.GetAsync(captcha.DntCaptchaAudioUrl);
    Check(consumed.StatusCode == HttpStatusCode.BadRequest, "consumed challenge can no longer be played");
}

var old = await Create("AudioDigits", CaptchaAudioVoice.Male);
var fresh = await client.GetFromJsonAsync<DNTCaptchaApiResponse>("test/refresh?data=" + Uri.EscapeDataString(Data(old)));
Check(fresh?.DntCaptchaAudioUrl != old.DntCaptchaAudioUrl, "refresh replaces the protected URL even for the same digits");
using (var oldAudio = await client.GetAsync(old.DntCaptchaAudioUrl))
{
    Check(oldAudio.StatusCode == HttpStatusCode.BadRequest, "refresh invalidates old audio token");
}
using (var freshAudio = await client.GetAsync(fresh!.DntCaptchaAudioUrl))
{
    Check(freshAudio.IsSuccessStatusCode, "refreshed challenge can be played");
}

var protector = app.Services.GetRequiredService<IDataProtectionProvider>()
    .CreateProtector("DNTCaptcha.Core.AudioDigits.v1").ToTimeLimitedDataProtector();
var expired = protector.Protect("{}", DateTimeOffset.UtcNow.AddSeconds(-1));
using (var expiredAudio = await client.GetAsync("/example/DNTCaptchaImage/Audio?data=" + Uri.EscapeDataString(expired)))
{
    Check(expiredAudio.StatusCode == HttpStatusCode.BadRequest, "expired signed payload rejected");
}
foreach (var data in new[] { "", "not-base64", "AAAA" })
{
    using var malformed = await client.GetAsync("/example/DNTCaptchaImage/Audio?data=" + data);
    Check(malformed.StatusCode == HttpStatusCode.BadRequest && malformed.Headers.CacheControl?.NoStore == true, "malformed or missing payload rejected without caching");
}

foreach (var mode in new[] { DisplayMode.ShowDigits, DisplayMode.NumberToWord, DisplayMode.SumOfTwoNumbers, DisplayMode.SumOfTwoNumbersToWords })
{
    var visual = await Create(mode.ToString(), CaptchaAudioVoice.Male);
    Check(visual.DntCaptchaImgUrl.Contains("/Show?", StringComparison.Ordinal) && visual.DntCaptchaAudioUrl is null, $"{mode}: existing visual contract preserved");
}

using (var scope = app.Services.CreateScope())
{
    var accessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
    accessor.HttpContext = new DefaultHttpContext();
    var api = scope.ServiceProvider.GetRequiredService<IDNTCaptchaApiProvider>();
    Throws<InvalidOperationException>(() => api.CreateDNTCaptcha(new DNTCaptchaTagHelperHtmlAttributes
    {
        DisplayMode = DisplayMode.AudioDigits, Language = Language.Persian
    }), "unsupported language fails clearly");
    Throws<ArgumentOutOfRangeException>(() => api.CreateDNTCaptcha(new DNTCaptchaTagHelperHtmlAttributes
    {
        DisplayMode = DisplayMode.AudioDigits, Language = Language.English, Min = -1
    }), "negative audio range rejected");
}

var limited = false;
for (var attempt = 0; attempt < 40; attempt++)
{
    using var response = await client.GetAsync(fresh!.DntCaptchaAudioUrl);
    if (response.StatusCode == HttpStatusCode.TooManyRequests)
    {
        Check(response.Headers.CacheControl?.NoStore == true, "rate-limit rejection is no-store");
        limited = true;
        break;
    }
}
Check(limited, "audio action uses the CAPTCHA rate limiter");
await app.StopAsync();
Console.WriteLine($"PASS: {checks} backend checks (WAV assembly, HTTP playback, tokens, refresh, validation, and rate limiting).");

async Task<DNTCaptchaApiResponse> Create(string mode, CaptchaAudioVoice voice)
    => await client.GetFromJsonAsync<DNTCaptchaApiResponse>($"test/create/{mode}/{voice}") ?? throw new InvalidOperationException("Missing API response");

static string Data(DNTCaptchaApiResponse response)
    => QueryHelpers.ParseQuery(new Uri("http://localhost" + response.DntCaptchaAudioUrl).Query)["data"].ToString();

void Check(bool condition, string description)
{
    if (!condition) throw new InvalidOperationException("FAIL: " + description);
    checks++;
}

void Throws<T>(Action action, string description) where T : Exception
{
    try { action(); }
    catch (T) { checks++; return; }
    throw new InvalidOperationException("FAIL: " + description);
}
