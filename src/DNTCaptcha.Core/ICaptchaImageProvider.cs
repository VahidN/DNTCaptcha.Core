namespace DNTCaptcha.Core
{
    /// <summary>
    /// Captcha Image Provider
    /// </summary>
    public interface ICaptchaImageProvider
    {
        /// <summary>
        /// Creates the captcha image.
        /// </summary>
        byte[] DrawCaptcha(string message, string foreColor, string backColor, float fontSize, string fontName);

        /// <summary>
        /// Creates the captcha image.
        /// </summary>
        byte[] DrawCaptcha(string message, string foreColor, float fontSize, string fontName);
    }
}