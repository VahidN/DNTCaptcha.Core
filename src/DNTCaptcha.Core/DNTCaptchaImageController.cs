using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
#if NET7_0 || NET8_0 || NET9_0
using Microsoft.AspNetCore.RateLimiting;
#endif

namespace DNTCaptcha.Core;

/// <summary>
///     DNTCaptcha Image Controller
/// </summary>
/// <remarks>
///     DNTCaptcha Image Controller
/// </remarks>
[AllowAnonymous]
#if NET7_0 || NET8_0 || NET9_0
[EnableRateLimiting(DNTCaptchaRateLimiterPolicy.Name)]
#endif
public class DNTCaptchaImageController(
    ICaptchaImageProvider captchaImageProvider,
    ICaptchaCryptoProvider captchaProtectionProvider,
    ITempDataProvider tempDataProvider,
    ICaptchaStorageProvider captchaStorageProvider,
    ILogger<DNTCaptchaImageController> logger,
    ISerializationProvider serializationProvider,
    IOptions<DNTCaptchaOptions> options) : Controller
{
    private const string TheReceivedDataIsNullOrEmpty = "The received data is null or empty.";

    private const string CouldntDecryptTheReceivedData =
        "Couldn't decrypt the received data. Probably it's malformed or changed/destroyed.";

    private const string IsYourNetworkDistributed =
        "Couldn't deserialize the model. Are you on a distributed environment? If yes, please read the `How to choose a correct storage mode` in the readme file.";

    private const string TurnOnTheLogDebugLevel =
        "Turn on the `LogDebug` level, to see the actual details of the exception, in the logs.";

    private readonly ICaptchaImageProvider _captchaImageProvider =
        captchaImageProvider ?? throw new ArgumentNullException(nameof(captchaImageProvider));

    private readonly ICaptchaCryptoProvider _captchaProtectionProvider = captchaProtectionProvider ??
                                                                         throw new ArgumentNullException(
                                                                             nameof(captchaProtectionProvider));

    private readonly ICaptchaStorageProvider _captchaStorageProvider =
        captchaStorageProvider ?? throw new ArgumentNullException(nameof(captchaStorageProvider));

    private readonly ILogger<DNTCaptchaImageController> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    private readonly DNTCaptchaOptions _options =
        options == null ? throw new ArgumentNullException(nameof(options)) : options.Value;

    private readonly ISerializationProvider _serializationProvider =
        serializationProvider ?? throw new ArgumentNullException(nameof(serializationProvider));

    private readonly ITempDataProvider _tempDataProvider =
        tempDataProvider ?? throw new ArgumentNullException(nameof(tempDataProvider));

    /// <summary>
    ///     The ViewContext Provider
    /// </summary>
    [ViewContext]
    public ViewContext? ViewContext { get; set; }

    /// <summary>
    ///     Refresh the captcha
    /// </summary>
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true, Duration = 0)]
    [HttpGet(template: "[action]")]
    [HttpPost(template: "[action]")]
    public IActionResult Refresh(string data)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return BadRequest(TheReceivedDataIsNullOrEmpty);
            }

            var decryptedModel = _captchaProtectionProvider.Decrypt(data);

            if (decryptedModel == null)
            {
                return BadRequest(CouldntDecryptTheReceivedData);
            }

            var model = _serializationProvider.Deserialize<DNTCaptchaTagHelperHtmlAttributes>(decryptedModel);

            if (model == null)
            {
                return BadRequest(IsYourNetworkDistributed);
            }

            InvalidateToken(model);

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
            tagHelper.TooManyRequestsErrorMessage = model.TooManyRequestsErrorMessage;
            tagHelper.ValidationMessageClass = model.ValidationMessageClass;
            tagHelper.RefreshButtonClass = model.RefreshButtonClass;
            tagHelper.DisplayMode = model.DisplayMode;
            tagHelper.UseRelativeUrls = model.UseRelativeUrls;
            tagHelper.ShowRefreshButton = model.ShowRefreshButton;

            var tagHelperContext = new TagHelperContext([], new Dictionary<object, object>
            {
                {
                    typeof(IUrlHelper), Url
                }
            }, Guid.NewGuid().ToString(format: "N"));

            var tagHelperOutput = new TagHelperOutput(tagName: "div", [], (useCachedResult, encoder) =>
            {
                var tagHelperContent = new DefaultTagHelperContent();
                tagHelperContent.SetContent(string.Empty);

                return Task.FromResult<TagHelperContent>(tagHelperContent);
            });

            tagHelper.ViewContext = ViewContext ?? new ViewContext(
                new ActionContext(HttpContext, HttpContext.GetRouteData(), ControllerContext.ActionDescriptor),
                new FakeView(), new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = null
                }, new TempDataDictionary(HttpContext, _tempDataProvider), TextWriter.Null, new HtmlHelperOptions());

            tagHelper.Process(tagHelperContext, tagHelperOutput);

            var attrs = new StringBuilder();

            foreach (var attr in tagHelperOutput.Attributes)
            {
                attrs.Append(value: ' ').Append(attr.Name).Append(value: "='").Append(attr.Value).Append(value: '\'');
            }

            var content = $"<div {attrs}>{tagHelperOutput.Content.GetContent()}</div>";

            return Content(content);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, message: "Failed to refresh the captcha image.");

            return _options.ShowExceptions ? BadRequest(ex.ToString()) : BadRequest(TurnOnTheLogDebugLevel);
        }
    }

    private void InvalidateToken(DNTCaptchaTagHelperHtmlAttributes model)
        => _captchaStorageProvider.Remove(HttpContext, model.CaptchaToken);

    /// <summary>
    ///     Creates the captcha image.
    /// </summary>
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true, Duration = 0)]
    [HttpGet(template: "[action]")]
    [HttpPost(template: "[action]")]
    public IActionResult Show(string data)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return BadRequest(TheReceivedDataIsNullOrEmpty);
            }

            var decryptedModel = _captchaProtectionProvider.Decrypt(data);

            if (decryptedModel == null)
            {
                return BadRequest(CouldntDecryptTheReceivedData);
            }

            var model = _serializationProvider.Deserialize<CaptchaImageParams>(decryptedModel);

            if (model == null)
            {
                return BadRequest(IsYourNetworkDistributed);
            }

            var decryptedText = _captchaProtectionProvider.Decrypt(model.Text);

            if (decryptedText == null)
            {
                return BadRequest(error: "Couldn't decrypt the text.");
            }

            var image = _captchaImageProvider.DrawCaptcha(decryptedText, model.ForeColor, model.BackColor,
                model.FontSize, model.FontName);

            return new FileContentResult(image, contentType: "image/png");
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, message: "Failed to show the captcha image.");

            return _options.ShowExceptions ? BadRequest(ex.ToString()) : BadRequest(TurnOnTheLogDebugLevel);
        }
    }
}