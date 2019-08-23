using System;
using DNTCaptcha.Core.Contracts;

namespace DNTCaptcha.Core.Providers
{
    /// <summary>
    /// Convert a number into text
    /// </summary>
    public class CaptchaTextProvider : ICaptchaTextProvider
    {
        private readonly IHumanReadableIntegerProvider _humanReadableIntegerProvider;
        private readonly IShowDigitsProvider _showDigitsProvider;
        private readonly ISumOfTwoNumbersProvider _sumOfTwoNumbersProvider;

        /// <summary>
        /// Convert a number into text
        /// </summary>
        public CaptchaTextProvider(
            IHumanReadableIntegerProvider humanReadableIntegerProvider,
            IShowDigitsProvider showDigitsProvider,
            ISumOfTwoNumbersProvider sumOfTwoNumbersProvider)
        {
            _humanReadableIntegerProvider = humanReadableIntegerProvider;
            _showDigitsProvider = showDigitsProvider;
            _sumOfTwoNumbersProvider = sumOfTwoNumbersProvider;
        }

        /// <summary>
        /// display a numeric value using the equivalent text
        /// </summary>
        /// <param name="number">input number</param>
        /// <param name="language">local language</param>
        /// <param name="displayMode">algorithm</param>
        /// <returns>the equivalent text</returns>
        public string GetText(long number, Language language, DisplayMode displayMode)
        {
            switch (displayMode)
            {
                case DisplayMode.NumberToWord:
                    return _humanReadableIntegerProvider.NumberToText(number, language);
                case DisplayMode.ShowDigits:
                    return _showDigitsProvider.GetText(number, language);
                case DisplayMode.SumOfTwoNumbers:
                    return _sumOfTwoNumbersProvider.GetText(number, language);
                default:
                    throw new NotSupportedException($"`{displayMode}` is not supported yet.");
            }
        }
    }
}