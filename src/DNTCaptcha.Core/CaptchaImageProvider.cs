using Microsoft.Extensions.Options;
using System;
using System.IO;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using System.Globalization;
using SixLabors.ImageSharp.Formats.Png;

namespace DNTCaptcha.Core
{
    /// <summary>
    /// The default captcha image provider
    /// </summary>
    public class CaptchaImageProvider : ICaptchaImageProvider
    {
        private const int Margin = 10;
        private readonly IRandomNumberProvider _randomNumberProvider;
        private readonly DNTCaptchaOptions _options;
        private Font? _font;

        /// <summary>
        /// The default captcha image provider
        /// </summary>
        public CaptchaImageProvider(
            IRandomNumberProvider randomNumberProvider,
            IOptions<DNTCaptchaOptions> options)
        {
            _randomNumberProvider = randomNumberProvider ?? throw new ArgumentNullException(nameof(randomNumberProvider));
            _options = options == null ? throw new ArgumentNullException(nameof(options)) : options.Value;
        }

        /// <summary>
        /// Creates the captcha image.
        /// </summary>
        public byte[] DrawCaptcha(string message, string foreColor, string backColor, float fontSize, string fontName)
        {
            _font ??= getFont(fontName, fontSize);
            var (width, height) = getImageSize(message);
            using (var image = new Image<Rgba32>(width, height))
            {
                image.Mutate(pc =>
                {
                    pc.SetGraphicsOptions(g => g.Antialias = true);
                    drawText(pc, message, foreColor, backColor);
                    drawRandomLines(pc, width, height);
                    distortImage(pc, foreColor);
                });
                return saveAsPng(image);
            }
        }

        private (int Width, int Height) getImageSize(string message)
        {
            if (_font is null)
            {
                throw new InvalidOperationException("font is null.");
            }
            var captchaSize = TextMeasurer.Measure(message, new TextOptions(_font));
            var width = (int)captchaSize.Width + Margin;
            var height = (int)captchaSize.Height + Margin;
            return (width, height);
        }

        private static byte[] saveAsPng(Image<Rgba32> image)
        {
            using var stream = new MemoryStream();
            image.Save(stream, new PngEncoder());
            return stream.ToArray();
        }

        private void drawText(IImageProcessingContext pc, string message, string foreColor, string backColor)
        {
            var fColor = Color.ParseHex(foreColor);
            var bColor = Color.ParseHex(backColor);
            pc.Fill(bColor);
            pc.DrawText(message, _font, fColor, new PointF(0, 0));
        }

        private void distortImage(IImageProcessingContext pc, string foreColor)
        {
            var degrees = _randomNumberProvider.NextNumber(1, 6);
            pc.Skew(degrees, degrees);
            pc.Vignette(Color.ParseHex(foreColor));
        }

        private void drawRandomLines(IImageProcessingContext pc, int width, int height)
        {
            const float stroke = 1.5F;
            var linesCount = _options.CaptchaNoise.NoiseLinesCount;
            const int distanceFromEdge = 10;
            for (var i = 0; i < linesCount; i++)
            {
                var x0 = _randomNumberProvider.NextNumber(distanceFromEdge, width - distanceFromEdge);
                var y0 = _randomNumberProvider.NextNumber(distanceFromEdge, height - distanceFromEdge);
                var x1 = _randomNumberProvider.NextNumber(distanceFromEdge, width - distanceFromEdge);
                var y1 = _randomNumberProvider.NextNumber(distanceFromEdge, height - distanceFromEdge);
                pc.DrawLines(Color.LightGray, stroke, new PointF(x0, y0), new PointF(x1, y1));
            }
        }

        private Font getFont(string fontName, float fontSize)
        {
            if (string.IsNullOrWhiteSpace(_options.CustomFontPath))
            {
                var fontFamily = SystemFonts.Get(fontName, CultureInfo.InvariantCulture);
                return new Font(fontFamily, fontSize);
            }

            var fontCollection = new FontCollection();
            return fontCollection.Add(_options.CustomFontPath, CultureInfo.InvariantCulture).CreateFont(fontSize);
        }
    }
}