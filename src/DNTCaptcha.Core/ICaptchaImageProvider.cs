namespace DNTCaptcha.Core
{
    /// <summary>
    ///     Captcha Image Provider
    /// </summary>
    public interface ICaptchaImageProvider
    {
        /// <summary>
        ///     Creates the captcha image.
        /// </summary>
        byte[] DrawCaptcha(string text, string foreColor, string backColor, float fontSize, string fontName);
    }
}