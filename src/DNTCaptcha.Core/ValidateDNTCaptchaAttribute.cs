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
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            filterContext.CheckArgumentNull(nameof(filterContext));

            var httpContext = filterContext.HttpContext;
            httpContext.CheckArgumentNull(nameof(httpContext));


            var validatorService = httpContext.RequestServices.GetService<IDNTCaptchaValidatorService>();
            if (validatorService.HasRequestValidCaptchaEntry(
                    CaptchaGeneratorLanguage,
                    CaptchaGeneratorDisplayMode,
                    filterContext.ActionArguments.Select(item => item.Value).OfType<DNTCaptchaBase>().FirstOrDefault()))
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            var controllerBase = filterContext.Controller as ControllerBase;
            controllerBase.CheckArgumentNull(nameof(controllerBase));

            controllerBase.ModelState.AddModelError(DNTCaptchaTagHelper.CaptchaInputName, ErrorMessage);
            base.OnActionExecuting(filterContext);
        }
    }
}