namespace DNTCaptcha.Core;

/// <summary>
///     Validates the input number.
/// </summary>
public interface IDNTCaptchaValidatorService
{
    /// <summary>
    ///     Validates the input number.
    /// </summary>
    bool HasRequestValidCaptchaEntry();
}