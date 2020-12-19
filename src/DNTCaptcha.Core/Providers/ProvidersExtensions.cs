using System;
using System.Globalization;
using DNTCaptcha.Core.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace DNTCaptcha.Core.Providers
{
    /// <summary>
    /// Providers Extensions
    /// </summary>
    public static class ProvidersExtensions
    {
        /// <summary>
        /// The cookie value's bindings.
        /// </summary>
        public static string GetSalt(this HttpContext context, ICaptchaCryptoProvider captchaProtectionProvider)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (captchaProtectionProvider == null)
            {
                throw new ArgumentNullException(nameof(captchaProtectionProvider));
            }

            var ip = context.Connection.RemoteIpAddress;
            var userAgent = (string)context.Request.Headers[HeaderNames.UserAgent];
            var issueDate = DateTime.Now.ToString("yyyy_MM_dd", CultureInfo.InvariantCulture);
            var name = typeof(ProvidersExtensions).Name;
            var salt = $"::{issueDate}::{name}::{ip}::{userAgent}";
            return captchaProtectionProvider.Hash(salt).HashString;
        }
    }
}