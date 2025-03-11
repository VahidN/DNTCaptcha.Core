using System.IO;
using DNTCaptcha.Core;
using DNTCaptcha.TestWebApp.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
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
    services.AddCors(options =>
    {
        options.AddPolicy(name: "CorsPolicy", cp => cp
            .WithOrigins("http://localhost:4200") //Note:  The URL must be specified without a trailing slash (/).
            .AllowAnyMethod()
            .AllowAnyHeader()
            .SetIsOriginAllowed(_ => true)
            .AllowCredentials());
    });

    services.AddMvc(options => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

    services.Configure<RouteOptions>(o =>
    {
        o.LowercaseUrls = true; /* o.LowercaseQueryStrings = true; // don't use this! */
    });

    // Added it for AddDNTCaptcha -> options.UseDistributedCacheStorageProvider()
    /*services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = "localhost:6379";
        options.InstanceName = "test";
    });*/

    services.AddDNTCaptcha(options =>
    {
        // options.UseSessionStorageProvider() // -> It doesn't rely on the server or client's times. Also it's the safest one.
        // options.UseMemoryCacheStorageProvider() // -> It relies on the server's times. It's safer than the CookieStorageProvider.
        options.UseCookieStorageProvider(
                /* If you are using CORS, set it to `None` */) // -> It relies on the server and client's times. It's ideal for scalability, because it doesn't save anything in the server's memory.
            // .UseDistributedCacheStorageProvider() // --> It's ideal for scalability using `services.AddStackExchangeRedisCache()` for instance.
            // .UseDistributedSerializationProvider()

            // Don't set this line (remove it) to use the installed system's fonts (FontName = "Tahoma").
            // Or if you want to use a custom font, make sure that font is present in the wwwroot/fonts folder and also use a good and complete font!
            .UseCustomFont(Path.Combine(env.WebRootPath, path2: "fonts", path3: "tahoma.ttf"))
            .ShowExceptionsInResponse(env.IsDevelopment())
            .AbsoluteExpiration(minutes: 1)
            .RateLimiterPermitLimit(
                permitLimit:
                10) // for .NET 7x, Also you need to call app.UseRateLimiter() after calling app.UseRouting().
            .ShowThousandsSeparators(show: false)
            .WithNoise(baseFrequencyX: 0.015f, baseFrequencyY: 0.015f, numOctaves: 1, seed: 0.0f)
            .WithEncryptionKey(key: "This is my secure key!")
            .WithNonceKey(key: "NETESCAPADES_NONCE")
            .WithCaptchaImageControllerNameTemplate(template: "my-custom-captcha")
            .WithCaptchaImageControllerRouteTemplate(template: "my-custom-captcha/[action]")
            .InputNames( // This is optional. Change it if you don't like the default names.
                new DNTCaptchaComponent
                {
                    CaptchaHiddenInputName = "DNT_CaptchaText",
                    CaptchaHiddenTokenName = "DNT_CaptchaToken",
                    CaptchaInputName = "DNT_CaptchaInputText"
                })
            .Identifier(className: "dnt_Captcha") // This is optional. Change it if you don't like its default name.
            ;
    });

    services.AddSession();

    services.AddControllersWithViews();
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

    logging.AddConfiguration(configuration.GetSection(key: "Logging"));
}

void ConfigureMiddlewares(IApplicationBuilder app, IHostEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler(errorHandlingPath: "/Home/Error");
        app.UseHsts();
    }

    app.UseSecurityHeaders(policies => policies.AddMyCustomCsp(env.IsDevelopment()));

    app.UseHttpsRedirection();

    app.UseCors(policyName: "CorsPolicy");

    app.UseStaticFiles();

    app.UseRouting();
    app.UseRateLimiter();

    app.UseSession();
}

void ConfigureEndpoints(IApplicationBuilder app)
    => app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
        endpoints.MapRazorPages();
    });
