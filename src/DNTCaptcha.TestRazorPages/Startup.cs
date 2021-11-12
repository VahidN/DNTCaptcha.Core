using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using DNTCaptcha.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Routing;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace DNTCaptcha.TestRazorPages
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            _env = env;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDNTCaptcha(options =>
            {
                // options.UseSessionStorageProvider(); // -> It doesn't rely on the server or client's times. Also it's the safest one.
                // options.UseMemoryCacheStorageProvider(); // -> It relies on the server's times. It's safer than the CookieStorageProvider.
                options.UseCookieStorageProvider(SameSiteMode.Strict /* If you are using CORS, set it to `None` */) // -> It relies on the server and client's times. It's ideal for scalability, because it doesn't save anything in the server's memory.
                // options.UseDistributedCacheStorageProvider(); // --> It's ideal for scalability using `services.AddStackExchangeRedisCache()` for instance.
                // options.UseDistributedSerializationProvider();

                // Don't set this line (remove it) to use the installed system's fonts (FontName = "Tahoma").
                // Or if you want to use a custom font, make sure that font is present in the wwwroot/fonts folder and also use a good and complete font!
                .UseCustomFont(Path.Combine(_env.WebRootPath, "fonts", "IRANSans(FaNum)_Bold.ttf"))
                .AbsoluteExpiration(minutes: 7)
                .ShowThousandsSeparators(false)
                .WithNoise(pixelsDensity: 25, linesCount: 3)
                .WithEncryptionKey("This is my secure key!")
                .InputNames(// This is optional. Change it if you don't like the default names.
                    new DNTCaptchaComponent
                    {
                        CaptchaHiddenInputName = "DNT_CaptchaText",
                        CaptchaHiddenTokenName = "DNT_CaptchaToken",
                        CaptchaInputName = "DNT_CaptchaInputText"
                    })
                .Identifier("dnt_Captcha");// This is optional. Change it if you don't like its default name.
            });

            services.AddControllers(); // this is necessary for the captcha's image provider
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();

                // this is necessary for the captcha's image provider
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
