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
/// <remarks>
///     DNTCaptcha TagHelper
/// </remarks>
[HtmlTargetElement(tag: "dnt-captcha")]
public class DNTCaptchaTagHelper(
    ICaptchaCryptoProvider captchaProtectionProvider,
    IRandomNumberProvider randomNumberProvider,
    Func<DisplayMode, ICaptchaTextProvider> captchaTextProvider,
    ICaptchaStorageProvider captchaStorageProvider,
    IAntiforgery antiforgery,
    ISerializationProvider serializationProvider,
    IOptions<DNTCaptchaOptions> options) : DNTCaptchaTagHelperHtmlAttributes, ITagHelper
{
    private const string DataAjaxBeginFunctionName = "onRefreshButtonDataAjaxBegin";
    private const string DataAjaxFailureFunctionName = "onRefreshButtonDataAjaxFailure";
    private readonly IAntiforgery _antiforgery = antiforgery ?? throw new ArgumentNullException(nameof(antiforgery));

    private readonly DNTCaptchaOptions _captchaOptions =
        options == null ? throw new ArgumentNullException(nameof(options)) : options.Value;

    private readonly ICaptchaCryptoProvider _captchaProtectionProvider = captchaProtectionProvider ??
                                                                         throw new ArgumentNullException(
                                                                             nameof(captchaProtectionProvider));

    private readonly ICaptchaStorageProvider _captchaStorageProvider =
        captchaStorageProvider ?? throw new ArgumentNullException(nameof(captchaStorageProvider));

    private readonly Func<DisplayMode, ICaptchaTextProvider> _captchaTextProvider =
        captchaTextProvider ?? throw new ArgumentNullException(nameof(captchaTextProvider));

    private readonly IRandomNumberProvider _randomNumberProvider =
        randomNumberProvider ?? throw new ArgumentNullException(nameof(randomNumberProvider));

    private readonly ISerializationProvider _serializationProvider =
        serializationProvider ?? throw new ArgumentNullException(nameof(serializationProvider));

    private IUrlHelper? _urlHelper;

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
            throw new InvalidOperationException(message: "`ViewContext` is null.");
        }

        SetUrlHelper(ViewContext);

        output.TagName = "div";
        output.Attributes.Add(name: "class", _captchaOptions.CaptchaClass);

        var captchaDivId =
            Invariant($"{_captchaOptions.CaptchaClass}{context.UniqueId}{_randomNumberProvider.NextNumber(Min, Max)}");

        output.Attributes.Add(name: "id", captchaDivId);
        output.TagMode = TagMode.StartTagAndEndTag;

        var number = _randomNumberProvider.NextNumber(Min, Max);
        var randomText = _captchaTextProvider(DisplayMode).GetText(number, Language);
        var encryptedText = _captchaProtectionProvider.Encrypt(randomText);

        var captchaImage = GetCaptchaImageTagBuilder(ViewContext, encryptedText);
        output.Content.AppendHtml(captchaImage);

        var cookieToken = $".{captchaDivId}";

        if (ShowRefreshButton)
        {
            var refreshButton = GetRefreshButtonTagBuilder(ViewContext, captchaDivId, cookieToken);
            output.Content.AppendHtml(refreshButton);
        }

        var encryptedNumber = _captchaProtectionProvider.Encrypt(number.ToString(CultureInfo.InvariantCulture));
        var hiddenInput = GetHiddenInputTagBuilder(encryptedNumber);
        output.Content.AppendHtml(hiddenInput);

        var textInput = GetTextInputTagBuilder();

        output.Content.AppendHtml(
            $"{string.Format(CultureInfo.InvariantCulture, TextBoxTemplate, textInput.GetString())}");

        var validationMessage = GetValidationMessageTagBuilder(ViewContext);
        output.Content.AppendHtml(validationMessage);

        var hiddenInputToken = GetHiddenInputTokenTagBuilder(_captchaProtectionProvider.Encrypt(cookieToken));
        output.Content.AppendHtml(hiddenInputToken);

        var dataAjaxScripts = GetOnRefreshButtonDataAjaxScripts(ViewContext);
        output.Content.AppendHtml(dataAjaxScripts);

        _captchaStorageProvider.Add(ViewContext.HttpContext, cookieToken,
            number.ToString(CultureInfo.InvariantCulture));
    }

    private void SetUrlHelper(ViewContext viewContext)
    {
        _urlHelper = viewContext.HttpContext.Items.Values.OfType<IUrlHelper>().FirstOrDefault();

        if (_urlHelper == null)
        {
            throw new InvalidOperationException(message: "Failed to find the IUrlHelper of ViewContext.HttpContext.");
        }
    }

    private TagBuilder GetHiddenInputTagBuilder(string encryptedText)
    {
        var hiddenInput = new TagBuilder(tagName: "input")
        {
            TagRenderMode = TagRenderMode.SelfClosing
        };

        hiddenInput.Attributes.Add(key: "id", _captchaOptions.CaptchaComponent.CaptchaHiddenInputName);
        hiddenInput.Attributes.Add(key: "name", _captchaOptions.CaptchaComponent.CaptchaHiddenInputName);
        hiddenInput.Attributes.Add(key: "type", value: "hidden");
        hiddenInput.Attributes.Add(key: "value", encryptedText);

        return hiddenInput;
    }

    private TagBuilder GetHiddenInputTokenTagBuilder(string token)
    {
        var hiddenInput = new TagBuilder(tagName: "input")
        {
            TagRenderMode = TagRenderMode.SelfClosing
        };

        hiddenInput.Attributes.Add(key: "id", _captchaOptions.CaptchaComponent.CaptchaHiddenTokenName);
        hiddenInput.Attributes.Add(key: "name", _captchaOptions.CaptchaComponent.CaptchaHiddenTokenName);
        hiddenInput.Attributes.Add(key: "type", value: "hidden");
        hiddenInput.Attributes.Add(key: "value", token);

        return hiddenInput;
    }

    private TagBuilder GetCaptchaImageTagBuilder(ViewContext viewContext, string encryptedText)
    {
        if (_urlHelper == null)
        {
            throw new InvalidOperationException(message: "Failed to find the IUrlHelper of ViewContext.HttpContext.");
        }

        var values = new CaptchaImageParams
        {
            Text = encryptedText,
            RndDate = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture),
            ForeColor = ForeColor,
            BackColor = BackColor,
            FontSize = FontSize,
            FontName = FontName
        };

        var serializedValues = _serializationProvider.Serialize(values);
        var encryptSerializedValues = _captchaProtectionProvider.Encrypt(serializedValues);

        var actionUrl = UseRelativeUrls
            ? _urlHelper.Action(nameof(DNTCaptchaImageController.Show),
                nameof(DNTCaptchaImageController)
                    .Replace(oldValue: "Controller", string.Empty, StringComparison.Ordinal), new
                {
                    data = encryptSerializedValues,
                    area = ""
                })
            : _urlHelper.Action(nameof(DNTCaptchaImageController.Show),
                nameof(DNTCaptchaImageController)
                    .Replace(oldValue: "Controller", string.Empty, StringComparison.Ordinal), new
                {
                    data = encryptSerializedValues,
                    area = ""
                }, viewContext.HttpContext.Request.Scheme);

        if (string.IsNullOrWhiteSpace(actionUrl))
        {
            throw new InvalidOperationException(
                message:
                "It's not possible to determine the URL of the `DNTCaptchaImageController.Show` method. Please register the `services.AddControllers()` and `endpoints.MapControllerRoute(...)`.");
        }

        var captchaImage = new TagBuilder(tagName: "img");
        var dntCaptchaImg = $"{_captchaOptions.CaptchaClass}Img";
        captchaImage.TagRenderMode = TagRenderMode.SelfClosing;
        captchaImage.Attributes.Add(key: "id", dntCaptchaImg);
        captchaImage.Attributes.Add(key: "name", dntCaptchaImg);
        captchaImage.Attributes.Add(key: "alt", value: "captcha");
        captchaImage.Attributes.Add(key: "src", actionUrl);
        captchaImage.Attributes.Add(key: "style", value: "margin-bottom: 4px;");

        return captchaImage;
    }

    private TagBuilder GetRefreshButtonTagBuilder(ViewContext viewContext, string captchaDivId, string captchaToken)
    {
        if (_urlHelper == null)
        {
            throw new InvalidOperationException(message: "Failed to find the IUrlHelper of ViewContext.HttpContext.");
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
            TooManyRequestsErrorMessage = TooManyRequestsErrorMessage,
            ValidationMessageClass = ValidationMessageClass,
            CaptchaToken = captchaToken,
            RefreshButtonClass = RefreshButtonClass,
            DisplayMode = DisplayMode,
            UseRelativeUrls = UseRelativeUrls,
            ShowRefreshButton = ShowRefreshButton
        };

        var serializedValues = _serializationProvider.Serialize(values);
        var encryptSerializedValues = _captchaProtectionProvider.Encrypt(serializedValues);

        var actionUrl = UseRelativeUrls
            ? _urlHelper.Action(nameof(DNTCaptchaImageController.Refresh),
                nameof(DNTCaptchaImageController)
                    .Replace(oldValue: "Controller", string.Empty, StringComparison.Ordinal), new
                {
                    data = encryptSerializedValues,
                    area = ""
                })
            : _urlHelper.Action(nameof(DNTCaptchaImageController.Refresh),
                nameof(DNTCaptchaImageController)
                    .Replace(oldValue: "Controller", string.Empty, StringComparison.Ordinal), new
                {
                    data = encryptSerializedValues,
                    area = ""
                }, viewContext.HttpContext.Request.Scheme);

        var refreshButton = new TagBuilder(tagName: "a");
        var dntCaptchaRefreshButton = $"{_captchaOptions.CaptchaClass}RefreshButton";
        refreshButton.Attributes.Add(key: "id", dntCaptchaRefreshButton);
        refreshButton.Attributes.Add(key: "name", dntCaptchaRefreshButton);
        refreshButton.Attributes.Add(key: "href", value: "#refresh");
        refreshButton.Attributes.Add(key: "data-ajax-url", actionUrl);
        refreshButton.Attributes.Add(key: "data-ajax", value: "true");
        refreshButton.Attributes.Add(key: "data-ajax-method", value: "POST");
        refreshButton.Attributes.Add(key: "data-ajax-mode", value: "replace-with");
        refreshButton.Attributes.Add(key: "data-ajax-update", $"#{captchaDivId}");
        refreshButton.Attributes.Add(key: "data-ajax-begin", DataAjaxBeginFunctionName);
        refreshButton.Attributes.Add(key: "data-ajax-failure", DataAjaxFailureFunctionName);
        refreshButton.Attributes.Add(key: "class", RefreshButtonClass);

        return refreshButton;
    }

    private DNTScriptTag GetOnRefreshButtonDataAjaxScripts(ViewContext viewContext)
    {
        var requestVerificationToken = _antiforgery.GetAndStoreTokens(viewContext.HttpContext).RequestToken;

        return new DNTScriptTag(
            $" function {DataAjaxBeginFunctionName}(xhr, settings) {{ settings.data = settings.data + '&__RequestVerificationToken={requestVerificationToken}';  }}" +
            $" function {DataAjaxFailureFunctionName}(xhr, status, error) {{ if(xhr.status === 429) alert('{TooManyRequestsErrorMessage}'); }}",
            GetNonceValue(viewContext));
    }

    private string GetNonceValue(ViewContext viewContext)
        => viewContext.HttpContext.Items.TryGetValue(_captchaOptions.NonceKey, out var value)
            ? value as string ?? string.Empty
            : string.Empty;

    private TagBuilder GetTextInputTagBuilder()
    {
        var textInput = new TagBuilder(tagName: "input")
        {
            TagRenderMode = TagRenderMode.SelfClosing
        };

        textInput.Attributes.Add(key: "id", _captchaOptions.CaptchaComponent.CaptchaInputName);
        textInput.Attributes.Add(key: "name", _captchaOptions.CaptchaComponent.CaptchaInputName);
        textInput.Attributes.Add(key: "autocomplete", value: "off");
        textInput.Attributes.Add(key: "class", TextBoxClass);
        textInput.Attributes.Add(key: "data-val", value: "true");
        textInput.Attributes.Add(key: "data-val-required", ValidationErrorMessage);
        textInput.Attributes.Add(key: "required", value: "required");
        textInput.Attributes.Add(key: "data-required-msg", ValidationErrorMessage);
        textInput.Attributes.Add(key: "placeholder", Placeholder);
        textInput.Attributes.Add(key: "dir", Dir);
        textInput.Attributes.Add(key: "type", value: "text");
        textInput.Attributes.Add(key: "value", value: "");

        return textInput;
    }

    private TagBuilder GetValidationMessageTagBuilder(ViewContext viewContext)
    {
        var validationMessage = new TagBuilder(tagName: "span");
        validationMessage.Attributes.Add(key: "class", ValidationMessageClass);
        validationMessage.Attributes.Add(key: "data-valmsg-for", _captchaOptions.CaptchaComponent.CaptchaInputName);
        validationMessage.Attributes.Add(key: "data-valmsg-replace", value: "true");

        if (!viewContext.ModelState.IsValid &&
            viewContext.ModelState.TryGetValue(_captchaOptions.CaptchaComponent.CaptchaInputName,
                out var captchaInputNameValidationState) &&
            captchaInputNameValidationState.ValidationState == ModelValidationState.Invalid)
        {
            var error = captchaInputNameValidationState.Errors.FirstOrDefault();

            if (error != null)
            {
                var errorSpan = new TagBuilder(tagName: "span");
                errorSpan.InnerHtml.AppendHtml(error.ErrorMessage);
                validationMessage.InnerHtml.AppendHtml(errorSpan);
            }
        }

        return validationMessage;
    }
}