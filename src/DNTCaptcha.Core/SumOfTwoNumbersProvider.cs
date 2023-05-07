using static System.FormattableString;

namespace DNTCaptcha.Core;

/// <summary>
///     SumOfTwoNumbers Provider
/// </summary>
public class SumOfTwoNumbersProvider : ICaptchaTextProvider
{
    private readonly IRandomNumberProvider _randomNumberProvider;

    /// <summary>
    ///     SumOfTwoNumbers Provider
    /// </summary>
    public SumOfTwoNumbersProvider(IRandomNumberProvider randomNumberProvider) =>
        _randomNumberProvider = randomNumberProvider;

    /// <summary>
    ///     display a numeric value using the equivalent text
    /// </summary>
    /// <param name="number">input number</param>
    /// <param name="language">local language</param>
    /// <returns>the equivalent text</returns>
    public string GetText(int number, Language language)
    {
        var randomNumber = _randomNumberProvider.NextNumber(1, number);
        return number > randomNumber
                   ? Invariant($"{number - randomNumber} + {randomNumber}")
                   : Invariant($"0 + {number}");
    }
}