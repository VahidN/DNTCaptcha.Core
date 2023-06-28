using System.IO;
using DNTCaptcha.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);
ConfigureLogging(builder.Logging, builder.Environment, builder.Configuration);
ConfigureServices(builder.Services, builder.Environment);
var webApp = builder.Build();
ConfigureMiddlewares(webApp, webApp.Environment);
ConfigureEndpoints(webApp);
webApp.Run();

void ConfigureServices(IServiceCollection services, IWebHostEnvironment env)
{
    services.AddDNTCaptcha(options =>
                           {
                               // options.UseSessionStorageProvider(); // -> It doesn't rely on the server or client's times. Also it's the safest one.
                               // options.UseMemoryCacheStorageProvider(); // -> It relies on the server's times. It's safer than the CookieStorageProvider.
                               options
                                   .UseCookieStorageProvider( /* If you are using CORS, set it to `None` */) // -> It relies on the server and client's times. It's ideal for scalability, because it doesn't save anything in the server's memory.
                                   // options.UseDistributedCacheStorageProvider(); // --> It's ideal for scalability using `services.AddStackExchangeRedisCache()` for instance.
                                   // options.UseDistributedSerializationProvider();

                                   // Don't set this line (remove it) to use the installed system's fonts (FontName = "Tahoma").
                                   // Or if you want to use a custom font, make sure that font is present in the wwwroot/fonts folder and also use a good and complete font!
                                   .UseCustomFont(Path.Combine(env.WebRootPath, "fonts", "IRANSans(FaNum)_Bold.ttf"))
                                   .AbsoluteExpiration(7)
                                   .ShowExceptionsInResponse(env.IsDevelopment())
                                   .ShowThousandsSeparators(false)
                                   .WithNoise(0.015f, 0.015f, 1, 0.0f)
                                   .WithEncryptionKey("This is my secure key!")
                                   .InputNames( // This is optional. Change it if you don't like the default names.
                                               new DNTCaptchaComponent
                                               {
                                                   CaptchaHiddenInputName = "DNT_CaptchaText",
                                                   CaptchaHiddenTokenName = "DNT_CaptchaToken",
                                                   CaptchaInputName = "DNT_CaptchaInputText",
                                               })
                                   .Identifier("dnt_Captcha"); // This is optional. Change it if you don't like its default name.
                           });

    services.AddControllers(); // this is necessary for the captcha's image provider
    services.AddRazorPages();
}

void ConfigureLogging(ILoggingBuilder logging, IHostEnvironment env, IConfiguration configuration)
{
    logging.ClearProviders();

    logging.AddDebug();

    if (env.IsDevelopment())
    {
        logging.AddConsole();
    }

    logging.AddConfiguration(configuration.GetSection("Logging"));
}

void ConfigureMiddlewares(IApplicationBuilder app, IHostEnvironment env)
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
}

void ConfigureEndpoints(IApplicationBuilder app)
{
    app.UseEndpoints(endpoints =>
                     {
                         endpoints.MapRazorPages();

                         // this is necessary for the captcha's image provider
                         endpoints.MapControllerRoute(
                                                      "default",
                                                      "{controller=Home}/{action=Index}/{id?}");
                     });
}