namespace DNTCaptcha.Core
{
    /// <summary>
    /// Defines DNTCaptcha's input names
    /// </summary>
    public class DNTCaptchaComponent
    {
        /// <summary>
        /// The default hidden input name of the captcha.
        /// Its default value is `DNTCaptchaText`.
        /// </summary>
        public string CaptchaHiddenInputName { get; set; } = "DNTCaptchaText";

        /// <summary>
        /// The default hidden input name of the captcha's cookie token.
        /// Its default value is `DNTCaptchaToken`.
        /// </summary>
        public string CaptchaHiddenTokenName { get; set; } = "DNTCaptchaToken";

        /// <summary>
        /// The default input name of the captcha.
        /// Its default value is `DNTCaptchaInputText`.
        /// </summary>
        public string CaptchaInputName { get; set; } = "DNTCaptchaInputText";
    }
}