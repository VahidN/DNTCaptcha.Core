using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace DNTCaptcha.Core.Providers
{
    /// <summary>
    /// A fake view provider for rendering tag helpers
    /// </summary>
    public class FakeView : IView
    {
        string IView.Path
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// A fake view provider for rendering tag helpers
        /// </summary>
        public Task RenderAsync(ViewContext viewContext)
        {
            throw new InvalidOperationException();
        }

        Task IView.RenderAsync(ViewContext context)
        {
            throw new NotImplementedException();
        }
    }
}