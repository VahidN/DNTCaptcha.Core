using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace DNTCaptcha.Core;

/// <summary>
///     A fake view provider for rendering tag helpers
/// </summary>
public class FakeView : IView
{
    /// <summary>
    ///     Gets the path of the view as resolved by the Microsoft.AspNetCore.Mvc.ViewEngines.IViewEngine.
    /// </summary>
    public string Path => throw new InvalidOperationException();

    /// <summary>
    ///     A fake view provider for rendering tag helpers
    /// </summary>
    public Task RenderAsync(ViewContext context) => throw new InvalidOperationException();
}