using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using HarfBuzzSharp;
using Microsoft.Extensions.Options;
using SkiaSharp;
using SkiaSharp.HarfBuzz;
using Buffer = HarfBuzzSharp.Buffer;

namespace DNTCaptcha.Core;

/// <summary>
///     The default captcha image provider
/// </summary>
/// <remarks>
///     The default captcha image provider
/// </remarks>
public class CaptchaImageProvider(IRandomNumberProvider randomNumberProvider, IOptions<DNTCaptchaOptions> options)
    : ICaptchaImageProvider
{
    private const int TextMargin = 5;

    private static readonly ConcurrentDictionary<string, SKTypeface> FontsTypeface =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly DNTCaptchaOptions _options =
        options == null ? throw new ArgumentNullException(nameof(options)) : options.Value;

    private readonly IRandomNumberProvider _randomNumberProvider =
        randomNumberProvider ?? throw new ArgumentNullException(nameof(randomNumberProvider));

    /// <summary>
    ///     Creates the captcha image.
    /// </summary>
    public byte[] DrawCaptcha(string text, string foreColor, string backColor, float fontSize, string fontName)
    {
        var fontType = GetFont(fontName, _options.CustomFontPath) ?? throw new InvalidOperationException(
            message:
            "`fontType` is null. It's better to set this option first: .UseCustomFont(Path.Combine(env.WebRootPath, \"fonts\", \"my-font.ttf\")); ");

        using var shaper = new SKShaper(fontType);

        if (!SKColor.TryParse(foreColor, out var skColor))
        {
            skColor = SKColors.Black;
        }

        using var textPaint = new SKPaint();
        textPaint.Color = skColor;
        textPaint.IsAntialias = true;
        textPaint.Style = SKPaintStyle.Fill;

        using var font = new SKFont();
        font.Size = fontSize;
        font.Typeface = fontType;
        font.Subpixel = true;

        var textBounds = GetTextBounds(text, font, textPaint);
        var width = GetTextWidth(text, fontSize, fontType);

        var imageWidth = (int)width + 2 * TextMargin;
        var imageHeight = (int)textBounds.Height + 2 * TextMargin;

        using var sKBitmap = new SKBitmap(imageWidth, imageHeight);
        using var canvas = new SKCanvas(sKBitmap);

        if (!SKColor.TryParse(backColor, out var skBackColor))
        {
            skBackColor = SKColors.White;
        }

        canvas.Clear(skBackColor);

        DrawText(text, canvas, shaper, textPaint, textBounds, SKTextAlign.Left, font);
        AddWaves(imageWidth, imageHeight, sKBitmap);
        CreateNoises(canvas);
        DrawRectangle(canvas, width, textBounds.Height);

        return ToPng(sKBitmap);
    }

    private void AddWaves(int width, int height, SKBitmap pic)
    {
        using var copy = new SKBitmap();
        pic.CopyTo(copy);

        double distort = _randomNumberProvider.NextNumber(min: 1, max: 6) *
                         (_randomNumberProvider.NextNumber(max: 2) == 1 ? 1 : -1);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                // Adds a simple wave
                var newX = (int)(x + distort * Math.Sin(Math.PI * y / 84.0));
                var newY = (int)(y + distort * Math.Cos(Math.PI * x / 44.0));

                if (newX < 0 || newX >= width)
                {
                    newX = 0;
                }

                if (newY < 0 || newY >= height)
                {
                    newY = 0;
                }

                pic.SetPixel(x, y, copy.GetPixel(newX, newY));
            }
        }
    }

    private void CreateNoises(SKCanvas canvas)
    {
        using var shader = SKShader.CreatePerlinNoiseTurbulence(_options.CaptchaNoise.BaseFrequencyX,
            _options.CaptchaNoise.BaseFrequencyY, _options.CaptchaNoise.NumOctaves, _options.CaptchaNoise.Seed);

        using var paint = new SKPaint();
        paint.Shader = shader;
        canvas.DrawPaint(paint);
    }

    private static float GetTextWidth(string text, float fontSize, SKTypeface typeface)
    {
        using var blob = typeface.OpenStream().ToHarfBuzzBlob();
        using var hbFace = new Face(blob, index: 0);
        using var hbFont = new Font(hbFace);
        using var buffer = new Buffer();
        buffer.AddUtf16(text);
        buffer.GuessSegmentProperties();
        hbFont.Shape(buffer);

        hbFont.GetScale(out var xScale, out _);
        var scale = fontSize / xScale;
        var width = buffer.GlyphPositions.Sum(position => position.XAdvance) * scale;

        return width;
    }

    private static SKRect GetTextBounds(string text, SKFont font, SKPaint textPaint)
    {
        font.MeasureText(text, out var textBounds, textPaint);

        return textBounds;
    }

    private void DrawText(string text,
        SKCanvas canvas,
        SKShaper shaper,
        SKPaint textPaint,
        SKRect textBounds,
        SKTextAlign textAlign,
        SKFont font)
    {
        var x = TextMargin + textBounds.Left;
        var y = Math.Abs(textBounds.Top) + TextMargin;

        canvas.DrawShapedText(shaper, text, x, y, textAlign, font, textPaint);

        textPaint.Color = SKColors.LightGray;

        switch (_randomNumberProvider.NextNumber(min: 1, max: 4))
        {
            case 1:
                canvas.DrawShapedText(shaper, text, x - 1, y - 1, textAlign, font, textPaint);

                break;

            case 2:
                canvas.DrawShapedText(shaper, text, x + 1, y - 1, textAlign, font, textPaint);

                break;

            case 3:
                canvas.DrawShapedText(shaper, text, x - 1, y + 1, textAlign, font, textPaint);

                break;

            case 4:
                canvas.DrawShapedText(shaper, text, x + 1, y + 1, textAlign, font, textPaint);

                break;
        }
    }

    private static void DrawRectangle(SKCanvas canvas, float width, float height)
    {
        using var skPaint = new SKPaint();
        skPaint.Color = SKColors.LightGray;
        skPaint.IsStroke = true;
        skPaint.StrokeWidth = 1f;

        canvas.DrawRect(new SKRect(left: 0, top: 0, width + 2 * TextMargin - 1, height + 2 * TextMargin - 1), skPaint);
    }

    private static SKTypeface GetFont(string fontName, string? customFontPath)
    {
        if (string.IsNullOrWhiteSpace(customFontPath))
        {
            return FontsTypeface.GetOrAdd(fontName, SKTypeface.FromFamilyName);
        }

        return FontsTypeface.GetOrAdd(customFontPath, key =>
        {
            using var embeddedFont = File.OpenRead(key);

            return SKTypeface.FromStream(File.OpenRead(key));
        });
    }

    private static byte[] ToPng(SKBitmap bitmap)
    {
        using var data = bitmap.Encode(SKEncodedImageFormat.Png, quality: 100);
        using var memory = new MemoryStream();
        data.SaveTo(memory);

        return memory.ToArray();
    }
}