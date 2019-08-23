using DNTCaptcha.Core.Providers;

namespace DNTCaptcha.Core.Contracts
{
    /// <summary>
    /// Convert a number into text
    /// </summary>
    public interface ICaptchaTextProvider
    {
        /// <summary>
        /// display a numeric value using the equivalent text
        /// </summary>
        /// <param name="number">input number</param>
        /// <param name="language">local language</param>
        /// <param name="displayMode">algorithm</param>
        /// <returns>the equivalent text</returns>
        string GetText(long number, Language language, DisplayMode displayMode);
    }
}