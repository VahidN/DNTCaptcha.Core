using DNTCaptcha.Core.Contracts;

namespace DNTCaptcha.Core.Providers
{
    /// <summary>
    /// SumOfTwoNumbersToWords Provider
    /// </summary>
    public class SumOfTwoNumbersToWordsProvider : ICaptchaTextProvider
    {
        private readonly int _randomNumber;
        private readonly HumanReadableIntegerProvider _humanReadableIntegerProvider;

        /// <summary>
        /// SumOfTwoNumbersToWords Provider
        /// </summary>
        public SumOfTwoNumbersToWordsProvider(
            IRandomNumberProvider randomNumberProvider,
            HumanReadableIntegerProvider humanReadableIntegerProvider)
        {
            _randomNumber = randomNumberProvider.Next(1, 7);
            _humanReadableIntegerProvider = humanReadableIntegerProvider;
        }

        /// <summary>
        /// display a numeric value using the equivalent text
        /// </summary>
        /// <param name="number">input number</param>
        /// <param name="language">local language</param>
        /// <returns>the equivalent text</returns>
        public string GetText(long number, Language language)
        {
            var text = number > _randomNumber ?
                   $"{_humanReadableIntegerProvider.NumberToText(number - _randomNumber, language)} + {_humanReadableIntegerProvider.NumberToText(_randomNumber, language)}" :
                   $"{_humanReadableIntegerProvider.NumberToText(0, language)} + {_humanReadableIntegerProvider.NumberToText(number, language)}";
            return language == Language.Persian ? text.ToPersianNumbers() : text;
        }
    }
}