using DNTCaptcha.Core;
using Microsoft.AspNetCore.Mvc;

namespace DNTCaptcha.BackendTests;

[ApiController]
[Route("test")]
public sealed class BackendTestController(IDNTCaptchaApiProvider api, IDNTCaptchaValidatorService validator,
    CaptchaAudioService audioService) : Controller
{
    [HttpGet("razor/{mode}/{voice}")]
    public IActionResult Razor(DisplayMode mode, CaptchaAudioVoice voice, Language language = Language.English,
        bool relative = true, bool refresh = true)
    {
        var attributes = Attributes(mode, voice, language);
        attributes.UseRelativeUrls = relative;
        attributes.ShowRefreshButton = refresh;
        return View("Captcha", attributes);
    }

    [HttpGet("create/{mode}/{voice}")]
    public DNTCaptchaApiResponse Create(DisplayMode mode, CaptchaAudioVoice voice, Language language = Language.English) => api.CreateDNTCaptcha(Attributes(mode, voice, language));

    [HttpGet("refresh")]
    public DNTCaptchaApiResponse Refresh(string data, Language language = Language.English, CaptchaAudioVoice voice = CaptchaAudioVoice.Male)
    {
        audioService.Invalidate(HttpContext, data);
        return api.CreateDNTCaptcha(Attributes(DisplayMode.AudioDigits, voice, language));
    }

    [HttpPost("validate")]
    public bool Validate() => validator.HasRequestValidCaptchaEntry();

    private static DNTCaptchaTagHelperHtmlAttributes Attributes(DisplayMode mode, CaptchaAudioVoice voice, Language language) => new()
    {
        DisplayMode = mode, Language = language, AudioVoice = voice,
        Min = 2107, Max = 2107, UseRelativeUrls = true
    };
}
