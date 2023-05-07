using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using static System.FormattableString;

namespace DNTCaptcha.Core;

/// <summary>
///     DNTCaptcha TagHelper
/// </summary>
[HtmlTargetElement("dnt-captcha")]
public class DNTCaptchaTagHelper : DNTCaptchaTagHelperHtmlAttributes, ITagHelper
{
    private const string DataAjaxBeginFunctionName = "onRefreshButtonDataAjaxBegin";
    private readonly IAntiforgery _antiforgery;
    private readonly DNTCaptchaOptions _captchaOptions;
    private readonly ICaptchaCryptoProvider _captchaProtectionProvider;
    private readonly ICaptchaStorageProvider _captchaStorageProvider;
    private readonly Func<DisplayMode, ICaptchaTextProvider> _captchaTextProvider;
    private readonly IRandomNumberProvider _randomNumberProvider;
    private readonly ISerializationProvider _serializationProvider;
    private IUrlHelper? _urlHelper;

    /// <summary>
    ///     DNTCaptcha TagHelper
    /// </summary>
    public DNTCaptchaTagHelper(
        ICaptchaCryptoProvider captchaProtectionProvider,
        IRandomNumberProvider randomNumberProvider,
        Func<DisplayMode, ICaptchaTextProvider> captchaTextProvider,
        ICaptchaStorageProvider captchaStorageProvider,
        IAntiforgery antiforgery,
        ISerializationProvider serializationProvider,
        IOptions<DNTCaptchaOptions> options
    )
    {
        _captchaProtectionProvider = captchaProtectionProvider ??
                                     throw new ArgumentNullException(nameof(captchaProtectionProvider));
        _randomNumberProvider = randomNumberProvider ?? throw new ArgumentNullException(nameof(randomNumberProvider));
        _captchaTextProvider = captchaTextProvider ?? throw new ArgumentNullException(nameof(captchaTextProvider));
        _captchaStorageProvider =
            captchaStorageProvider ?? throw new ArgumentNullException(nameof(captchaStorageProvider));
        _antiforgery = antiforgery ?? throw new ArgumentNullException(nameof(antiforgery));
        _serializationProvider =
            serializationProvider ?? throw new ArgumentNullException(nameof(serializationProvider));
        _captchaOptions = options == null ? throw new ArgumentNullException(nameof(options)) : options.Value;
    }

    /// <summary>
    ///     The current ViewContext.
    /// </summary>
    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext? ViewContext { get; set; }

    /// <summary>
    ///     Default order is <c>0</c>.
    /// </summary>
    public int Order { get; }

    /// <summary>
    ///     Initializes the ITagHelperComponent with the given context. Additions to TagHelperContext.Items
    ///     should be done within this method to ensure they're added prior to executing the children.
    /// </summary>
    public void Init(TagHelperContext context)
    {
        // Part of the definition of the tag helper
    }

    /// <summary>
    ///     Asynchronously executes the <see cref="TagHelper" /> with the given <paramref name="context" /> and
    ///     <paramref name="output" />.
    /// </summary>
    /// <param name="context">Contains information associated with the current HTML tag.</param>
    /// <param name="output">A stateful HTML element used to generate an HTML tag.</param>
    /// <returns>A <see cref="Task" /> that on completion updates the <paramref name="output" />.</returns>
    /// <remarks>By default this calls into <see cref="Process" />.</remarks>
    /// .
    public Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        Process(context, output);
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Process the taghelper and generate the output.
    /// </summary>
    public void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (output == null)
        {
            throw new ArgumentNullException(nameof(output));
        }

        if (ViewContext == null)
        {
            throw new InvalidOperationException("`ViewContext` is null.");
        }

        setUrlHelper(ViewContext);

        output.TagName = "div";
        output.Attributes.Add("class", _captchaOptions.CaptchaClass);
        var captchaDivId =
            Invariant($"{_captchaOptions.CaptchaClass}{context.UniqueId}{_randomNumberProvider.NextNumber(Min, Max)}");
        output.Attributes.Add("id", captchaDivId);
        output.TagMode = TagMode.StartTagAndEndTag;

        var number = _randomNumberProvider.NextNumber(Min, Max);
        var randomText = _captchaTextProvider(DisplayMode).GetText(number, Language);
        var encryptedText = _captchaProtectionProvider.Encrypt(randomText);

        var captchaImage = getCaptchaImageTagBuilder(ViewContext, encryptedText);
        output.Content.AppendHtml(captchaImage);

        var cookieToken = $".{captchaDivId}";
        if (ShowRefreshButton)
        {
            var refreshButton = getRefreshButtonTagBuilder(ViewContext, captchaDivId, cookieToken);
            output.Content.AppendHtml(refreshButton);
        }

        var encryptedNumber = _captchaProtectionProvider.Encrypt(number.ToString(CultureInfo.InvariantCulture));
        var hiddenInput = getHiddenInputTagBuilder(encryptedNumber);
        output.Content.AppendHtml(hiddenInput);

