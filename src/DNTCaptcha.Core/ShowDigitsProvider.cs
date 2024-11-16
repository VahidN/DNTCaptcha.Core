using System.Globalization;
using Microsoft.Extensions.Options;

namespace DNTCaptcha.Core;

/// <summary>
///     display a numeric value using the equivalent text
/// </summary>
/// <remarks>
///     display a numeric value using the equivalent text
/// </remarks>
public class ShowDigitsProvider(IOptions<DNTCaptchaOptions> options) : ICaptchaTextProvider
{
    private readonly DNTCaptchaOptions _captchaOptions = options.Value;

    /// <summary>
    ///     display a numeric value using the equivalent text
    /// </summary>
    /// <param name="number">input number</param>
    /// <param name="language">local language</param>
    /// <returns>the equivalent text</returns>
    public string GetText(int number, Language language)
        => _captchaOptions.AllowThousandsSeparators
            ? string.Format(CultureInfo.InvariantCulture, format: "{0:N0}", number)
            : number.ToString(CultureInfo.InvariantCulture);
}