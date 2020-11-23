using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using DNTCaptcha.Core.Contracts;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DNTCaptcha.Core.Providers
{
    /// <summary>
    /// The default captcha protection provider
    /// </summary>
    public interface ICaptchaCryptoProvider
    {
        /// <summary>
        /// Decrypts the message
        /// </summary>
        string Decrypt(string inputText);

        /// <summary>
        /// Encrypts the message
        /// </summary>
        string Encrypt(string inputText);

        /// <summary>
        /// Creates the hash of the message
        /// </summary>
        (string HashString, byte[] HashBytes) Hash(string inputText);
    }

    /// <summary>
    /// The default captcha protection provider
    /// </summary>
    public class CaptchaCryptoProvider : ICaptchaCryptoProvider
    {
        private readonly byte[] _keyBytes;
        private readonly ILogger<CaptchaCryptoProvider> _logger;

        /// <summary>
        /// The default captcha protection provider
        /// </summary>
        public CaptchaCryptoProvider(
            IOptions<DNTCaptchaOptions> options,
            IRandomNumberProvider randomNumberProvider,
            ILogger<CaptchaCryptoProvider> logger)
        {
            _logger = logger;
            _keyBytes = getDesKey(options.Value.EncryptionKey, randomNumberProvider.Next().ToString());
        }

        /// <summary>
        /// Creates the hash of the message
        /// </summary>
        public (string HashString, byte[] HashBytes) Hash(string inputText)
        {
            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(inputText));
                return (Encoding.UTF8.GetString(hash), hash);
            }
        }

        /// <summary>
        /// Decrypts the message
        /// </summary>
        public string Decrypt(string inputText)
        {
            inputText.CheckArgumentNull(nameof(inputText));

            try
            {
                var inputBytes = WebEncoders.Base64UrlDecode(inputText);
                var bytes = decrypt(inputBytes);
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
            var bytes = encrypt(inputBytes);
            return WebEncoders.Base64UrlEncode(bytes);
        }

        private byte[] encrypt(byte[] data)
        {
            using (var des = new TripleDESCryptoServiceProvider
            {
                Key = _keyBytes,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            })
            {
                using (var cTransform = des.CreateEncryptor())
                {
                    var result = cTransform.TransformFinalBlock(data, 0, data.Length);
                    des.Clear();
                    return result;
                }
            }
        }

        private byte[] decrypt(byte[] data)
        {
            using (var des = new TripleDESCryptoServiceProvider
            {
                Key = _keyBytes,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            })
            {
                using (var cTransform = des.CreateDecryptor())
                {
                    var result = cTransform.TransformFinalBlock(data, 0, data.Length);
                    des.Clear();
                    return result;
                }
            }
        }

        private byte[] getDesKey(string key, string randomKey)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                key = randomKey;
                _logger.LogWarning("A random key was generated. You can change it by setting the `options.WithEncryptionKey(...)` method.");
            }
            // The key size of TripleDES is 168 bits, its len in byte is 24 Bytes (or 192 bits).
            // Last bit of each byte is not used (or used as version in some hardware).
            // Key len for TripleDES can also be 112 bits which is again stored in 128 bits or 16 bytes.
            return Hash(key).HashBytes.Take(24).ToArray();
        }
    }
}