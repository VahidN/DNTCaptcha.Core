namespace DNTCaptcha.Core;

internal static class NumbersNormalizer
{
    /// <summary>
    ///     Converts Persian and Arabic digits of a given string to their equivalent English digits.
    /// </summary>
    /// <param name="data">Persian number</param>
    internal static string ToEnglishNumbers(this string data)
    {
        if (string.IsNullOrWhiteSpace(data)) return string.Empty;

        var length = data.Length;

        return string.Create(length, data, (chars, context) =>
        {
            for (var i = 0; i < length; i++)
                switch (context[i])
                {
                    case '\u06F0':
                    case '\u0660':
                        chars[i] = '0';
                        break;

                    case '\u06F1':
                    case '\u0661':
                        chars[i] = '1';
                        break;

                    case '\u06F2':
                    case '\u0662':
                        chars[i] = '2';
                        break;

                    case '\u06F3':
                    case '\u0663':
                        chars[i] = '3';
                        break;

                    case '\u06F4':
                    case '\u0664':
                        chars[i] = '4';
                        break;

                    case '\u06F5':
                    case '\u0665':
                        chars[i] = '5';
                        break;

                    case '\u06F6':
                    case '\u0666':
                        chars[i] = '6';
                        break;

                    case '\u06F7':
                    case '\u0667':
                        chars[i] = '7';
                        break;

                    case '\u06F8':
                    case '\u0668':
                        chars[i] = '8';
                        break;

                    case '\u06F9':
                    case '\u0669':
                        chars[i] = '9';
                        break;

                    default:
                        chars[i] = context[i];
                        break;
                }
        });
    }
}
