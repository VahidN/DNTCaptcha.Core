using System;
using DNTCaptcha.Core.Contracts;

namespace DNTCaptcha.Core.Providers
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
                   $"{number - _randomNumber} + {_randomNumber}" :
                   $"0 + {number}";
            return language == Language.Persian ? text.ToPersianNumbers() : text;
        }
    }
}