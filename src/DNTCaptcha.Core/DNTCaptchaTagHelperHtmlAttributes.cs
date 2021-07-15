using System;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace DNTCaptcha.Core
{
    /// <summary>
    /// Tag helper attributes
    /// </summary>
    [Serializable]
    public class DNTCaptchaTagHelperHtmlAttributes
    {
        /// <summary>
        /// Create an absolute "fully qualified" url from an action and controller.
        /// </summary>
        [HtmlAttributeName("asp-use-relative-urls")]
        public bool UseRelativeUrls { set; get; }

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
        /// Display mode of the captcha's text. It's default value is NumberToWord.
        /// </summary>
        [HtmlAttributeName("asp-captcha-generator-display-mode")]
        public DisplayMode DisplayMode { set; get; }

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
        /// The direction of the input box. It's default value is `ltr`.
        /// </summary>
        [HtmlAttributeName("asp-dir")]
        public string Dir { set; get; } = "ltr";

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
            "<div class='input-group col-md-4'><span class='input-group-addon'><span class='glyphicon glyphicon-lock'></span></span>{0}</div>";

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
        /// The refresh-button-class of the captcha. It's default value is `glyphicon glyphicon-refresh btn-sm`.
        /// </summary>
        [HtmlAttributeName("asp-refresh-button-class")]
        public string RefreshButtonClass { set; get; } = "glyphicon glyphicon-refresh btn-sm";

        /// <summary>
        /// The Captcha Token
        /// </summary>
        [HtmlAttributeNotBound]
        public string CaptchaToken { set; get; } = default!;

        /// <summary>
        /// Its default value is true.
        /// </summary>
        [HtmlAttributeName("asp-show-refresh-button")]
        public bool ShowRefreshButton { set; get; } = true;
    }
}