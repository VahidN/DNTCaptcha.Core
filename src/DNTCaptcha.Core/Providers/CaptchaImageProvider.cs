using DNTCaptcha.Core.Contracts;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace DNTCaptcha.Core.Providers
{
    /// <summary>
    /// The default captcha image provider
    /// </summary>
    public class CaptchaImageProvider : ICaptchaImageProvider
    {
        private readonly IRandomNumberProvider _randomNumberProvider;

        /// <summary>
        /// The default captcha image provider
        /// </summary>
        public CaptchaImageProvider(IRandomNumberProvider randomNumberProvider)
        {
            randomNumberProvider.CheckArgumentNull(nameof(randomNumberProvider));

            _randomNumberProvider = randomNumberProvider;
        }

        /// <summary>
        /// Creates the captcha image.
        /// </summary>
        public byte[] DrawCaptcha(string message, string foreColor, string backColor, float fontSize, string fontName)
        {
            var fColor = ColorTranslator.FromHtml(foreColor);
            var bColor = string.IsNullOrWhiteSpace(backColor) ?
                Color.Transparent : ColorTranslator.FromHtml(backColor);

            var captchaFont = new Font(fontName, fontSize, FontStyle.Regular, GraphicsUnit.Pixel);

            var captchaSize = measureString(message, captchaFont);

            const int margin = 8;
            var height = (int)captchaSize.Height + margin;
            var width = (int)captchaSize.Width + margin;

            var rectF = new Rectangle(0, 0, width: width, height: height);
            using (var pic = new Bitmap(width: width, height: height))
            {
                using (var graphics = Graphics.FromImage(pic))
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.High;
                    graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

                    using (var font = captchaFont)
                    {
                        using (var format = new StringFormat())
                        {
                            format.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                            var rect = drawRoundedRectangle(graphics, rectF, 15, new Pen(bColor) { Width = 1.1f }, bColor);
                            graphics.DrawString(message, font, new SolidBrush(fColor), rect, format);

                            using (var stream = new MemoryStream())
                            {
                                distortImage(height, width, pic);
                                pic.Save(stream, ImageFormat.Png);
                                return stream.ToArray();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates the captcha image.
        /// </summary>
        public byte[] DrawCaptcha(string message, string foreColor, float fontSize, string fontName)
        {
            var fColor = ColorTranslator.FromHtml(foreColor);
            message = message.Replace(",", string.Empty);
            const int margin = 8;
            var captchaFont = new Font(fontName, fontSize, FontStyle.Bold, GraphicsUnit.Pixel);
            var captchaSize = measureString(message, captchaFont);
            var height = (int)captchaSize.Height + margin;
            var width = (int)captchaSize.Width + margin;

            using (var pic = new Bitmap(width, height, PixelFormat.Format24bppRgb))
            {
                var data = pic.LockBits(new Rectangle(0, 0, pic.Width, pic.Height), ImageLockMode.WriteOnly, pic.PixelFormat);
                var noise = new byte[data.Width * data.Height * 3];
                new Random().NextBytes(noise);
                Marshal.Copy(noise, 0, data.Scan0, noise.Length);
                pic.UnlockBits(data);
                using (var graphics = Graphics.FromImage(pic))
                {
                    var stringFormat = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    graphics.DrawString(message, captchaFont, new SolidBrush(fColor), new RectangleF(0, 0, pic.Width, pic.Height), stringFormat);
                    var random = new Random((int)DateTime.Now.Ticks);
                    for (var i = 0; i < 30; i++)
                    {
                        var x0 = random.Next(0, width);
                        var y0 = random.Next(0, height);
                        var x1 = random.Next(0, width);
                        var y1 = random.Next(0, height);
                        graphics.DrawLine(Pens.White, x0, y0, x1, x1);
                    }
                }

                using (var stream = new MemoryStream())
                {
                    distortImage(height, width, pic);
                    pic.Save(stream, ImageFormat.Png);
                    return stream.ToArray();
                }
            }
        }

        private static Rectangle drawRoundedRectangle(Graphics gfx, Rectangle bounds, int cornerRadius, Pen drawPen, Color fillColor)
        {
            int strokeOffset = Convert.ToInt32(Math.Ceiling(drawPen.Width));
            bounds = Rectangle.Inflate(bounds, -strokeOffset, -strokeOffset);
            drawPen.EndCap = drawPen.StartCap = LineCap.Round;
            GraphicsPath gfxPath = new GraphicsPath();
            gfxPath.AddArc(bounds.X, bounds.Y, cornerRadius, cornerRadius, 180, 90);
            gfxPath.AddArc(bounds.X + bounds.Width - cornerRadius, bounds.Y, cornerRadius, cornerRadius, 270, 90);
            gfxPath.AddArc(bounds.X + bounds.Width - cornerRadius, bounds.Y + bounds.Height - cornerRadius, cornerRadius, cornerRadius, 0, 90);
            gfxPath.AddArc(bounds.X, bounds.Y + bounds.Height - cornerRadius, cornerRadius, cornerRadius, 90, 90);
            gfxPath.CloseAllFigures();
            gfx.FillPath(new SolidBrush(fillColor), gfxPath);
            gfx.DrawPath(drawPen, gfxPath);

            return bounds;
        }

        private static SizeF measureString(string text, Font f)
        {
            using (var bmp = new Bitmap(1, 1))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    return g.MeasureString(text, f);
                }
            }
        }

        private void distortImage(int height, int width, Bitmap pic)
        {
            using (var copy = (Bitmap)pic.Clone())
            {
                double distort = _randomNumberProvider.Next(1, 6) * (_randomNumberProvider.Next(10) == 1 ? 1 : -1);
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
        }
    }
}