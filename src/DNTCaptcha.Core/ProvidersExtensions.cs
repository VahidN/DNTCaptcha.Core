using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace DNTCaptcha.Core
{
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
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			if (captchaProtectionProvider == null)
			{
				throw new ArgumentNullException(nameof(captchaProtectionProvider));
			}

			var userAgent = context.Request.Headers[HeaderNames.UserAgent].ToString();
			var issueDate = DateTime.Now.ToString("yyyy_MM_dd", CultureInfo.InvariantCulture);
			var name = nameof(ProvidersExtensions);
			var salt = $"::{issueDate}::{name}::{userAgent}";
			return captchaProtectionProvider.Hash(salt).HashString;
		}

		/// <summary>
		/// ToSha256
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string ToSha256(string value)
		{
			byte[] valueBytes = Encoding.UTF8.GetBytes(value);
			return Convert.ToHexString(SHA256.HashData(valueBytes));
		}

		/// <summary>
		/// ToBase64
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string ToBase64(string value)
		{
			byte[] valueBytes = Encoding.UTF8.GetBytes(value);
			return Convert.ToBase64String(SHA256.HashData(valueBytes));
		}

	}
}