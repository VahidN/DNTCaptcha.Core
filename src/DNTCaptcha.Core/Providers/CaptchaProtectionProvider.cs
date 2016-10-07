using System;
using System.Security.Cryptography;
using System.Text;
using DNTCaptcha.Core.Contracts;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;

namespace DNTCaptcha.Core.Providers
{
    /// <summary>
    /// The default captcha protection provider
    /// </summary>
    public class CaptchaProtectionProvider : ICaptchaProtectionProvider
    {
        private readonly ILogger<CaptchaProtectionProvider> _logger;
        private readonly IDataProtector _dataProtector;

        /// <summary>
        /// The default captcha protection provider
        /// </summary>
        public CaptchaProtectionProvider(
            IDataProtectionProvider dataProtectionProvider,
            ILogger<CaptchaProtectionProvider> logger)
        {
            dataProtectionProvider.CheckArgumentNull(nameof(dataProtectionProvider));
            logger.CheckArgumentNull(nameof(logger));

            _logger = logger;
            _dataProtector = dataProtectionProvider.CreateProtector(typeof(CaptchaProtectionProvider).FullName);
        }

        /// <summary>
        /// Decrypts the message
        /// </summary>
        public string Decrypt(string inputText)
        {
            inputText.CheckArgumentNull(nameof(inputText));

            try
            {
                var inputBytes = Convert.FromBase64String(inputText);
                var bytes = _dataProtector.Unprotect(inputBytes);
                return Encoding.UTF8.GetString(bytes);
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex.Message, "Invalid base 64 string. Fall through.");
            }
            catch (CryptographicException ex)
            {
                _logger.LogError(ex.Message, "Invalid protected payload. Fall through.");
            }

            return null;
        }

        /// <summary>
        /// Encrypts the message
        /// </summary>
        public string Encrypt(string inputText)
        {
            inputText.CheckArgumentNull(nameof(inputText));

            var inputBytes = Encoding.UTF8.GetBytes(inputText);
            var bytes = _dataProtector.Protect(inputBytes);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Creates the hash of the message
        /// </summary>
        public string Hash(string inputText)
        {
            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(inputText));
                return Encoding.UTF8.GetString(hash);
            }
        }
    }
}