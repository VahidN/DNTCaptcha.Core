namespace DNTCaptcha.Core;

/// <summary>
///     SumOfTwoNumbersToWords Provider
/// </summary>
public class SumOfTwoNumbersToWordsProvider : ICaptchaTextProvider
{
    private readonly HumanReadableIntegerProvider _humanReadableIntegerProvider;
    private readonly IRandomNumberProvider _randomNumberProvider;

    /// <summary>
    ///     SumOfTwoNumbersToWords Provider
    /// </summary>
    public SumOfTwoNumbersToWordsProvider(
        IRandomNumberProvider randomNumberProvider,
        HumanReadableIntegerProvider humanReadableIntegerProvider)
    {
        _randomNumberProvider = randomNumberProvider;
        _humanReadableIntegerProvider = humanReadableIntegerProvider;
    }

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
                   ? $"{_humanReadableIntegerProvider.NumberToText(number - randomNumber, language)} + {_humanReadableIntegerProvider.NumberToText(randomNumber, language)}"
                   : $"{_humanReadableIntegerProvider.NumberToText(0, language)} + {_humanReadableIntegerProvider.NumberToText(number, language)}";
    }
}