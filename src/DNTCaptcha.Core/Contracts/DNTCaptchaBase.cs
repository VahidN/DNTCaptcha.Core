namespace DNTCaptcha.Core.Contracts
{
    /// <summary>
    /// The base class of the DNTCaptcha Info
    /// </summary>
    public abstract class DNTCaptchaBase
    {
        /// <summary>
        /// Captcha's Text
        /// </summary>
        /// <returns></returns>
        public string DNTCaptchaText { set; get; } = default!;

        /// <summary>
        /// Captcha's Token
        /// </summary>
        /// <returns></returns>
        public string DNTCaptchaToken { set; get; } = default!;

        /// <summary>
        /// Captcha's InputText
        /// </summary>
        /// <returns></returns>
        public string DNTCaptchaInputText { set; get; } = default!;
    }
}