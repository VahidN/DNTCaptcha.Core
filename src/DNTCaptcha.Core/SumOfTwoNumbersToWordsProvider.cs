namespace DNTCaptcha.Core;

/// <summary>
///     SumOfTwoNumbersToWords Provider
/// </summary>
/// <remarks>
///     SumOfTwoNumbersToWords Provider
/// </remarks>
public class SumOfTwoNumbersToWordsProvider(
    IRandomNumberProvider randomNumberProvider,
    HumanReadableIntegerProvider humanReadableIntegerProvider) : ICaptchaTextProvider
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
            ? $"{humanReadableIntegerProvider.NumberToText(number - randomNumber, language)} + {humanReadableIntegerProvider.NumberToText(randomNumber, language)}"
            : $"{humanReadableIntegerProvider.NumberToText(number: 0, language)} + {humanReadableIntegerProvider.NumberToText(number, language)}";
    }
}