using System;
using System.Security.Cryptography;
using DNTCaptcha.Core.Contracts;

namespace DNTCaptcha.Core.Providers
{
    /// <summary>
    /// Provides methods for generating cryptographically-strong random numbers.
    /// </summary>
    public class RandomNumberProvider : IRandomNumberProvider
    {
        private readonly RandomNumberGenerator _rand = RandomNumberGenerator.Create();

        /// <summary>
        /// Generates a random non-negative number.
        /// </summary>
        public int Next()
        {
            var randb = new byte[4];
            _rand.GetBytes(randb);
            var value = BitConverter.ToInt32(randb, 0);
            if (value < 0) value = -value;
            return value;
        }
        /// <summary>
        /// Generates a random non-negative number less than or equal to max.
        /// </summary>
        /// <param name="max">The maximum possible value.</param>
        public int Next(int max)
        {
            var randb = new byte[4];
            _rand.GetBytes(randb);
            var value = BitConverter.ToInt32(randb, 0);
            value = value % (max + 1); // % calculates remainder
            if (value < 0) value = -value;
            return value;
        }
        /// <summary>
        /// Generates a random non-negative number bigger than or equal to min and less than or
        ///  equal to max.
        /// </summary>
        /// <param name="min">The minimum possible value.</param>
        /// <param name="max">The maximum possible value.</param>
        public int Next(int min, int max)
        {
            var value = Next(max - min) + min;
            return value;
        }
    }
}