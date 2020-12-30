using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DNTCaptcha.Core
{
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
            if (randomNumberProvider == null)
            {
                throw new ArgumentNullException(nameof(randomNumberProvider));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _keyBytes = getDesKey(options.Value.EncryptionKey, randomNumberProvider.NextNumber().ToString(CultureInfo.InvariantCulture));
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
        public string? Decrypt(string inputText)
        {
            if (string.IsNullOrWhiteSpace(inputText))
            {
                throw new ArgumentNullException(nameof(inputText));
            }

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
            if (string.IsNullOrWhiteSpace(inputText))
            {
                throw new ArgumentNullException(nameof(inputText));
            }

            var inputBytes = Encoding.UTF8.GetBytes(inputText);
            var bytes = encrypt(inputBytes);
            return WebEncoders.Base64UrlEncode(bytes);
        }

        [SuppressMessage("Microsoft.Usage", "S5547:encrypt uses a weak cryptographic algorithm TripleDES",
                        Justification = "That's enough for our usecase!")]
        [SuppressMessage("Microsoft.Usage", "CA5350:encrypt uses a weak cryptographic algorithm TripleDES",
                        Justification = "That's enough for our usecase!")]
        private byte[] encrypt(byte[] data)
        {
            using (var des = new TripleDESCryptoServiceProvider
            {
                Key = _keyBytes,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            })
            {
                using var encryptor = des.CreateEncryptor();
                using var cipherStream = new MemoryStream();
                using (var cryptoStream = new CryptoStream(cipherStream, encryptor, CryptoStreamMode.Write))
                using (var binaryWriter = new BinaryWriter(cryptoStream))
                {
                    // prepend IV to data
                    cipherStream.Write(des.IV); // This is an auto-generated random key
                    binaryWriter.Write(data);
                    cryptoStream.FlushFinalBlock();
                }
                return cipherStream.ToArray();
            }
        }

        [SuppressMessage("Microsoft.Usage", "S5547:encrypt uses a weak cryptographic algorithm TripleDES",
                        Justification = "That's enough for our usecase!")]
        [SuppressMessage("Microsoft.Usage", "CA5350:encrypt uses a weak cryptographic algorithm TripleDES",
                        Justification = "That's enough for our usecase!")]
        private byte[] decrypt(byte[] data)
        {
            using (var des = new TripleDESCryptoServiceProvider
            {
                Key = _keyBytes,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            })
            {
                var iv = new byte[8]; // 3DES-IV is always 8 bytes/64 bits because block size is always 64 bits
                Array.Copy(data, 0, iv, 0, iv.Length);

                using var ms = new MemoryStream();
                using (var decryptor = new CryptoStream(ms, des.CreateDecryptor(_keyBytes, iv), CryptoStreamMode.Write))
                using (var binaryWriter = new BinaryWriter(decryptor))
                {
                    // decrypt cipher text from data, starting just past the IV
                    binaryWriter.Write(
                        data,
                        iv.Length,
                        data.Length - iv.Length
                    );
                }
                return ms.ToArray();
            }
        }

        private byte[] getDesKey(string? key, string randomKey)
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