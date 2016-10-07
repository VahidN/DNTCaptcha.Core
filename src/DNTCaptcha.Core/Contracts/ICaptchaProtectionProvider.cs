namespace DNTCaptcha.Core.Contracts
{
    /// <summary>
    /// Captcha Protection Provider
    /// </summary>
    public interface ICaptchaProtectionProvider
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
        string Hash(string inputText);
    }
}