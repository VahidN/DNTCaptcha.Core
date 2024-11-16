namespace DNTCaptcha.Core;

internal static class NumbersNormalizer
{
    /// <summary>
    ///     Converts Persian and Arabic digits of a given string to their equivalent English digits.
    /// </summary>
    /// <param name="data">Persian number</param>
    internal static string ToEnglishNumbers(this string data)
    {
        if (string.IsNullOrWhiteSpace(data))
        {
            return string.Empty;
        }

        var length = data.Length;

        return string.Create(length, data, (chars, context) =>
        {
            for (var i = 0; i < length; i++)
            {
                chars[i] = context[i] switch
                {
                    '\u06F0' or '\u0660' => '0',
                    '\u06F1' or '\u0661' => '1',
                    '\u06F2' or '\u0662' => '2',
                    '\u06F3' or '\u0663' => '3',
                    '\u06F4' or '\u0664' => '4',
                    '\u06F5' or '\u0665' => '5',
                    '\u06F6' or '\u0666' => '6',
                    '\u06F7' or '\u0667' => '7',
                    '\u06F8' or '\u0668' => '8',
                    '\u06F9' or '\u0669' => '9',
                    _ => context[i]
                };
            }
        });
    }
}