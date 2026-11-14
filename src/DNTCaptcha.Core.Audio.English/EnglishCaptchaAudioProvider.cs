using System;
using System.IO;
using System.Linq;
using System.Text;

namespace DNTCaptcha.Core;

/// <summary>
///     Assembles English digit recordings into mono, 24 kHz, 16-bit PCM WAV challenges.
/// </summary>
public sealed class EnglishCaptchaAudioProvider : ICaptchaAudioProvider
{
    private const int SampleRate = 24000;
    private const int BytesPerSample = 2;
    private const int PauseMilliseconds = 150;
    private static readonly byte[] Pause = new byte[SampleRate * BytesPerSample * PauseMilliseconds / 1000];
    private static readonly Lazy<byte[][]> MaleDigits = new(() => LoadDigits(CaptchaAudioVoice.Male));
    private static readonly Lazy<byte[][]> FemaleDigits = new(() => LoadDigits(CaptchaAudioVoice.Female));

    /// <inheritdoc />
    public bool Supports(Language language, CaptchaAudioVoice voice)
        => language == Language.English && voice is CaptchaAudioVoice.Male or CaptchaAudioVoice.Female;

    /// <inheritdoc />
    public byte[] CreateAudio(string digits, Language language, CaptchaAudioVoice voice)
    {
        if (!Supports(language, voice))
        {
            throw new ArgumentException("Only English Male and Female recordings are available.", nameof(language));
        }

        if (string.IsNullOrEmpty(digits) || digits.Length > 10 || digits.Any(digit => digit is < '0' or > '9'))
        {
            throw new ArgumentException("Expected between one and ten ASCII digits.", nameof(digits));
        }

        var recordings = voice == CaptchaAudioVoice.Male ? MaleDigits.Value : FemaleDigits.Value;
        var dataLength = digits.Sum(digit => recordings[digit - '0'].Length) + (digits.Length - 1) * Pause.Length;

        using var stream = new MemoryStream(44 + dataLength);
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        WriteHeader(writer, dataLength);

        for (var index = 0; index < digits.Length; index++)
        {
            if (index > 0)
            {
                writer.Write(Pause);
            }

            writer.Write(recordings[digits[index] - '0']);
        }

        return stream.ToArray();
    }

    private static void WriteHeader(BinaryWriter writer, int dataLength)
    {
        writer.Write("RIFF"u8);
        writer.Write(36 + dataLength);
        writer.Write("WAVEfmt "u8);
        writer.Write(16);
        writer.Write((short)1); // PCM
        writer.Write((short)1); // Mono
        writer.Write(SampleRate);
        writer.Write(SampleRate * BytesPerSample);
        writer.Write((short)BytesPerSample);
        writer.Write((short)16);
        writer.Write("data"u8);
        writer.Write(dataLength);
    }

    private static byte[][] LoadDigits(CaptchaAudioVoice voice)
        => Enumerable.Range(0, 10).Select(digit => ReadPcm(voice, digit)).ToArray();

    private static byte[] ReadPcm(CaptchaAudioVoice voice, int digit)
    {
        var name = $"DNTCaptcha.Core.Audio.English.{voice}.{digit}.wav";
        using var stream = typeof(EnglishCaptchaAudioProvider).Assembly.GetManifestResourceStream(name) ??
                           throw new InvalidOperationException($"Missing embedded recording: {name}");
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

        if (new string(reader.ReadChars(4)) != "RIFF" || reader.ReadUInt32() != stream.Length - 8 ||
            new string(reader.ReadChars(4)) != "WAVE")
        {
            throw new InvalidDataException($"Invalid WAV header: {name}");
        }

        var validFormat = false;
        byte[]? pcm = null;

        while (stream.Position + 8 <= stream.Length)
        {
            var chunk = new string(reader.ReadChars(4));
            var length = reader.ReadUInt32();
            var next = stream.Position + length + (length & 1);

            if (next > stream.Length)
            {
                throw new InvalidDataException($"Truncated WAV chunk: {name}");
            }

            if (chunk == "fmt " && length >= 16)
            {
                validFormat = reader.ReadUInt16() == 1 && reader.ReadUInt16() == 1 &&
                              reader.ReadUInt32() == SampleRate && reader.ReadUInt32() == SampleRate * BytesPerSample &&
                              reader.ReadUInt16() == BytesPerSample && reader.ReadUInt16() == 16;
            }
            else if (chunk == "data")
            {
                pcm = reader.ReadBytes(checked((int)length));
            }

            stream.Position = next;
        }

        return validFormat && pcm is { Length: > 0 } && pcm.Length % BytesPerSample == 0
            ? pcm
            : throw new InvalidDataException($"Expected mono, 24 kHz, 16-bit PCM: {name}");
    }
}
