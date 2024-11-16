using static System.FormattableString;

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
            ? Invariant($"{number - randomNumber} + {randomNumber}")
            : Invariant($"0 + {number}");
    }
}