using System;
using System.Linq;
using DNTCaptcha.Core.Contracts;
using DNTCaptcha.Core.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace DNTCaptcha.Core
{
    /// <summary>
    /// DNTCaptcha TagHelper
    /// </summary>
    [HtmlTargetElement("dnt-captcha")]
    public class DNTCaptchaTagHelper : TagHelper
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

        private readonly ICaptchaProtectionProvider _captchaProtectionProvider;
        private readonly ICaptchaStorageProvider _captchaStorageProvider;
        private readonly IHumanReadableIntegerProvider _humanReadableIntegerProvider;
        private readonly IRandomNumberProvider _randomNumberProvider;

        /// <summary>
        /// DNTCaptcha TagHelper
        /// </summary>
        public DNTCaptchaTagHelper(
            ICaptchaProtectionProvider captchaProtectionProvider,
            IRandomNumberProvider randomNumberProvider,
            IHumanReadableIntegerProvider humanReadableIntegerProvider,
            ICaptchaStorageProvider captchaStorageProvider)
        {
            captchaProtectionProvider.CheckArgumentNull(nameof(captchaProtectionProvider));
            randomNumberProvider.CheckArgumentNull(nameof(randomNumberProvider));
            humanReadableIntegerProvider.CheckArgumentNull(nameof(humanReadableIntegerProvider));
            captchaStorageProvider.CheckArgumentNull(nameof(captchaStorageProvider));

            _captchaProtectionProvider = captchaProtectionProvider;
            _randomNumberProvider = randomNumberProvider;
            _humanReadableIntegerProvider = humanReadableIntegerProvider;
            _captchaStorageProvider = captchaStorageProvider;
        }

        /// <summary>
        /// Current area name. It's default value is string.Empty.
        /// </summary>
        [HtmlAttributeName("asp-area-name")]
        public string AreaName { set; get; } = "";

        /// <summary>
        /// The back-color of the captcha. It's default value is string.Empty.
        /// </summary>
        [HtmlAttributeName("asp-back-color")]
        public string BackColor { set; get; } = "";

        /// <summary>
        /// The font-name of the captcha. It's default value is Tahoma.
        /// </summary>
        [HtmlAttributeName("asp-font-name")]
        public string FontName { set; get; } = "Tahoma";

        /// <summary>
        /// The font-size of the captcha. It's default value is 12.
        /// </summary>
        [HtmlAttributeName("asp-font-size")]
        public float FontSize { set; get; } = 12;

        /// <summary>
        /// The fore-color of the captcha. It's default value is #1B0172.
        /// </summary>
        [HtmlAttributeName("asp-fore-color")]
        public string ForeColor { set; get; } = "#1B0172";

        /// <summary>
        /// The language of the captcha. It's default value is Persian.
        /// </summary>
        [HtmlAttributeName("asp-captcha-generator-language")]
        public Language Language { set; get; } = Language.Persian;

        /// <summary>
        /// The max value of the captcha. It's default value is 9000.
        /// </summary>
        [HtmlAttributeName("asp-captcha-generator-max")]
        public int Max { set; get; } = 9000;

        /// <summary>
        /// The min value of the captcha. It's default value is 1.
        /// </summary>
        [HtmlAttributeName("asp-captcha-generator-min")]
        public int Min { set; get; } = 1;

        /// <summary>
        /// The placeholder value of the captcha. It's default value is `کد امنیتی به رقم`.
        /// </summary>
        [HtmlAttributeName("asp-placeholder")]
        public string Placeholder { set; get; } = "کد امنیتی به رقم";

        /// <summary>
        /// The css class of the captcha. It's default value is `text-box single-line form-control col-md-4`.
        /// </summary>
        [HtmlAttributeName("asp-text-box-class")]
        public string TextBoxClass { set; get; } = "text-box single-line form-control col-md-4";

        /// <summary>
        /// The text-box-template of the captcha. It's default value is `<div class='input-group col-md-4'><span class='input-group-addon'><span class='glyphicon glyphicon-lock'></span></span>{0}</div>`.
        /// </summary>
        [HtmlAttributeName("asp-text-box-template")]
        public string TextBoxTemplate { set; get; } =
            @"<div class='input-group col-md-4'><span class='input-group-addon'><span class='glyphicon glyphicon-lock'></span></span>{0}</div>";

        /// <summary>
        /// The validation-error-message of the captcha. It's default value is `لطفا کد امنیتی را به رقم وارد نمائید`.
        /// </summary>
        [HtmlAttributeName("asp-validation-error-message")]
        public string ValidationErrorMessage { set; get; } = "لطفا کد امنیتی را به رقم وارد نمائید";

        /// <summary>
        /// The validation-message-class of the captcha. It's default value is `text-danger`.
        /// </summary>
        [HtmlAttributeName("asp-validation-message-class")]
        public string ValidationMessageClass { set; get; } = "text-danger";

        /// <summary>
        /// The current ViewContext.
        /// </summary>
        [ViewContext, HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        /// <summary>
        /// Process the taghelper and generate the output.
        /// </summary>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            context.CheckArgumentNull(nameof(context));
            output.CheckArgumentNull(nameof(output));

            output.TagName = "div";
            output.Attributes.Add("class", "dntCaptcha");
            output.Attributes.Add("id", $"dntCaptcha.{_randomNumberProvider.Next(Min, Max)}");
            output.TagMode = TagMode.StartTagAndEndTag;

            var number = _randomNumberProvider.Next(Min, Max);
            var randomText = _humanReadableIntegerProvider.NumberToText(number, Language);
            var encryptedText = _captchaProtectionProvider.Encrypt(randomText);

            var captchaImage = getCaptchaImageTagBuilder(encryptedText);
            output.Content.AppendHtml(captchaImage);

            var hiddenInput = getHiddenInputTagBuilder(encryptedText);
            output.Content.AppendHtml(hiddenInput);

            var textInput = getTextInputTagBuilder();
            output.Content.AppendHtml($"{string.Format(TextBoxTemplate, textInput.GetString())}");

            var validationMessage = getValidationMessageTagBuilder();
            output.Content.AppendHtml(validationMessage);

            var cookieToken = $".{context.UniqueId}.{_randomNumberProvider.Next(Min, Max)}";
            var hiddenInputToken = getHiddenInputTokenTagBuilder(_captchaProtectionProvider.Encrypt(cookieToken));
            output.Content.AppendHtml(hiddenInputToken);

            _captchaStorageProvider.Add(ViewContext.HttpContext, cookieToken, randomText);
        }

        private static TagBuilder getHiddenInputTagBuilder(string encryptedText)
        {
            var hiddenInput = new TagBuilder("input");
            hiddenInput.Attributes.Add("id", CaptchaHiddenInputName);
            hiddenInput.Attributes.Add("name", CaptchaHiddenInputName);
            hiddenInput.Attributes.Add("type", "hidden");
            hiddenInput.Attributes.Add("value", encryptedText);
            return hiddenInput;
        }

        private static TagBuilder getHiddenInputTokenTagBuilder(string token)
        {
            var hiddenInput = new TagBuilder("input");
            hiddenInput.Attributes.Add("id", CaptchaHiddenTokenName);
            hiddenInput.Attributes.Add("name", CaptchaHiddenTokenName);
            hiddenInput.Attributes.Add("type", "hidden");
            hiddenInput.Attributes.Add("value", token);
            return hiddenInput;
        }

        private TagBuilder getCaptchaImageTagBuilder(string encryptedText)
        {
            IUrlHelper urlHelper = new UrlHelper(ViewContext);
            var actionUrl = urlHelper.Action(action: nameof(DNTCaptchaImageController.Show),
                controller: nameof(DNTCaptchaImageController).Replace("Controller", string.Empty),
                values:
                new
                {
                    text = encryptedText,
                    rndDate = DateTime.Now.Ticks,
                    foreColor = ForeColor,
                    backColor = BackColor,
                    fontSize = FontSize,
                    fontName = FontName,
                    Area = AreaName
                });

            var captchaImage = new TagBuilder("img");
            var dntCaptchaImg = "dntCaptchaImg";
            captchaImage.Attributes.Add("id", dntCaptchaImg);
            captchaImage.Attributes.Add("name", dntCaptchaImg);
            captchaImage.Attributes.Add("alt", "captcha");
            captchaImage.Attributes.Add("src", actionUrl);
            captchaImage.Attributes.Add("style", "margin-bottom: 4px;");
            return captchaImage;
        }

        private TagBuilder getTextInputTagBuilder()
        {
            var textInput = new TagBuilder("input");
            textInput.Attributes.Add("id", CaptchaInputName);
            textInput.Attributes.Add("name", CaptchaInputName);
            textInput.Attributes.Add("autocomplete", "off");
            textInput.Attributes.Add("class", TextBoxClass);
            textInput.Attributes.Add("data-val", "true");
            textInput.Attributes.Add("data-val-required", ValidationErrorMessage);
            textInput.Attributes.Add("placeholder", Placeholder);
            textInput.Attributes.Add("dir", "ltr");
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
                ModelStateEntry captchaInputNameValidationState;
                if (ViewContext.ModelState.TryGetValue(CaptchaInputName, out captchaInputNameValidationState))
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