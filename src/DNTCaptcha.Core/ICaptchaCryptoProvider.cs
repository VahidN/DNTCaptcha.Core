namespace DNTCaptcha.Core
{
    /// <summary>
    /// The default captcha protection provider
    /// </summary>
    public interface ICaptchaCryptoProvider
    {
        /// <summary>
        /// Decrypts the message
        /// </summary>
        string? Decrypt(string inputText);

        /// <summary>
        /// Encrypts the message
        /// </summary>
        string Encrypt(string inputText);

        /// <summary>
        /// Creates the hash of the message
        /// </summary>
        (string HashString, byte[] HashBytes) Hash(string inputText);
    }
}