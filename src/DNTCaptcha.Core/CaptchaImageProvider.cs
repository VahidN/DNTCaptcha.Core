using Microsoft.Extensions.Options;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;

namespace DNTCaptcha.Core
{
    /// <summary>
    /// The default captcha image provider
    /// </summary>
    public class CaptchaImageProvider : ICaptchaImageProvider
    {
        private readonly IRandomNumberProvider _randomNumberProvider;
        private readonly DNTCaptchaOptions _options;

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
            return useFont(fontName, fontSize, captchaFont =>
            {
                var fColor = ColorTranslator.FromHtml(foreColor);
                var bColor = string.IsNullOrWhiteSpace(backColor)
                                ? Color.Transparent : ColorTranslator.FromHtml(backColor);

                var captchaSize = measureString(message, captchaFont);

                const int margin = 8;
                var height = (int)captchaSize.Height + margin;
                var width = (int)captchaSize.Width + margin;

                var rectF = new Rectangle(0, 0, width: width, height: height);
                using var pic = new Bitmap(width: width, height: height);
                using var graphics = Graphics.FromImage(pic);
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.High;
                graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

                using var format = new StringFormat
                {
                    FormatFlags = StringFormatFlags.DirectionRightToLeft
                };
                var rect = drawRoundedRectangle(graphics, rectF, 15, new Pen(bColor) { Width = 1.1f }, bColor);
                graphics.DrawString(message, captchaFont, new SolidBrush(fColor), rect, format);

                using var stream = new MemoryStream();
                distortImage(fontName, height, width, pic);
                pic.Save(stream, ImageFormat.Png);
                return stream.ToArray();
            });
        }

        private static Rectangle drawRoundedRectangle(Graphics gfx, Rectangle bounds, int cornerRadius, Pen drawPen, Color fillColor)
        {
            int strokeOffset = Convert.ToInt32(Math.Ceiling(drawPen.Width));
            bounds = Rectangle.Inflate(bounds, -strokeOffset, -strokeOffset);
            drawPen.EndCap = drawPen.StartCap = LineCap.Round;
            using var gfxPath = new GraphicsPath();
            gfxPath.AddArc(bounds.X, bounds.Y, cornerRadius, cornerRadius, 180, 90);
            gfxPath.AddArc(bounds.X + bounds.Width - cornerRadius, bounds.Y, cornerRadius, cornerRadius, 270, 90);
            gfxPath.AddArc(bounds.X + bounds.Width - cornerRadius, bounds.Y + bounds.Height - cornerRadius, cornerRadius, cornerRadius, 0, 90);
            gfxPath.AddArc(bounds.X, bounds.Y + bounds.Height - cornerRadius, cornerRadius, cornerRadius, 90, 90);
            gfxPath.CloseAllFigures();
            using var brush = new SolidBrush(fillColor);
            gfx.FillPath(brush, gfxPath);
            gfx.DrawPath(drawPen, gfxPath);
            return bounds;
        }

        private static SizeF measureString(string text, Font f)
        {
            using var bmp = new Bitmap(1, 1);
            using var g = Graphics.FromImage(bmp);
            return g.MeasureString(text, f);
        }

        private void distortImage(string fontName, int height, int width, Bitmap pic)
        {
            addWaves(height, width, pic);
            drawRandomLines(fontName, height, width, pic);
        }

        private void addWaves(int height, int width, Bitmap pic)
        {
            using var copy = new Bitmap(pic);
            double distort = _randomNumberProvider.NextNumber(1, 6) * (_randomNumberProvider.NextNumber(2) == 1 ? 1 : -1);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Adds a simple wave
                    int newX = (int)(x + (distort * Math.Sin(Math.PI * y / 84.0)));
                    int newY = (int)(y + (distort * Math.Cos(Math.PI * x / 44.0)));
                    if (newX < 0 || newX >= width) newX = 0;
                    if (newY < 0 || newY >= height) newY = 0;
                    pic.SetPixel(x, y, copy.GetPixel(newX, newY));
                }
            }
        }

        private void drawRandomLines(string fontName, int height, int width, Bitmap pic)
        {
            using var graphics = Graphics.FromImage(pic);
            using var hatchBrush = new HatchBrush(HatchStyle.LargeConfetti, Color.LightGray, Color.DarkGray);
            useFont(fontName, 2, captchaFont =>
            {
                const int distanceFromEdge = 10;
                float density = _options.CaptchaNoise.NoisePixelsDensity;
                for (var i = 0; i < (int)(width * height / density); i++)
                {
                    var x = _randomNumberProvider.NextNumber(distanceFromEdge, width - distanceFromEdge);
                    var y = _randomNumberProvider.NextNumber(distanceFromEdge, height - distanceFromEdge);
                    graphics.DrawString("*", captchaFont, hatchBrush, x, y);
                }

                var linesCount = _options.CaptchaNoise.NoiseLinesCount;
                for (var i = 0; i < linesCount; i++)
                {
                    var x0 = _randomNumberProvider.NextNumber(distanceFromEdge, width - distanceFromEdge);
                    var y0 = _randomNumberProvider.NextNumber(distanceFromEdge, height - distanceFromEdge);
                    var x1 = _randomNumberProvider.NextNumber(distanceFromEdge, width - distanceFromEdge);
                    var y1 = _randomNumberProvider.NextNumber(distanceFromEdge, height - distanceFromEdge);
                    graphics.DrawLine(Pens.White, x0, y0, x1, y1);
                }

                return Array.Empty<byte>();
            });
        }

        private byte[] useFont(string fontName, float fontSize, Func<Font, byte[]> action)
        {
            if (string.IsNullOrWhiteSpace(_options.CustomFontPath))
            {
                using var captchaFont = new Font(fontName, fontSize, FontStyle.Regular, GraphicsUnit.Pixel);
                return action(captchaFont);
            }
            else
            {
                using var privateFontCollection = new PrivateFontCollection();
                privateFontCollection.AddFontFile(_options.CustomFontPath);
                using var fontFamily = privateFontCollection.Families[0];
                using var captchaFont = new Font(fontFamily, fontSize, FontStyle.Regular, GraphicsUnit.Pixel);
                return action(captchaFont);
            }
        }
    }
}