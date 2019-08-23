using System.Globalization;
using DNTCaptcha.Core.Contracts;

namespace DNTCaptcha.Core.Providers
{
    /// <summary>
    /// display a numeric value using the equivalent text
    /// </summary>
    public class ShowDigitsProvider : IShowDigitsProvider
    {
        /// <summary>
        /// display a numeric value using the equivalent text
        /// </summary>
        /// <param name="number">input number</param>
        /// <param name="language">local language</param>
        /// <returns>the equivalent text</returns>
        public string GetText(long number, Language language)
        {
            var text = string.Format(CultureInfo.InvariantCulture, "{0:N0}", number);
            return language == Language.Persian ? text.ToPersianNumbers() : text;
        }
    }
}