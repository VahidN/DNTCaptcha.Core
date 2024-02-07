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