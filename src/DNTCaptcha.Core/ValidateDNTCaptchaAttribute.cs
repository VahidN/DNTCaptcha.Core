using System;
using System.Linq;
using DNTCaptcha.Core.Contracts;
using DNTCaptcha.Core.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace DNTCaptcha.Core
{
    /// <summary>
    /// Validate DNTCaptcha Attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class ValidateDNTCaptchaAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// The language of captcha generator. It's default value is Persian.
        /// </summary>
        public Language CaptchaGeneratorLanguage { set; get; } = Language.Persian;

        /// <summary>
        /// Validation error message. It's default value is `لطفا کد امنیتی را به رقم وارد نمائید`.
        /// </summary>
        public string ErrorMessage { set; get; } = "لطفا کد امنیتی را به رقم وارد نمائید";

        /// <summary>
        /// The input number is not numeric error message. It's default value is `لطفا در قسمت کد امنیتی تنها عدد وارد نمائید`.
        /// </summary>
        public string IsNumericErrorMessage { set; get; } = "لطفا در قسمت کد امنیتی تنها عدد وارد نمائید";

        /// <summary>
        /// Captcha validator.
        /// </summary>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            filterContext.CheckArgumentNull(nameof(filterContext));

            var httpContext = filterContext.HttpContext;
            httpContext.CheckArgumentNull(nameof(httpContext));

            var (captchaText, inputText, cookieToken) = getFormValues(filterContext);
            var validatorService = httpContext.RequestServices.GetService<IDNTCaptchaValidatorService>();
            var result = validatorService.Validate(
                httpContext,
                captchaText,
                inputText,
                cookieToken,
                CaptchaGeneratorLanguage,
                ErrorMessage,
                IsNumericErrorMessage,
                deleteCookieAfterValidation: true);
            if (result.IsValid)
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            var controllerBase = filterContext.Controller as ControllerBase;
            controllerBase.CheckArgumentNull(nameof(controllerBase));

            controllerBase.ModelState.AddModelError(DNTCaptchaTagHelper.CaptchaInputName, result.ErrorMessage);
            base.OnActionExecuting(filterContext);
        }

        private (string captchaText, string inputText, string cookieToken) getFormValues(ActionExecutingContext filterContext)
        {
            var httpContext = filterContext.HttpContext;

            string captchaText, inputText, cookieToken;
            if (httpContext.Request.HasFormContentType)
            {
                var form = httpContext.Request.Form;
                form.CheckArgumentNull(nameof(form));

                captchaText = (string)form[DNTCaptchaTagHelper.CaptchaHiddenInputName];
                inputText = (string)form[DNTCaptchaTagHelper.CaptchaInputName];
                cookieToken = (string)form[DNTCaptchaTagHelper.CaptchaHiddenTokenName];
            }
            else
            {
                var model = filterContext.ActionArguments
                                         .Select(item => item.Value)
                                         .OfType<DNTCaptchaBase>()
                                         .FirstOrDefault();
                if (model == null)
                {
                    throw new NotSupportedException("Your ViewModel should implement the DNTCaptchaBase class (public class AccountViewModel : DNTCaptchaBase {}).");
                }
                captchaText = model.DNTCaptchaText;
                inputText = model.DNTCaptchaInputText;
                cookieToken = model.DNTCaptchaToken;
            }

            return (captchaText, inputText, cookieToken);
        }
    }
}