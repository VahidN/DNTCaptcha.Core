using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using DNTCaptcha.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace DNTCaptcha.TestWebApp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .SetIsOriginAllowed((host) => true)
                        .AllowCredentials());
            });
            services.AddMvc(options => { options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()); })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.Configure<RouteOptions>(o => o.LowercaseUrls = true);

            services.AddDNTCaptcha(options =>
                // options.UseSessionStorageProvider() // -> It doesn't rely on the server or client's times. Also it's the safest one.
                // options.UseMemoryCacheStorageProvider() // -> It relies on the server's times. It's safer than the CookieStorageProvider.
                options.UseCookieStorageProvider() // -> It relies on the server and client's times. It's ideal for scalability, because it doesn't save anything in the server's memory.
                );

            services.AddSession();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseCors(policyName: "CorsPolicy");

            app.UseDeveloperExceptionPage();

            app.UseStaticFiles();

            app.UseSession();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}