using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace DNTCaptcha.Core;

/// <summary>
///     Allow changing the route's template dynamically at run-time.
/// </summary>
public class DynamicRoutingControllerModelConvention(string? routeTemplate) : IControllerModelConvention
{
    /// <inheritdoc />
    public void Apply(ControllerModel controller)
    {
        ArgumentNullException.ThrowIfNull(controller);

        if (HasRouteAttributes(controller))
        {
            /*
             Actions are either conventionally routed or attribute routed.
             */
            return;
        }

        if (!IsCaptchaController(controller))
        {
            return;
        }

        ApplyNewTemplateDynamically(controller);
    }

    private void ApplyNewTemplateDynamically(ControllerModel controllerModel)
    {
        if (string.IsNullOrWhiteSpace(routeTemplate))
        {
            return;
        }

        foreach (var selector in controllerModel.Selectors)
        {
            selector.AttributeRouteModel = new AttributeRouteModel
            {
                Template = routeTemplate
            };
        }
    }

    private static bool HasRouteAttributes(ControllerModel controllerModel)
        => controllerModel.Selectors.Any(selector => selector.AttributeRouteModel != null);

    private static bool IsCaptchaController(ControllerModel controllerModel)
    {
        var captchaControllerType = typeof(DNTCaptchaImageController);

        var captchaControllerName =
            captchaControllerType.Name.Replace("Controller", "", StringComparison.OrdinalIgnoreCase);

        return string.Equals(controllerModel.ControllerType.Namespace, captchaControllerType.Namespace,
            StringComparison.OrdinalIgnoreCase) && string.Equals(controllerModel.ControllerName, captchaControllerName,
            StringComparison.OrdinalIgnoreCase);
    }
}