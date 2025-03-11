using System;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace DNTCaptcha.Core;

/// <summary>
///     Providers Extensions
/// </summary>
public static class ProvidersExtensions
{
    /// <summary>
    ///     The cookie value's bindings.
    /// </summary>
    public static string GetSalt(this HttpContext context, ICaptchaCryptoProvider captchaProtectionProvider)
    {
        ArgumentNullException.ThrowIfNull(context);

        ArgumentNullException.ThrowIfNull(captchaProtectionProvider);

        var userAgent = context.Request.Headers[HeaderNames.UserAgent].ToString();
        var issueDate = DateTime.Now.ToString(format: "yyyy_MM_dd", CultureInfo.InvariantCulture);
        var salt = $"::{issueDate}::{nameof(ProvidersExtensions)}::{userAgent}";

        return captchaProtectionProvider.Hash(salt).HashString;
    }
}
