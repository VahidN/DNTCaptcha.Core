﻿using System;
using System.Linq;
using System.Threading.Tasks;
using DNTCaptcha.Core.Contracts;
using DNTCaptcha.Core.Providers;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace DNTCaptcha.Core
{
    /// <summary>
    /// DNTCaptcha TagHelper
    /// </summary>
    [HtmlTargetElement("dnt-captcha")]
    public class DNTCaptchaTagHelper : DNTCaptchaTagHelperHtmlAttributes, ITagHelper
    {
        /// <summary>
        /// The default hidden input name of the captcha.
        /// </summary>
        public static readonly string CaptchaHiddenInputName = "DNTCaptchaText";

        /// <summary>
        /// The default hidden input name of the captcha's cookie token.
        /// </summary>
        public static readonly string CaptchaHiddenTokenName = "DNTCaptchaToken";

        /// <summary>
        /// The default input name of the captcha.
        /// </summary>
        public static readonly string CaptchaInputName = "DNTCaptchaInputText";

        private const string DataAjaxBeginFunctionName = "onRefreshButtonDataAjaxBegin";
        private readonly ICaptchaCryptoProvider _captchaProtectionProvider;
        private readonly ICaptchaStorageProvider _captchaStorageProvider;
        private readonly Func<DisplayMode, ICaptchaTextProvider> _captchaTextProvider;
        private readonly IRandomNumberProvider _randomNumberProvider;
        private readonly IAntiforgery _antiforgery;
        private IUrlHelper _urlHelper;
        private readonly ISerializationProvider _serializationProvider;

        /// <summary>
        /// DNTCaptcha TagHelper
        /// </summary>
        public DNTCaptchaTagHelper(
            ICaptchaCryptoProvider captchaProtectionProvider,
            IRandomNumberProvider randomNumberProvider,
            Func<DisplayMode, ICaptchaTextProvider> captchaTextProvider,
            ICaptchaStorageProvider captchaStorageProvider,
            IAntiforgery antiforgery,
            ISerializationProvider serializationProvider
            )
        {
            captchaProtectionProvider.CheckArgumentNull(nameof(captchaProtectionProvider));
            randomNumberProvider.CheckArgumentNull(nameof(randomNumberProvider));
            captchaTextProvider.CheckArgumentNull(nameof(captchaTextProvider));
            captchaStorageProvider.CheckArgumentNull(nameof(captchaStorageProvider));
            antiforgery.CheckArgumentNull(nameof(antiforgery));
            serializationProvider.CheckArgumentNull(nameof(serializationProvider));

            _captchaProtectionProvider = captchaProtectionProvider;
            _randomNumberProvider = randomNumberProvider;
            _captchaTextProvider = captchaTextProvider;
            _captchaStorageProvider = captchaStorageProvider;
            _antiforgery = antiforgery;
            _serializationProvider = serializationProvider;
        }

        /// <summary>
        /// Default order is <c>0</c>.
        /// </summary>
        public int Order { get; } = 0;

        /// <summary>
        /// The current ViewContext.
        /// </summary>
        [ViewContext, HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        /// <inheritdoc />
        public void Init(TagHelperContext context)
        {
        }

        /// <summary>
        /// Process the taghelper and generate the output.
        /// </summary>
        public void Process(TagHelperContext context, TagHelperOutput output)
        {
            context.CheckArgumentNull(nameof(context));
            output.CheckArgumentNull(nameof(output));

            setUrlHelper();

            output.TagName = "div";
            output.Attributes.Add("class", "dntCaptcha");
            var captchaDivId = $"dntCaptcha{context.UniqueId}{_randomNumberProvider.Next(Min, Max)}";
            output.Attributes.Add("id", captchaDivId);
            output.TagMode = TagMode.StartTagAndEndTag;

            var number = _randomNumberProvider.Next(Min, Max);
            var randomText = _captchaTextProvider(DisplayMode).GetText(number, Language);
            var encryptedText = _captchaProtectionProvider.Encrypt(randomText);

            var captchaImage = getCaptchaImageTagBuilder(encryptedText);
            output.Content.AppendHtml(captchaImage);

            var cookieToken = $".{captchaDivId}";
            var refreshButton = getRefreshButtonTagBuilder(captchaDivId, cookieToken);
            output.Content.AppendHtml(refreshButton);

            var hiddenInput = getHiddenInputTagBuilder(encryptedText);
            output.Content.AppendHtml(hiddenInput);

            var textInput = getTextInputTagBuilder();
            output.Content.AppendHtml($"{string.Format(TextBoxTemplate, textInput.GetString())}");

            var validationMessage = getValidationMessageTagBuilder();
            output.Content.AppendHtml(validationMessage);

            var hiddenInputToken = getHiddenInputTokenTagBuilder(_captchaProtectionProvider.Encrypt(cookieToken));
            output.Content.AppendHtml(hiddenInputToken);

            var dataAjaxBeginScript = getOnRefreshButtonDataAjaxBegin();
            output.Content.AppendHtml(dataAjaxBeginScript);

            _captchaStorageProvider.Add(ViewContext.HttpContext, cookieToken, randomText);
        }

        private void setUrlHelper()
        {
            _urlHelper = ViewContext.HttpContext.Items.Values.OfType<IUrlHelper>().FirstOrDefault();
            if (_urlHelper == null)
            {
                throw new NullReferenceException("Failed to find the IUrlHelper of ViewContext.HttpContext.");
            }
        }

        /// <summary>
        /// Asynchronously executes the <see cref="TagHelper"/> with the given <paramref name="context"/> and
        /// <paramref name="output"/>.
        /// </summary>
        /// <param name="context">Contains information associated with the current HTML tag.</param>
        /// <param name="output">A stateful HTML element used to generate an HTML tag.</param>
        /// <returns>A <see cref="Task"/> that on completion updates the <paramref name="output"/>.</returns>
        /// <remarks>By default this calls into <see cref="Process"/>.</remarks>.
        public Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            Process(context, output);
            return Task.CompletedTask;
        }

        private static TagBuilder getHiddenInputTagBuilder(string encryptedText)
        {
            var hiddenInput = new TagBuilder("input");
            hiddenInput.TagRenderMode = TagRenderMode.SelfClosing;
            hiddenInput.Attributes.Add("id", CaptchaHiddenInputName);
            hiddenInput.Attributes.Add("name", CaptchaHiddenInputName);
            hiddenInput.Attributes.Add("type", "hidden");
            hiddenInput.Attributes.Add("value", encryptedText);
            return hiddenInput;
        }

        private static TagBuilder getHiddenInputTokenTagBuilder(string token)
        {
            var hiddenInput = new TagBuilder("input");
            hiddenInput.TagRenderMode = TagRenderMode.SelfClosing;
            hiddenInput.Attributes.Add("id", CaptchaHiddenTokenName);
            hiddenInput.Attributes.Add("name", CaptchaHiddenTokenName);
            hiddenInput.Attributes.Add("type", "hidden");
            hiddenInput.Attributes.Add("value", token);
            return hiddenInput;
        }

        private TagBuilder getCaptchaImageTagBuilder(string encryptedText)
        {
            ViewContext.CheckArgumentNull(nameof(ViewContext));

            var values = new CaptchaImageParams
            {
                Text = encryptedText,
                RndDate = DateTime.Now.Ticks.ToString(),
                ForeColor = ForeColor,
                BackColor = BackColor,
                FontSize = FontSize,
                FontName = FontName,
                UseNoise = UseNoise
            };
            var serializedValues = _serializationProvider.Serialize(values);
            var encryptSerializedValues = _captchaProtectionProvider.Encrypt(serializedValues);
            var actionUrl = UseRelativeUrls ?
            _urlHelper.Action(
                            action: nameof(DNTCaptchaImageController.Show),
                            controller: nameof(DNTCaptchaImageController).Replace("Controller", string.Empty),
                            values: new { data = encryptSerializedValues, area = "" }) :
            _urlHelper.Action(
                            action: nameof(DNTCaptchaImageController.Show),
                            controller: nameof(DNTCaptchaImageController).Replace("Controller", string.Empty),
                            values: new { data = encryptSerializedValues, area = "" },
                            protocol: ViewContext.HttpContext.Request.Scheme);

            if (string.IsNullOrWhiteSpace(actionUrl))
            {
                throw new NullReferenceException("It's not possible to determine the URL of the `DNTCaptchaImageController.Show` method. Please register the `services.AddControllers()` and `endpoints.MapControllerRoute(...)`.");
            }

            var captchaImage = new TagBuilder("img");
            var dntCaptchaImg = "dntCaptchaImg";
            captchaImage.TagRenderMode = TagRenderMode.SelfClosing;
            captchaImage.Attributes.Add("id", dntCaptchaImg);
            captchaImage.Attributes.Add("name", dntCaptchaImg);
            captchaImage.Attributes.Add("alt", "captcha");
            captchaImage.Attributes.Add("src", actionUrl);
            captchaImage.Attributes.Add("style", "margin-bottom: 4px;");
            return captchaImage;
        }

        private TagBuilder getRefreshButtonTagBuilder(string captchaDivId, string captchaToken)
        {
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
                UseNoise = UseNoise
            };
            var serializedValues = _serializationProvider.Serialize(values);
            var encryptSerializedValues = _captchaProtectionProvider.Encrypt(serializedValues);
            var actionUrl = UseRelativeUrls ?
            _urlHelper.Action(
                action: nameof(DNTCaptchaImageController.Refresh),
                controller: nameof(DNTCaptchaImageController).Replace("Controller", string.Empty),
                values: new { data = encryptSerializedValues, area = "" }) :
            _urlHelper.Action(
                action: nameof(DNTCaptchaImageController.Refresh),
                controller: nameof(DNTCaptchaImageController).Replace("Controller", string.Empty),
                values: new { data = encryptSerializedValues, area = "" },
                protocol: ViewContext.HttpContext.Request.Scheme);

            var refreshButton = new TagBuilder("a");
            var dntCaptchaRefreshButton = "dntCaptchaRefreshButton";
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

        private TagBuilder getOnRefreshButtonDataAjaxBegin()
        {
            var requestVerificationToken = _antiforgery.GetAndStoreTokens(ViewContext.HttpContext).RequestToken;
            var script = new TagBuilder("script");
            script.Attributes.Add("type", "text/javascript");
            script.InnerHtml.AppendHtml(
                $"function {DataAjaxBeginFunctionName}(xhr, settings) {{ settings.data = settings.data + '&__RequestVerificationToken={requestVerificationToken}';  }}");
            return script;
        }

        private TagBuilder getTextInputTagBuilder()
        {
            var textInput = new TagBuilder("input");
            textInput.TagRenderMode = TagRenderMode.SelfClosing;
            textInput.Attributes.Add("id", CaptchaInputName);
            textInput.Attributes.Add("name", CaptchaInputName);
            textInput.Attributes.Add("autocomplete", "off");
            textInput.Attributes.Add("class", TextBoxClass);
            textInput.Attributes.Add("data-val", "true");
            textInput.Attributes.Add("data-val-required", ValidationErrorMessage);
            textInput.Attributes.Add("required", "required");
            textInput.Attributes.Add("data-required-msg", ValidationErrorMessage);
            textInput.Attributes.Add("placeholder", Placeholder);
            textInput.Attributes.Add("type", "text");
            textInput.Attributes.Add("value", "");
            return textInput;
        }

        private TagBuilder getValidationMessageTagBuilder()
        {
            var validationMessage = new TagBuilder("span");
            validationMessage.Attributes.Add("class", ValidationMessageClass);
            validationMessage.Attributes.Add("data-valmsg-for", CaptchaInputName);
            validationMessage.Attributes.Add("data-valmsg-replace", "true");

            if (!ViewContext.ModelState.IsValid)
            {
                if (ViewContext.ModelState.TryGetValue(CaptchaInputName, out var captchaInputNameValidationState))
                {
                    if (captchaInputNameValidationState.ValidationState == ModelValidationState.Invalid)
                    {
                        var error = captchaInputNameValidationState.Errors.FirstOrDefault();
                        if (error != null)
                        {
                            var errorSpan = new TagBuilder("span");
                            errorSpan.InnerHtml.AppendHtml(error.ErrorMessage);
                            validationMessage.InnerHtml.AppendHtml(errorSpan);
                        }
                    }
                }
            }

            return validationMessage;
        }
    }
}
