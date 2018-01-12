
using DNTCaptcha.Core.Contracts;
using DNTCaptcha.Core.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System;

namespace DNTCaptcha.Core
{
    /// <summary>
    /// DNTCaptcha Image Controller
    /// </summary>
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public class DNTCaptchaImageController : Controller
    {
        private readonly ICaptchaImageProvider _captchaImageProvider;
        private readonly ICaptchaProtectionProvider _captchaProtectionProvider;
        private readonly ICaptchaStorageProvider _captchaStorageProvider;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly ILogger<DNTCaptchaImageController> _logger;

        /// <summary>
        /// DNTCaptcha Image Controller
        /// </summary>
        public DNTCaptchaImageController(
            ICaptchaImageProvider captchaImageProvider,
            ICaptchaProtectionProvider captchaProtectionProvider,
            ITempDataProvider tempDataProvider,
            ICaptchaStorageProvider captchaStorageProvider,
            ILogger<DNTCaptchaImageController> logger)
        {
            captchaImageProvider.CheckArgumentNull(nameof(captchaImageProvider));
            captchaProtectionProvider.CheckArgumentNull(nameof(captchaProtectionProvider));
            tempDataProvider.CheckArgumentNull(nameof(tempDataProvider));
            captchaStorageProvider.CheckArgumentNull(nameof(captchaStorageProvider));
            captchaStorageProvider.CheckArgumentNull(nameof(logger));

            _captchaImageProvider = captchaImageProvider;
            _captchaProtectionProvider = captchaProtectionProvider;
            _tempDataProvider = tempDataProvider;
            _captchaStorageProvider = captchaStorageProvider;
            _logger = logger;
        }

        /// <summary>
        /// The ViewContext Provider
        /// </summary>
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        /// <summary>
        /// Refresh the captcha
        /// </summary>
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true, Duration = 0)]
        public IActionResult Refresh(string rndDate, DNTCaptchaTagHelperHtmlAttributes model)
        {
            _captchaStorageProvider.Remove(HttpContext, model.CaptchaToken);

            var tagHelper = HttpContext.RequestServices.GetRequiredService<DNTCaptchaTagHelper>();
            tagHelper.BackColor = model.BackColor;
            tagHelper.FontName = model.FontName;
            tagHelper.FontSize = model.FontSize;
            tagHelper.ForeColor = model.ForeColor;
            tagHelper.Language = model.Language;
            tagHelper.Max = model.Max;
            tagHelper.Min = model.Min;
            tagHelper.Placeholder = model.Placeholder;
            tagHelper.TextBoxClass = model.TextBoxClass;
            tagHelper.TextBoxTemplate = model.TextBoxTemplate;
            tagHelper.ValidationErrorMessage = model.ValidationErrorMessage;
            tagHelper.ValidationMessageClass = model.ValidationMessageClass;
            tagHelper.RefreshButtonClass = model.RefreshButtonClass;

            var tagHelperContext = new TagHelperContext(
                allAttributes: new TagHelperAttributeList(),
                items: new Dictionary<object, object>(),
                uniqueId: Guid.NewGuid().ToString("N"));

            var tagHelperOutput = new TagHelperOutput(
                tagName: "div",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.SetContent(string.Empty);
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });
            tagHelper.ViewContext = ViewContext ?? new ViewContext(
                    new ActionContext(this.HttpContext, HttpContext.GetRouteData(), ControllerContext.ActionDescriptor),
                    new FakeView(),
                    new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                    {
                        Model = null
                    },
                    new TempDataDictionary(this.HttpContext, _tempDataProvider),
                    TextWriter.Null,
                    new HtmlHelperOptions());

            tagHelper.Process(tagHelperContext, tagHelperOutput);

            var attrs = new StringBuilder();
            foreach (var attr in tagHelperOutput.Attributes)
            {
                attrs.Append($" {attr.Name}='{attr.Value}'");
            }

            var content = $"<div {attrs}>{tagHelperOutput.Content.GetContent()}</div>";
            return Content(content);
        }

        /// <summary>
        /// Creates the captcha image.
        /// </summary>
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true, Duration = 0)]
        public IActionResult Show(string text, string rndDate, string foreColor = "#1B0172",
            string backColor = "", float fontSize = 12, string fontName = "Tahoma")
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return BadRequest();
            }

            var decryptedText = _captchaProtectionProvider.Decrypt(text);
            if (decryptedText == null)
            {
                return BadRequest();
            }

            byte[] image;
            try
            {
                image = _captchaImageProvider.DrawCaptcha(decryptedText, foreColor, backColor, fontSize, fontName);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(1001, ex, "DrawCaptcha error.");
                return BadRequest(ex.Message);
            }
            return new FileContentResult(_captchaImageProvider.DrawCaptcha(decryptedText, foreColor, backColor, fontSize, fontName), "image/png");
        }
    }
}
