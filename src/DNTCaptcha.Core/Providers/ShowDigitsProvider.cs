using System.Globalization;
using DNTCaptcha.Core.Contracts;
using Microsoft.Extensions.Options;

namespace DNTCaptcha.Core.Providers
{
    /// <summary>
    /// display a numeric value using the equivalent text
    /// </summary>
    public class ShowDigitsProvider : ICaptchaTextProvider
    {
        private readonly DNTCaptchaOptions _captchaOptions;

        /// <summary>
        /// display a numeric value using the equivalent text
        /// </summary>
        public ShowDigitsProvider(IOptions<DNTCaptchaOptions> options)
        {
            _captchaOptions = options.Value;
        }

        /// <summary>
        /// display a numeric value using the equivalent text
        /// </summary>
        /// <param name="number">input number</param>
        /// <param name="language">local language</param>
        /// <returns>the equivalent text</returns>
        public string GetText(long number, Language language)
        {
            var text = _captchaOptions.AllowThousandsSeparators ?
                            string.Format(CultureInfo.InvariantCulture, "{0:N0}", number) :
                            number.ToString(CultureInfo.InvariantCulture);
            return language == Language.Persian ? text.ToPersianNumbers() : text;
        }
    }
}