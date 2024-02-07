using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;

namespace DNTCaptcha.Core;

/// <summary>
///     Represents the storage to save the captcha tokens.
/// </summary>
public interface ICaptchaStorageProvider
{
    /// <summary>
    ///     Adds the specified token and its value to the storage.
    /// </summary>
    void Add(HttpContext context, string? token, string value);

    /// <summary>
    ///     Determines whether the <see cref="ICaptchaStorageProvider" /> contains a specific token.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="token">The specified token.</param>
    /// <returns>
    ///     <c>True</c> if the value is found in the <see cref="ICaptchaStorageProvider" />; otherwise <c>false</c>.
    /// </returns>
    bool Contains(HttpContext context, [NotNullWhen(true)] string? token);

    /// <summary>
    ///     Gets the value associated with the specified token.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="token">The specified token.</param>
    string? GetValue(HttpContext context, string? token);

    /// <summary>
    ///     Removes the specified token from the storage.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="token">The specified token.</param>
    void Remove(HttpContext context, string? token);
}