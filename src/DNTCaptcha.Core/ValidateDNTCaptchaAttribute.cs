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
        /// The display mode of captcha generator. It's default value is NumberToWord.
        /// </summary>
        public DisplayMode CaptchaGeneratorDisplayMode { set; get; }

        /// <summary>
        /// Validation error message. It's default value is `لطفا کد امنیتی را به رقم وارد نمائید`.
        /// </summary>
        public string ErrorMessage { set; get; } = "لطفا کد امنیتی را به رقم وارد نمائید";

        /// <summary>
        /// Captcha validator.
        /// </summary>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var httpContext = context.HttpContext;
            if (httpContext == null)
            {
                throw new InvalidOperationException("httpContext is null.");
            }


            var validatorService = httpContext.RequestServices.GetRequiredService<IDNTCaptchaValidatorService>();
            if (validatorService.HasRequestValidCaptchaEntry(
                    CaptchaGeneratorLanguage,
                    CaptchaGeneratorDisplayMode,
                    context.ActionArguments.Select(item => item.Value).OfType<DNTCaptchaBase>().FirstOrDefault()))
            {
                base.OnActionExecuting(context);
                return;
            }

            var controllerBase = context.Controller as ControllerBase;
            if (controllerBase == null)
            {
                throw new InvalidOperationException("controllerBase is null.");
            }

            controllerBase.ModelState.AddModelError(DNTCaptchaTagHelper.CaptchaInputName, ErrorMessage);
            base.OnActionExecuting(context);
        }
    }
}