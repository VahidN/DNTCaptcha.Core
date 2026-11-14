namespace DNTCaptcha.Core;

/// <summary>
///     Assembles recorded digits into a complete WAV challenge.
/// </summary>
public interface ICaptchaAudioProvider
{
    /// <summary>
    ///     Determines whether recordings exist for the requested language and voice.
    /// </summary>
    bool Supports(Language language, CaptchaAudioVoice voice);

    /// <summary>
    ///     Creates a PCM WAV file pronouncing the supplied ASCII digits individually.
    /// </summary>
    byte[] CreateAudio(string digits, Language language, CaptchaAudioVoice voice);
}
