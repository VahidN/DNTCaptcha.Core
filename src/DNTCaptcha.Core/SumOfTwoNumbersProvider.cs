using System;
using static System.FormattableString;

namespace DNTCaptcha.Core
{
    /// <summary>
    /// SumOfTwoNumbers Provider
    /// </summary>
    public class SumOfTwoNumbersProvider : ICaptchaTextProvider
    {
        private readonly int _randomNumber;

        /// <summary>
        /// SumOfTwoNumbers Provider
        /// </summary>
        public SumOfTwoNumbersProvider(IRandomNumberProvider randomNumberProvider)
        {
            if (randomNumberProvider == null)
            {
                throw new ArgumentNullException(nameof(randomNumberProvider));
            }

            _randomNumber = randomNumberProvider.NextNumber(1, 7);
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
                Invariant($"{number - _randomNumber} + {_randomNumber}") :
                Invariant($"0 + {number}");
            return language == Language.Persian ? text.ToPersianNumbers() : text;
        }
    }
}