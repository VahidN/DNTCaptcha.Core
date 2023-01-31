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
                    case '۰':
                    case '٠':
                        chars[i] = '0';
                        break;

                    case '١':
                    case '۱':
                        chars[i] = '1';
                        break;

                    case '٢':
                    case '۲':
                        chars[i] = '2';
                        break;

                    case '۳':
                    case '٣':
                        chars[i] = '3';
                        break;

                    case '۴':
                    case '٤':
                        chars[i] = '4';
                        break;

                    case '۵':
                    case '٥':
                        chars[i] = '5';
                        break;

                    case '۶':
                    case '٦':
                        chars[i] = '6';
                        break;

                    case '۷':
                    case '٧':
                        chars[i] = '7';
                        break;

                    case '۸':
                    case '٨':
                        chars[i] = '8';
                        break;

                    case '۹':
                    case '٩':
                        chars[i] = '9';
                        break;

                    default:
                        chars[i] = context[i];
                        break;
                }
        });
    }
}
