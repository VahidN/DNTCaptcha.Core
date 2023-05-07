namespace DNTCaptcha.Core;

/// <summary>
///     Convert a number into text
/// </summary>
public interface ICaptchaTextProvider
{
    /// <summary>
    ///     display a numeric value using the equivalent text
    /// </summary>
    /// <param name="number">input number</param>
    /// <param name="language">local language</param>
    /// <returns>the equivalent text</returns>
    string GetText(int number, Language language);
}