using DNTCaptcha.Core;
using Microsoft.AspNetCore.Mvc;

namespace DNTCaptcha.BackendTests;

[ApiController]
[Route("test")]
public sealed class BackendTestController(IDNTCaptchaApiProvider api, IDNTCaptchaValidatorService validator,
    CaptchaAudioService audioService) : ControllerBase
{
    [HttpGet("create/{mode}/{voice}")]
    public DNTCaptchaApiResponse Create(DisplayMode mode, CaptchaAudioVoice voice) => api.CreateDNTCaptcha(Attributes(mode, voice));

    [HttpGet("refresh")]
    public DNTCaptchaApiResponse Refresh(string data)
    {
        audioService.Invalidate(HttpContext, data);
        return api.CreateDNTCaptcha(Attributes(DisplayMode.AudioDigits, CaptchaAudioVoice.Male));
    }

    [HttpPost("validate")]
    public bool Validate() => validator.HasRequestValidCaptchaEntry();

    private static DNTCaptchaTagHelperHtmlAttributes Attributes(DisplayMode mode, CaptchaAudioVoice voice) => new()
    {
        DisplayMode = mode, Language = Language.English, AudioVoice = voice,
        Min = 2107, Max = 2107, UseRelativeUrls = true
    };
}
