namespace DNTCaptcha.Core
{
    /// <summary>
    /// DNTCaptcha Api
    /// </summary>
    public interface IDNTCaptchaApiProvider
    {
        /// <summary>
        /// Creates DNTCaptcha
        /// </summary>
        /// <param name="captchaAttributes">captcha attributes</param>
        DNTCaptchaApiResponse CreateDNTCaptcha(DNTCaptchaTagHelperHtmlAttributes captchaAttributes);
    }
}