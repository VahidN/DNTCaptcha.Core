using System;

namespace DNTCaptcha.Core.Contracts
{
    /// <summary>
    /// Refresh Data
    /// </summary>
    [Serializable]
    public class RefreshData : DNTCaptchaTagHelperHtmlAttributes
    {
        /// <summary>
        /// Current Date
        /// </summary>
        public long RndDate { get; set; }
    }
}