using System.Buffers.Binary;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
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
var persian = new PersianCaptchaAudioProvider();
var recordings = new (ICaptchaAudioProvider Provider, Language Language, CaptchaAudioVoice Voice)[]
{
    (provider, Language.English, CaptchaAudioVoice.Male),
    (provider, Language.English, CaptchaAudioVoice.Female),
    (persian, Language.Persian, CaptchaAudioVoice.Male),
    (persian, Language.Persian, CaptchaAudioVoice.Female)
};
foreach (var recording in recordings)
{
    var voice = $"{recording.Language}/{recording.Voice}";
    var result = recording.Provider.CreateAudio("0123456789", recording.Language, recording.Voice);
    Check(Encoding.ASCII.GetString(result, 0, 4) == "RIFF", $"{voice}: RIFF header");
    Check(BinaryPrimitives.ReadInt32LittleEndian(result.AsSpan(4)) == result.Length - 8, $"{voice}: RIFF length");
    Check(BinaryPrimitives.ReadInt32LittleEndian(result.AsSpan(40)) == result.Length - 44, $"{voice}: data length");
    Check(BinaryPrimitives.ReadInt32LittleEndian(result.AsSpan(24)) == 24000, $"{voice}: sample rate");
    var offset = 44;
    foreach (var digit in "0123456789")
    {
        var single = recording.Provider.CreateAudio(digit.ToString(), recording.Language, recording.Voice).AsSpan(44);
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
Check(typeof(PersianCaptchaAudioProvider).Assembly.GetManifestResourceNames().Count(name => name.EndsWith(".wav", StringComparison.OrdinalIgnoreCase)) == 10, "Persian assembly embeds exactly ten recordings");
Check(persian.Supports(Language.Persian, CaptchaAudioVoice.Female), "Persian Female is supported");
Check(persian.Supports(Language.Persian, CaptchaAudioVoice.Male), "Persian Male supports Female fallback");
Check(!persian.Supports(Language.English, CaptchaAudioVoice.Female), "Persian provider does not claim English");
Check(!persian.Supports(Language.Persian, (CaptchaAudioVoice)99), "unknown Persian voice is unsupported");
foreach (var digits in new[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0123456789", "990001" })
{
    Check(persian.CreateAudio(digits, Language.Persian, CaptchaAudioVoice.Male).SequenceEqual(
        persian.CreateAudio(digits, Language.Persian, CaptchaAudioVoice.Female)), "Persian Male plays identical Female recordings: " + digits);
}
Throws<ArgumentException>(() => persian.CreateAudio("1", Language.English, CaptchaAudioVoice.Female), "reject foreign language in Persian provider");
Throws<ArgumentException>(() => persian.CreateAudio("1", Language.Persian, (CaptchaAudioVoice)99), "reject unknown Persian voice");
foreach (var invalid in new[] { "", "12+3", "1,234", "-1", "۱۲", "12345678901" })
{
    Throws<ArgumentException>(() => provider.CreateAudio(invalid, Language.English, CaptchaAudioVoice.Male), $"reject invalid digits: {invalid}");
    Throws<ArgumentException>(() => persian.CreateAudio(invalid, Language.Persian, CaptchaAudioVoice.Female), $"reject invalid Persian digits: {invalid}");
    Throws<ArgumentException>(() => persian.CreateAudio(invalid, Language.Persian, CaptchaAudioVoice.Male), $"fallback rejects invalid Persian digits: {invalid}");
}
Throws<ArgumentException>(() => provider.CreateAudio("1", Language.Persian, CaptchaAudioVoice.Male), "reject unsupported language");
Throws<ArgumentException>(() => provider.CreateAudio("1", Language.English, (CaptchaAudioVoice)99), "reject unsupported voice");

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.WebHost.UseUrls("http://127.0.0.1:0");
builder.Services.AddDNTCaptcha(options => options.UseMemoryCacheStorageProvider()
    .WithEncryptionKey("backend-regression-test-key").RateLimiterPermitLimit(120));
builder.Services.AddDNTCaptchaEnglishAudio();
builder.Services.AddDNTCaptchaPersianAudio();
builder.Services.AddSingleton<IDataProtectionProvider>(new EphemeralDataProtectionProvider());
builder.Services.AddControllersWithViews().AddApplicationPart(typeof(BackendTestController).Assembly);
await using var app = builder.Build();
app.UseDeveloperExceptionPage();
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

foreach (var recording in recordings)
{
    var captcha = await Create("AudioDigits", recording.Voice, recording.Language);
    Check(captcha.DntCaptchaImgUrl.Length == 0 && captcha.DntCaptchaAudioUrl is not null, "audio mode has no image answer");
    Check(captcha.DntCaptchaAudioUrl!.StartsWith("/example/DNTCaptchaImage/Audio?", StringComparison.Ordinal), "relative audio URL respects PathBase");
    using var first = await client.GetAsync(captcha.DntCaptchaAudioUrl);
    using var replay = await client.GetAsync(captcha.DntCaptchaAudioUrl);
    Check(first.IsSuccessStatusCode && replay.IsSuccessStatusCode, "audio can be fetched repeatedly");
    Check(first.Content.Headers.ContentType?.MediaType == "audio/wav", "audio content type");
    Check(first.Headers.CacheControl?.NoStore == true, "successful audio is no-store");
    Check((await first.Content.ReadAsByteArrayAsync()).SequenceEqual(recording.Provider.CreateAudio("2107", recording.Language, recording.Voice)), "URL plays the same answer using the selected voice");

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

DNTCaptchaApiResponse? fresh = null;
foreach (var recording in recordings)
{
    var old = await Create("AudioDigits", recording.Voice, recording.Language);
    fresh = await client.GetFromJsonAsync<DNTCaptchaApiResponse>($"test/refresh?language={recording.Language}&voice={recording.Voice}&data=" + Uri.EscapeDataString(Data(old)));
    Check(fresh?.DntCaptchaAudioUrl != old.DntCaptchaAudioUrl, "refresh replaces the protected URL even for the same digits");
    using (var oldAudio = await client.GetAsync(old.DntCaptchaAudioUrl))
    {
        Check(oldAudio.StatusCode == HttpStatusCode.BadRequest, "refresh invalidates old audio token");
    }
    using (var freshAudio = await client.GetAsync(fresh!.DntCaptchaAudioUrl))
    {
        Check(freshAudio.IsSuccessStatusCode, "refreshed challenge can be played");
        Check((await freshAudio.Content.ReadAsByteArrayAsync()).SequenceEqual(
            recording.Provider.CreateAudio("2107", recording.Language, recording.Voice)), "refresh preserves language and voice");
    }
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
        DisplayMode = DisplayMode.AudioDigits, Language = Language.Persian, AudioVoice = (CaptchaAudioVoice)99
    }), "unknown Persian voice fails through the API");
    Throws<InvalidOperationException>(() => api.CreateDNTCaptcha(new DNTCaptchaTagHelperHtmlAttributes
    {
        DisplayMode = DisplayMode.AudioDigits, Language = Language.English, AudioVoice = (CaptchaAudioVoice)99
    }), "unsupported voice fails through the API");
    Throws<ArgumentOutOfRangeException>(() => api.CreateDNTCaptcha(new DNTCaptchaTagHelperHtmlAttributes
    {
        DisplayMode = DisplayMode.AudioDigits, Language = Language.English, Min = -1
    }), "negative audio range rejected");
}

Console.WriteLine("Checking Razor TagHelper audio and refresh over HTTP...");
foreach (var recording in recordings)
{
    var html = await GetHtml($"test/razor/AudioDigits/{recording.Voice}?language={recording.Language}");
    Check(html.Contains("<audio", StringComparison.Ordinal) && !html.Contains("<img", StringComparison.Ordinal), "Razor renders audio without an image of the answer");
    Check(html.Contains("controls=", StringComparison.Ordinal) && html.Contains("preload=\"none\"", StringComparison.Ordinal) &&
          !html.Contains("autoplay", StringComparison.Ordinal), "Razor audio uses native controls without preloading or autoplay");
    Check(html.Contains("aria-label=\"Listen &amp; enter the digits\"", StringComparison.Ordinal), "audio has an encoded localized accessible label");
    var audioUrl = ElementAttribute(html, "audio", "src");
    Check(audioUrl.StartsWith("/example/DNTCaptchaImage/Audio?", StringComparison.Ordinal), "Razor audio respects PathBase");
    using var first = await client.GetAsync(audioUrl);
    using var replay = await client.GetAsync(audioUrl);
    Check(first.IsSuccessStatusCode && replay.IsSuccessStatusCode, "Razor audio supports repeated playback");
    Check((await first.Content.ReadAsByteArrayAsync()).SequenceEqual(
        recording.Provider.CreateAudio("2107", recording.Language, recording.Voice)), "Razor selects the requested language and voice");

    var refreshUrl = ElementAttribute(html, "a", "data-ajax-url");
    using var refreshResponse = await client.PostAsync(refreshUrl, null);
    refreshResponse.EnsureSuccessStatusCode();
    var refreshedHtml = await refreshResponse.Content.ReadAsStringAsync();
    var newAudioUrl = ElementAttribute(refreshedHtml, "audio", "src");
    Check(newAudioUrl != audioUrl, "Razor refresh replaces the protected URL");
    Check(refreshedHtml.Contains("aria-label=\"Listen &amp; enter the digits\"", StringComparison.Ordinal) &&
          refreshedHtml.Contains("dir=\"rtl\"", StringComparison.Ordinal), "Razor refresh preserves localized label and direction");
    using var oldAudio = await client.GetAsync(audioUrl);
    Check(oldAudio.StatusCode == HttpStatusCode.BadRequest, "Razor refresh invalidates the old audio");
    using var refreshedAudio = await client.GetAsync(newAudioUrl);
    Check(refreshedAudio.IsSuccessStatusCode && (await refreshedAudio.Content.ReadAsByteArrayAsync()).SequenceEqual(
        recording.Provider.CreateAudio("2107", recording.Language, recording.Voice)), "Razor refresh preserves language and voice");

    var hiddenInputs = Regex.Matches(refreshedHtml, "<input\\b[^>]*>").Select(match => match.Value).ToArray();
    using var form = new FormUrlEncodedContent(new Dictionary<string, string>
    {
        ["DNTCaptchaText"] = ElementAttribute(hiddenInputs.Single(input => input.Contains("name=\"DNTCaptchaText\"", StringComparison.Ordinal)), "input", "value"),
        ["DNTCaptchaToken"] = ElementAttribute(hiddenInputs.Single(input => input.Contains("name=\"DNTCaptchaToken\"", StringComparison.Ordinal)), "input", "value"),
        ["DNTCaptchaInputText"] = "2107"
    });
    using var validation = await client.PostAsync("test/validate", form);
    Check(await validation.Content.ReadFromJsonAsync<bool>(), "Razor hidden fields validate after playback and refresh");
    using var consumed = await client.GetAsync(newAudioUrl);
    Check(consumed.StatusCode == HttpStatusCode.BadRequest, "validated Razor audio cannot be replayed");
}
var absoluteHtml = await GetHtml("test/razor/AudioDigits/Female?relative=false&refresh=false");
Check(ElementAttribute(absoluteHtml, "audio", "src").StartsWith(address + "/example/", StringComparison.Ordinal), "Razor supports absolute audio URLs");
Check(!absoluteHtml.Contains("data-ajax-url", StringComparison.Ordinal), "Razor audio works without a refresh button");
foreach (var mode in new[] { DisplayMode.ShowDigits, DisplayMode.NumberToWord, DisplayMode.SumOfTwoNumbers, DisplayMode.SumOfTwoNumbersToWords })
{
    var html = await GetHtml($"test/razor/{mode}/Male");
    Check(html.Contains("<img", StringComparison.Ordinal) && !html.Contains("<audio", StringComparison.Ordinal), $"{mode}: Razor retains visual rendering");
}

var limited = false;
for (var attempt = 0; attempt < 160; attempt++)
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

async Task<DNTCaptchaApiResponse> Create(string mode, CaptchaAudioVoice voice, Language language = Language.English)
    => await client.GetFromJsonAsync<DNTCaptchaApiResponse>($"test/create/{mode}/{voice}?language={language}") ?? throw new InvalidOperationException("Missing API response");

async Task<string> GetHtml(string url)
{
    using var response = await client.GetAsync(url);
    var html = await response.Content.ReadAsStringAsync();
    Check(response.IsSuccessStatusCode, $"Razor response {response.StatusCode}: {html}");
    return html;
}

static string Data(DNTCaptchaApiResponse response)
    => QueryHelpers.ParseQuery(new Uri("http://localhost" + response.DntCaptchaAudioUrl).Query)["data"].ToString();

static string ElementAttribute(string html, string element, string attribute)
{
    var match = Regex.Match(html, $"<{element}\\b[^>]*\\s{attribute}=\"([^\"]*)\"");
    return match.Success ? WebUtility.HtmlDecode(match.Groups[1].Value) : throw new InvalidOperationException($"Missing {element}[{attribute}] in: {html}");
}

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
