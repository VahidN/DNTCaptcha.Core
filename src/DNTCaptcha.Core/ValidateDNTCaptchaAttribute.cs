using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DNTCaptcha.Core;

/// <summary>
///     Validate DNTCaptcha Attribute
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ValidateDNTCaptchaAttribute : ActionFilterAttribute
{
    /// <summary>
    ///     Validation error message. It's default value is `لطفا کد امنیتی را به رقم وارد نمائید`.
    /// </summary>
    public string ErrorMessage { set; get; } = "لطفا کد امنیتی را به رقم وارد نمائید";

    /// <summary>
    ///     Captcha validator.
    /// </summary>
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var httpContext = context.HttpContext ?? throw new InvalidOperationException(message: "httpContext is null.");
        var validatorService = httpContext.RequestServices.GetRequiredService<IDNTCaptchaValidatorService>();

        if (validatorService.HasRequestValidCaptchaEntry())
        {
            base.OnActionExecuting(context);

            return;
        }

        if (context.Controller is not ControllerBase controllerBase)
        {
            throw new InvalidOperationException(message: "controllerBase is null.");
        }

        var options = httpContext.RequestServices.GetRequiredService<IOptions<DNTCaptchaOptions>>();
        controllerBase.ModelState.AddModelError(options.Value.CaptchaComponent.CaptchaInputName, ErrorMessage);
        base.OnActionExecuting(context);
    }
}