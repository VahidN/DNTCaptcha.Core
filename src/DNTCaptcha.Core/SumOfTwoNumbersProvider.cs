using System.Globalization;

namespace DNTCaptcha.Core;

/// <summary>
///     SumOfTwoNumbers Provider
/// </summary>
/// <remarks>
///     SumOfTwoNumbers Provider
/// </remarks>
public class SumOfTwoNumbersProvider(IRandomNumberProvider randomNumberProvider) : ICaptchaTextProvider
{
    /// <summary>
    ///     display a numeric value using the equivalent text
    /// </summary>
    /// <param name="number">input number</param>
    /// <param name="language">local language</param>
    /// <returns>the equivalent text</returns>
    public string GetText(int number, Language language)
    {
        var randomNumber = randomNumberProvider.NextNumber(min: 1, number);

        return number > randomNumber
            ? string.Create(CultureInfo.InvariantCulture, $"{number - randomNumber} + {randomNumber}")
            : string.Create(CultureInfo.InvariantCulture, $"0 + {number}");
    }
}
