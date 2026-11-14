using System;
using System.Globalization;

namespace DNTCaptcha.Core;

/// <summary>
///     Formats a non-negative number as digits without thousands separators.
/// </summary>
public sealed class AudioDigitsProvider : ICaptchaTextProvider
{
    /// <inheritdoc />
    public string GetText(int number, Language language)
        => number < 0
            ? throw new ArgumentOutOfRangeException(nameof(number), "AudioDigits requires a non-negative number.")
            : number.ToString(CultureInfo.InvariantCulture);
}
