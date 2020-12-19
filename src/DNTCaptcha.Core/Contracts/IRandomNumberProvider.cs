namespace DNTCaptcha.Core.Contracts
{
    /// <summary>
    /// Provides methods for generating cryptographically-strong random numbers.
    /// </summary>
    public interface IRandomNumberProvider
    {
        /// <summary>
        /// Fills an array of bytes with a cryptographically strong random sequence of values.
        /// </summary>
        /// <param name="randomBytes"></param>
        void NextBytes(byte[] randomBytes);

        /// <summary>
        /// Generates a random non-negative number.
        /// </summary>
        int NextNumber();

        /// <summary>
        /// Generates a random non-negative number less than or equal to max.
        /// </summary>
        /// <param name="max">The maximum possible value.</param>
        int NextNumber(int max);

        /// <summary>
        /// Generates a random non-negative number bigger than or equal to min and less than or
        ///  equal to max.
        /// </summary>
        /// <param name="min">The minimum possible value.</param>
        /// <param name="max">The maximum possible value.</param>
        int NextNumber(int min, int max);
    }
}