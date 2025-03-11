using System.IO;
using System.Text.Json.Serialization;
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
    services.AddSwaggerGen();

    services.AddDNTCaptcha(options =>
    {
        // options.UseSessionStorageProvider(); // -> It doesn't rely on the server or client's times. Also it's the safest one.
        // options.UseMemoryCacheStorageProvider(); // -> It relies on the server's times. It's safer than the CookieStorageProvider.
        options.UseCookieStorageProvider(
                /* If you are using CORS, set it to `None` */) // -> It relies on the server and client's times. It's ideal for scalability, because it doesn't save anything in the server's memory.
            // .UseDistributedCacheStorageProvider(); // --> It's ideal for scalability using `services.AddStackExchangeRedisCache()` for instance.
            // .UseDistributedSerializationProvider();

            // Don't set this line (remove it) to use the installed system's fonts (FontName = "Tahoma").
            // Or if you want to use a custom font, make sure that font is present in the wwwroot/fonts folder and also use a good and complete font!
            .UseCustomFont(Path.Combine(env.WebRootPath, path2: "fonts", path3: "tahoma.ttf"))
            .AbsoluteExpiration(minutes: 7)
            .RateLimiterPermitLimit(
                permitLimit:
                10) // for .NET 7x, Also you need to call app.UseRateLimiter() after calling app.UseRouting().
            .ShowExceptionsInResponse(env.IsDevelopment())
            .ShowThousandsSeparators(show: false)
            .WithNoise(baseFrequencyX: 0.015f, baseFrequencyY: 0.015f, numOctaves: 1, seed: 0.0f)
            .WithEncryptionKey(key: "This is my secure key!")
            .WithNonceKey(key: "NETESCAPADES_NONCE")
            .WithCaptchaImageControllerNameTemplate(template: "my-custom-captcha")
            .WithCaptchaImageControllerRouteTemplate(template: "my-custom-captcha/[action]")
            .InputNames(new DNTCaptchaComponent
            {
                CaptchaHiddenInputName = "DNTCaptchaText",
                CaptchaHiddenTokenName = "DNTCaptchaToken",
                CaptchaInputName = "DNTCaptchaInputText"
            })
            .Identifier(className: "dntCaptcha");
    });

    services.AddControllers()
        .AddJsonOptions(opt =>
        {
            // This is necessary for the languages enum.
            opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
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
        app.UseSwagger();
        app.UseSwaggerUI(); // The Swagger UI can be found at https://localhost:<port>/swagger
    }

    app.UseHttpsRedirection();
    app.UseDefaultFiles();
    app.UseStaticFiles();

    app.UseRouting();
    app.UseRateLimiter();

    app.UseAuthorization();
}

void ConfigureEndpoints(IApplicationBuilder app) => app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
