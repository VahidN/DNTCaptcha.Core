using System;
using System.Security.Cryptography;

namespace DNTCaptcha.Core;

/// <summary>
///     Provides methods for generating cryptographically-strong random numbers.
/// </summary>
public class RandomNumberProvider : IRandomNumberProvider
{
    private readonly RandomNumberGenerator _rand = RandomNumberGenerator.Create();

    /// <summary>
    ///     Fills an array of bytes with a cryptographically strong random sequence of values.
    /// </summary>
    /// <param name="randomBytes"></param>
    public void NextBytes(byte[] randomBytes) => _rand.GetBytes(randomBytes);

    /// <summary>
    ///     Generates a random non-negative number.
    /// </summary>
    public byte[] NextBytes()
    {
        var randb = new byte[4];
        _rand.GetBytes(randb);

        return randb;
    }

    /// <summary>
    ///     Generates a random non-negative number.
    /// </summary>
    public int NextNumber()
    {
        var randb = NextBytes();
        var value = BitConverter.ToInt32(randb, startIndex: 0);

        if (value < 0)
        {
            value = -value;
        }

        return value;
    }

    /// <summary>
    ///     Generates a random non-negative number less than or equal to max.
    /// </summary>
    /// <param name="max">The maximum possible value.</param>
    public int NextNumber(int max)
    {
        var randb = new byte[4];
        _rand.GetBytes(randb);
        var value = BitConverter.ToInt32(randb, startIndex: 0);
        value %= max + 1; // % calculates remainder

        if (value < 0)
        {
            value = -value;
        }

        return value;
    }

    /// <summary>
    ///     Generates a random non-negative number bigger than or equal to min and less than or
    ///     equal to max.
    /// </summary>
    /// <param name="min">The minimum possible value.</param>
    /// <param name="max">The maximum possible value.</param>
    public int NextNumber(int min, int max)
    {
        var value = NextNumber(max - min) + min;

        return value;
    }
}