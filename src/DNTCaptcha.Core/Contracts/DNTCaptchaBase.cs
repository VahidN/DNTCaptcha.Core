namespace DNTCaptcha.Core.Contracts
{
    /// <summary>
    ///
    /// </summary>
    public abstract class DNTCaptchaBase
    {
        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public string DNTCaptchaText { set; get; }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public string DNTCaptchaToken { set; get; }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public string DNTCaptchaInputText { set; get; }
    }
}