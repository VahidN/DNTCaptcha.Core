using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace DNTCaptcha.Core;

/// <summary>
///     Allow changing the route's template dynamically at run-time.
/// </summary>
public class ControllerRoutingConvention(Type controllerType, string? routeTemplate, string? nameTemplate)
    : IControllerModelConvention
{
    /// <inheritdoc />
    public void Apply(ControllerModel controller)
    {
        ArgumentNullException.ThrowIfNull(controller);

        if (HasRouteAttributes(controller))
        {
            return;
        }

        if (!IsControllerInstance(controllerType, controller))
        {
            return;
        }

        ApplyNewRouteDynamically(controller);
    }

    private void ApplyNewRouteDynamically(ControllerModel controllerModel)
    {
        if (string.IsNullOrWhiteSpace(routeTemplate))
        {
            return;
        }

        foreach (var action in controllerModel.Actions)
        {
            action.Selectors[0].AttributeRouteModel = new AttributeRouteModel
            {
                Template = routeTemplate,
                Name = $"{nameTemplate}-{action.ActionName}"
            };
        }
    }

    private static bool HasRouteAttributes(ControllerModel controllerModel)
        => controllerModel.Selectors.Any(selector => selector.AttributeRouteModel != null);

    private static bool IsControllerInstance(Type controller, ControllerModel controllerModel)
    {
        var controllerName =
            controller.Name.Replace(oldValue: "Controller", newValue: "", StringComparison.OrdinalIgnoreCase);

        return string.Equals(controllerModel.ControllerType.Namespace, controller.Namespace,
            StringComparison.OrdinalIgnoreCase) && string.Equals(controllerModel.ControllerName, controllerName,
            StringComparison.OrdinalIgnoreCase);
    }
}