        var textInput = getTextInputTagBuilder();
        output.Content
              .AppendHtml($"{string.Format(CultureInfo.InvariantCulture, TextBoxTemplate, textInput.GetString())}");

        var validationMessage = getValidationMessageTagBuilder(ViewContext);
        output.Content.AppendHtml(validationMessage);

        var hiddenInputToken = getHiddenInputTokenTagBuilder(_captchaProtectionProvider.Encrypt(cookieToken));
        output.Content.AppendHtml(hiddenInputToken);

        var dataAjaxBeginScript = getOnRefreshButtonDataAjaxBegin(ViewContext);
        output.Content.AppendHtml(dataAjaxBeginScript);

        _captchaStorageProvider.Add(ViewContext.HttpContext, cookieToken,
                                    number.ToString(CultureInfo.InvariantCulture));
    }

    private void setUrlHelper(ViewContext viewContext)
    {
        _urlHelper = viewContext.HttpContext.Items.Values.OfType<IUrlHelper>().FirstOrDefault();
        if (_urlHelper == null)
        {
            throw new InvalidOperationException("Failed to find the IUrlHelper of ViewContext.HttpContext.");
        }
    }

    private TagBuilder getHiddenInputTagBuilder(string encryptedText)
    {
        var hiddenInput = new TagBuilder("input")
                          {
                              TagRenderMode = TagRenderMode.SelfClosing,
                          };
        hiddenInput.Attributes.Add("id", _captchaOptions.CaptchaComponent.CaptchaHiddenInputName);
        hiddenInput.Attributes.Add("name", _captchaOptions.CaptchaComponent.CaptchaHiddenInputName);
        hiddenInput.Attributes.Add("type", "hidden");
        hiddenInput.Attributes.Add("value", encryptedText);
        return hiddenInput;
    }

    private TagBuilder getHiddenInputTokenTagBuilder(string token)
    {
        var hiddenInput = new TagBuilder("input")
                          {
                              TagRenderMode = TagRenderMode.SelfClosing,
                          };
        hiddenInput.Attributes.Add("id", _captchaOptions.CaptchaComponent.CaptchaHiddenTokenName);
        hiddenInput.Attributes.Add("name", _captchaOptions.CaptchaComponent.CaptchaHiddenTokenName);
        hiddenInput.Attributes.Add("type", "hidden");
        hiddenInput.Attributes.Add("value", token);
        return hiddenInput;
    }

    private TagBuilder getCaptchaImageTagBuilder(ViewContext viewContext, string encryptedText)
    {
        if (_urlHelper == null)
        {
            throw new InvalidOperationException("Failed to find the IUrlHelper of ViewContext.HttpContext.");
        }

        var values = new CaptchaImageParams
                     {
                         Text = encryptedText,
                         RndDate = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture),
                         ForeColor = ForeColor,
                         BackColor = BackColor,
                         FontSize = FontSize,
                         FontName = FontName,
                     };
        var serializedValues = _serializationProvider.Serialize(values);
        var encryptSerializedValues = _captchaProtectionProvider.Encrypt(serializedValues);
        var actionUrl = UseRelativeUrls
                            ? _urlHelper.Action(
                                                nameof(DNTCaptchaImageController.Show),
                                                nameof(DNTCaptchaImageController)
                                                    .Replace("Controller", string.Empty, StringComparison.Ordinal),
                                                new { data = encryptSerializedValues, area = "" })
                            : _urlHelper.Action(
                                                nameof(DNTCaptchaImageController.Show),
                                                nameof(DNTCaptchaImageController)
                                                    .Replace("Controller", string.Empty, StringComparison.Ordinal),
                                                new { data = encryptSerializedValues, area = "" },
                                                viewContext.HttpContext.Request.Scheme);

        if (string.IsNullOrWhiteSpace(actionUrl))
        {
            throw new
                InvalidOperationException("It's not possible to determine the URL of the `DNTCaptchaImageController.Show` method. Please register the `services.AddControllers()` and `endpoints.MapControllerRoute(...)`.");
        }

        var captchaImage = new TagBuilder("img");
        var dntCaptchaImg = $"{_captchaOptions.CaptchaClass}Img";
        captchaImage.TagRenderMode = TagRenderMode.SelfClosing;
        captchaImage.Attributes.Add("id", dntCaptchaImg);
        captchaImage.Attributes.Add("name", dntCaptchaImg);
        captchaImage.Attributes.Add("alt", "captcha");
        captchaImage.Attributes.Add("src", actionUrl);
        captchaImage.Attributes.Add("style", "margin-bottom: 4px;");
        return captchaImage;
    }

    private TagBuilder getRefreshButtonTagBuilder(ViewContext viewContext, string captchaDivId, string captchaToken)
    {
        if (_urlHelper == null)
        {
            throw new InvalidOperationException("Failed to find the IUrlHelper of ViewContext.HttpContext.");
        }

        var values = new RefreshData
                     {
                         RndDate = DateTime.Now.Ticks,
                         BackColor = BackColor,
                         FontName = FontName,
                         FontSize = FontSize,
                         ForeColor = ForeColor,
                         Language = Language,
                         Max = Max,
                         Min = Min,
                         Placeholder = Placeholder,
                         TextBoxClass = TextBoxClass,
                         TextBoxTemplate = TextBoxTemplate,
                         ValidationErrorMessage = ValidationErrorMessage,
                         ValidationMessageClass = ValidationMessageClass,
                         CaptchaToken = captchaToken,
                         RefreshButtonClass = RefreshButtonClass,
                         DisplayMode = DisplayMode,
                         UseRelativeUrls = UseRelativeUrls,
                         ShowRefreshButton = ShowRefreshButton,
                     };
        var serializedValues = _serializationProvider.Serialize(values);
        var encryptSerializedValues = _captchaProtectionProvider.Encrypt(serializedValues);
        var actionUrl = UseRelativeUrls
                            ? _urlHelper.Action(
                                                nameof(DNTCaptchaImageController.Refresh),
                                                nameof(DNTCaptchaImageController)
                                                    .Replace("Controller", string.Empty, StringComparison.Ordinal),
                                                new { data = encryptSerializedValues, area = "" })
                            : _urlHelper.Action(
                                                nameof(DNTCaptchaImageController.Refresh),
                                                nameof(DNTCaptchaImageController)
                                                    .Replace("Controller", string.Empty, StringComparison.Ordinal),
                                                new { data = encryptSerializedValues, area = "" },
                                                viewContext.HttpContext.Request.Scheme);

        var refreshButton = new TagBuilder("a");
        var dntCaptchaRefreshButton = $"{_captchaOptions.CaptchaClass}RefreshButton";
        refreshButton.Attributes.Add("id", dntCaptchaRefreshButton);
        refreshButton.Attributes.Add("name", dntCaptchaRefreshButton);
        refreshButton.Attributes.Add("href", "#refresh");
        refreshButton.Attributes.Add("data-ajax-url", actionUrl);
        refreshButton.Attributes.Add("data-ajax", "true");
        refreshButton.Attributes.Add("data-ajax-method", "POST");
        refreshButton.Attributes.Add("data-ajax-mode", "replace-with");
        refreshButton.Attributes.Add("data-ajax-update", $"#{captchaDivId}");
        refreshButton.Attributes.Add("data-ajax-begin", DataAjaxBeginFunctionName);
        refreshButton.Attributes.Add("class", RefreshButtonClass);
        return refreshButton;
    }

    private TagBuilder getOnRefreshButtonDataAjaxBegin(ViewContext viewContext)
    {
        var requestVerificationToken = _antiforgery.GetAndStoreTokens(viewContext.HttpContext).RequestToken;
        var script = new TagBuilder("script");
        script.Attributes.Add("type", "text/javascript");
        script.InnerHtml.AppendHtml(
                                    $"function {DataAjaxBeginFunctionName}(xhr, settings) {{ settings.data = settings.data + '&__RequestVerificationToken={requestVerificationToken}';  }}");
        return script;
    }

    private TagBuilder getTextInputTagBuilder()
    {
        var textInput = new TagBuilder("input")
                        {
                            TagRenderMode = TagRenderMode.SelfClosing,
                        };
        textInput.Attributes.Add("id", _captchaOptions.CaptchaComponent.CaptchaInputName);
        textInput.Attributes.Add("name", _captchaOptions.CaptchaComponent.CaptchaInputName);
        textInput.Attributes.Add("autocomplete", "off");
        textInput.Attributes.Add("class", TextBoxClass);
        textInput.Attributes.Add("data-val", "true");
        textInput.Attributes.Add("data-val-required", ValidationErrorMessage);
        textInput.Attributes.Add("required", "required");
        textInput.Attributes.Add("data-required-msg", ValidationErrorMessage);
        textInput.Attributes.Add("placeholder", Placeholder);
        textInput.Attributes.Add("dir", Dir);
        textInput.Attributes.Add("type", "text");
        textInput.Attributes.Add("value", "");
        return textInput;
    }

    private TagBuilder getValidationMessageTagBuilder(ViewContext viewContext)
    {
        var validationMessage = new TagBuilder("span");
        validationMessage.Attributes.Add("class", ValidationMessageClass);
        validationMessage.Attributes.Add("data-valmsg-for", _captchaOptions.CaptchaComponent.CaptchaInputName);
        validationMessage.Attributes.Add("data-valmsg-replace", "true");

        if (!viewContext.ModelState.IsValid
            && viewContext.ModelState.TryGetValue(_captchaOptions.CaptchaComponent.CaptchaInputName,
                                                  out var captchaInputNameValidationState)
            && captchaInputNameValidationState.ValidationState == ModelValidationState.Invalid)
        {
            var error = captchaInputNameValidationState.Errors.FirstOrDefault();
            if (error != null)
            {
                var errorSpan = new TagBuilder("span");
                errorSpan.InnerHtml.AppendHtml(error.ErrorMessage);
                validationMessage.InnerHtml.AppendHtml(errorSpan);
            }
        }

        return validationMessage;
    }
}