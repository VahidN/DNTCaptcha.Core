using System.Text.Json.Serialization;

namespace DNTCaptcha.Core;

/// <summary>
///     ApiProvider Response
/// </summary>
public class DNTCaptchaApiResponse
{
    /// <summary>
    ///     The captach's image url
    /// </summary>
    /// <value></value>
    public string DntCaptchaImgUrl { set; get; } = default!;

    /// <summary>
    ///     The protected audio URL for AudioDigits; otherwise null. Image URL is empty in AudioDigits mode.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DntCaptchaAudioUrl { set; get; }

    /// <summary>
    ///     Captcha Id
    /// </summary>
    public string DntCaptchaId { set; get; } = default!;

    /// <summary>
    ///     Captcha's TextValue
    /// </summary>
    public string DntCaptchaTextValue { set; get; } = default!;

    /// <summary>
    ///     Captcha's TokenValue
    /// </summary>
    public string DntCaptchaTokenValue { set; get; } = default!;
}
