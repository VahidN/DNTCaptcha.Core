namespace DNTCaptcha.Core;

/// <summary>
///     Defines options of the captcha's noise
/// </summary>
public class DNTCaptchaNoise
{
    /// <summary>
    ///     The frequency in the x-direction in the range of 0..1.
    ///     Its default value is 0.015f
    /// </summary>
    public float BaseFrequencyX { set; get; } = 0.015f;

    /// <summary>
    ///     The frequency in the y-direction in the range of 0..1.
    ///     Its default value is 0.015f
    /// </summary>
    public float BaseFrequencyY { set; get; } = 0.015f;

    /// <summary>
    ///     The number of octaves, usually fairly small.
    ///     Its default value is 1
    /// </summary>
    public int NumOctaves { set; get; } = 1;

    /// <summary>
    ///     The randomization seed.
    ///     Its default value is 0
    /// </summary>
    public float Seed { set; get; }
}