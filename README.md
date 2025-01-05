# DNTCaptcha.Core

<p align="left">
  <a href="https://github.com/VahidN/DNTCaptcha.Core">
     <img alt="GitHub Actions status" src="https://github.com/VahidN/DNTCaptcha.Core/workflows/.NET%20Core%20Build/badge.svg">
  </a>
</p>

`DNTCaptcha.Core` is a captcha generator and validator for ASP.NET Core applications.

![dntcaptcha](/src/DNTCaptcha.TestWebApp/wwwroot/Content/dntcaptcha.png)

## Install via NuGet

[![Nuget](https://img.shields.io/nuget/v/DNTCaptcha.Core)](http://www.nuget.org/packages/DNTCaptcha.Core/)

To install DNTCaptcha.Core, run the following command in the Package Manager Console:

```
PM> Install-Package DNTCaptcha.Core
```

You can also view the [package page](http://www.nuget.org/packages/DNTCaptcha.Core/) on NuGet.

## Linux (and containers) support

The `SkiaSharp` library needs extra dependencies to work on Linux and containers. Please install the following NuGet
packages:

```
PM> Install-Package SkiaSharp.NativeAssets.Linux.NoDependencies
PM> Install-Package HarfBuzzSharp.NativeAssets.Linux
```

You also need to modify your `.csproj` file to include some MSBuild directives that ensure the required files are in a
good place. These extra steps are normally not required but seems to be some issues on how .NET loads them.

```xml
<Target Name="CopyFilesAfterPublish" AfterTargets="AfterPublish">
    <Copy SourceFiles="$(TargetDir)runtimes/linux-x64/native/libSkiaSharp.so" DestinationFolder="$([System.IO.Path]::GetFullPath('$(PublishDir)'))/bin/" />
    <Copy SourceFiles="$(TargetDir)runtimes/linux-x64/native/libHarfBuzzSharp.so" DestinationFolder="$([System.IO.Path]::GetFullPath('$(PublishDir)'))/bin/" />
</Target>
```

Also you should
use [a custom font](https://github.com/VahidN/DNTCaptcha.Core/blob/master/src/DNTCaptcha.TestWebApp/Program.cs#L59) for
Linux systems, because they don't have the `Tahoma` font which is defined by `asp-font-name="Tahoma"` by default.

## Usage

- After installing the DNTCaptcha.Core package, add the following definition to
  the [\_ViewImports.cshtml](/src/DNTCaptcha.TestWebApp/Views/_ViewImports.cshtml) file:

```csharp
@addTagHelper *, DNTCaptcha.Core
```

- Then to use it, add its new tag-helper to [your view](/src/DNTCaptcha.TestWebApp/Views/Home/_LoginFormBody.cshtml):

For bootstrap-3:

```xml
<dnt-captcha asp-captcha-generator-max="9000"
             asp-captcha-generator-min="1"
             asp-captcha-generator-language="English"
             asp-captcha-generator-display-mode="NumberToWord"
             asp-use-relative-urls="true"
             asp-placeholder="Security code as a number"
             asp-validation-error-message="Please enter the security code as a number."
             asp-too-many-requests-error-message="Too many requests! Please wait a minute!"
             asp-font-name="Tahoma"
             asp-font-size="20"
             asp-fore-color="#333333"
             asp-back-color="#ccc"
             asp-text-box-class="text-box single-line form-control col-md-4"
             asp-text-box-template="<div class='input-group col-md-4'><span class='input-group-addon'><span class='glyphicon glyphicon-lock'></span></span>{0}</div>"
             asp-validation-message-class="text-danger"
             asp-refresh-button-class="glyphicon glyphicon-refresh btn-sm"
             asp-show-refresh-button="true"
             />
```

For bootstrap-4 (you will need Bootstrap Icons for the missing [font-glyphs](https://icons.getbootstrap.com/) too):

```xml
<dnt-captcha asp-captcha-generator-max="9000"
             asp-captcha-generator-min="1"
             asp-captcha-generator-language="English"
             asp-captcha-generator-display-mode="NumberToWord"
             asp-use-relative-urls="true"
             asp-placeholder="Security code as a number"
             asp-validation-error-message="Please enter the security code as a number."
             asp-too-many-requests-error-message="Too many requests! Please wait a minute!"
             asp-font-name="Tahoma"
             asp-font-size="20"
             asp-fore-color="#333333"
             asp-back-color="#ccc"
             asp-text-box-class="text-box form-control"
             asp-text-box-template="<div class='input-group'><span class='input-group-prepend'><span class='input-group-text'><i class='fas fa-lock'></i></span></span>{0}</div>"
             asp-validation-message-class="text-danger"
             asp-refresh-button-class="fas fa-redo btn-sm"
             asp-show-refresh-button="true"
             />
```

For bootstrap-5 (you will need Bootstrap Icons for the missing [font-glyphs](https://icons.getbootstrap.com/) too):

```xml
<dnt-captcha asp-captcha-generator-max="30"
			 asp-captcha-generator-min="1"
			 asp-captcha-generator-language="English"
			 asp-captcha-generator-display-mode="NumberToWord"
			 asp-use-relative-urls="true"
			 asp-placeholder="Security code as a number"
			 asp-validation-error-message="Please enter the security code as a number."
                         asp-too-many-requests-error-message="Too many requests! Please wait a minute!"
			 asp-font-name="Tahoma"
			 asp-font-size="20"
			 asp-fore-color="#333333"
			 asp-back-color="#FCF6F5FF"
			 asp-text-box-class="form-control"
			 asp-text-box-template="<div class='input-group'><span class='input-group-text'><span class='bi-lock'></span></span>{0}</div>"
			 asp-validation-message-class="text-danger"
			 asp-refresh-button-class="bi-arrow-counterclockwise btn-lg"
			 asp-show-refresh-button="true"
			 asp-dir="ltr"
			 />
```

- To register its default providers, call `services.AddDNTCaptcha();` method in
  your [Program class](/src/DNTCaptcha.TestWebApp/Program.cs).

```csharp
using DNTCaptcha.Core;

namespace DNTCaptcha.TestWebApp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
	    services.AddDNTCaptcha(options =>
            {
                // options.UseSessionStorageProvider() // -> It doesn't rely on the server or client's times. Also it's the safest one.
                // options.UseMemoryCacheStorageProvider() // -> It relies on the server's times. It's safer than the CookieStorageProvider.
                options.UseCookieStorageProvider(SameSiteMode.Strict) // -> It relies on the server and client's times. It's ideal for scalability, because it doesn't save anything in the server's memory.
                                                   // .UseDistributedCacheStorageProvider() // --> It's ideal for scalability using `services.AddStackExchangeRedisCache()` for instance.
                                                   // .UseDistributedSerializationProvider()

                // Don't set this line (remove it) to use the installed system's fonts (FontName = "Tahoma").
                // Or if you want to use a custom font, make sure that font is present in the wwwroot/fonts folder and also use a good and complete font!
                .UseCustomFont(Path.Combine(_env.WebRootPath, "fonts", "IRANSans(FaNum)_Bold.ttf")) // This is optional.
                .AbsoluteExpiration(minutes: 7)
                .RateLimiterPermitLimit(10) // for .NET 7x+, Also you need to call app.UseRateLimiter() after calling app.UseRouting().
                .WithRateLimiterRejectResponse("RateLimit Exceeded.") //you can instead provide an object, it will automatically converted to json result.
                .ShowThousandsSeparators(false)
                .WithNoise(0.015f, 0.015f, 1, 0.0f)
                .WithEncryptionKey("This is my secure key!")
                .WithNonceKey("NETESCAPADES_NONCE")
                .WithCaptchaImageControllerRouteTemplate("my-custom-captcha/[action]")
                .WithCaptchaImageControllerNameTemplate("my-custom-captcha")
                .InputNames(// This is optional. Change it if you don't like the default names.
                    new DNTCaptchaComponent
                    {
                        CaptchaHiddenInputName = "DNTCaptchaText",
                        CaptchaHiddenTokenName = "DNTCaptchaToken",
                        CaptchaInputName = "DNTCaptchaInputText"
                    })
                .Identifier("dntCaptcha")// This is optional. Change it if you don't like its default name.
                ;
     	    });
        }
```

- Now you can add the `ValidateDNTCaptcha`
  attribute [to your action method](/src/DNTCaptcha.TestWebApp/Controllers/HomeController.cs) to verify the entered
  security code:

```csharp
[HttpPost, ValidateAntiForgeryToken]
[ValidateDNTCaptcha(ErrorMessage = "Please enter the security code as a number.")]
public IActionResult Index([FromForm]AccountViewModel data)
{
    if (ModelState.IsValid) // If `ValidateDNTCaptcha` fails, it will set a `ModelState.AddModelError`.
    {
        //TODO: Save data
        return RedirectToAction(nameof(Thanks), new { name = data.Username });
    }
    return View();
}
```

Or you can use the `IDNTCaptchaValidatorService` directly:

```csharp
namespace DNTCaptcha.TestWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDNTCaptchaValidatorService _validatorService;
        private readonly DNTCaptchaOptions _captchaOptions;

        public HomeController(IDNTCaptchaValidatorService validatorService, IOptions<DNTCaptchaOptions> options)
        {
            _validatorService = validatorService;
            _captchaOptions = options == null ? throw new ArgumentNullException(nameof(options)) : options.Value;
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Login2([FromForm]AccountViewModel data)
        {
            if (!_validatorService.HasRequestValidCaptchaEntry())
            {
                this.ModelState.AddModelError(_captchaOptions.CaptchaComponent.CaptchaInputName, "Please enter the security code as a number.");
                return View(nameof(Index));
            }

            //TODO: Save data
            return RedirectToAction(nameof(Thanks), new { name = data.Username });
        }
```

## How to choose a correct storage mode

If your environment is distributed and you are using a `Session (UseSessionStorageProvider())`
or `Memory (UseMemoryCacheStorageProvider())` storage providers to store some temporary values, these values will not be
distributed at all. By default, ASP.NET's session is maintained in the RAM of the running web server. However, for
instance Windows Azure is a stateless platform, web role instances have no local storage; at any time the web role
instance could be moved to a different server in the data center. When the web role instance is moved, the session state
**is lost**. To have a perceived sense of state with a stateless protocol on a stateless web server, you need permanent
server side storage that persists even if the web role instance is moved. In this case you
should `UseDistributedCacheStorageProvider()` or at first try using the `UseCookieStorageProvider()`.

## How to debug it!

If you can't run this library and the captcha image is not being displayed, first you should checkout the server's logs
and at least you should see what's the response of the requested image. Open the network's tab of the browser's
developer tools and see the responses.
If you want to see the output
of [_logger.LogDebug](https://github.com/VahidN/DNTCaptcha.Core/blob/master/src/DNTCaptcha.Core/DNTCaptchaImageController.cs#L188),
you
should [turn on this level of logging](https://github.com/VahidN/DNTCaptcha.Core/blob/master/src/DNTCaptcha.TestWebApp/appsettings.json#L3)
in the appsettings.json file.
Also you can set the `.ShowExceptionsInResponse(env.IsDevelopment())` option to simplify the debugging by showing the
exceptions in the response body.

## The Blazor based version!

If you have a Blazor based app, it's better to try [DNTCaptcha.Blazor](https://github.com/VahidN/DNTCaptcha.Blazor).

**Tips**

- If you are using the `UseCookieStorageProvider()` and also the `CORS` is activated, you should set the `SameSiteMode`
  to `None`: `options.UseCookieStorageProvider(SameSiteMode.None)` otherwise its default mode effectively
  disables `CORS`.
- If you are using the `Cloudflare`, it doesn't like the SameSite cookies. So in this case try setting
  the `SameSiteMode.None` if the `CookieStorageProvider` is in use.
- If your app is behind a reverse proxy, don't forget to add
  the [forwarded headers middleware](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-5.0#forwarded-headers-middleware-order).

```C#
services.Configure<ForwardedHeadersOptions>(options =>
{
  options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
  options.KnownProxies.Add(IPAddress.Parse("my load balancer ip 1"));
  options.KnownProxies.Add(IPAddress.Parse("my load balancer ip 2"));
});
```

**Samples:**

- [ASP.NET Core MVC Sample](/src/DNTCaptcha.TestWebApp)
- [ASP.NET Core Razor Pages Sample](/src/DNTCaptcha.TestRazorPages)
- [ASP.NET Core Web API sample](/src/DNTCaptcha.TestApiApp)

**Different supported DisplayModes:**

| DisplayMode            | Output                                                              |
|------------------------|---------------------------------------------------------------------|
| NumberToWord           | ![dntcaptcha](/src/DNTCaptcha.TestWebApp/wwwroot/Content/mode1.png) |
| ShowDigits             | ![dntcaptcha](/src/DNTCaptcha.TestWebApp/wwwroot/Content/mode2.png) |
| SumOfTwoNumbers        | ![dntcaptcha](/src/DNTCaptcha.TestWebApp/wwwroot/Content/mode3.png) |
| SumOfTwoNumbersToWords | ![dntcaptcha](/src/DNTCaptcha.TestWebApp/wwwroot/Content/mode4.png) |

- This library uses unobtrusive Ajax library for the refresh button. Make sure you have included its related scripts
  too:
    - Add required files using the libman. To do it add [libman.json](/src/DNTCaptcha.TestWebApp/libman.json) file and
      then run the `libman restore` command.
    - Or you can download it from: https://github.com/aspnet/jquery-ajax-unobtrusive/releases

Please follow the [DNTCaptcha.TestWebApp](/src/DNTCaptcha.TestWebApp) sample for more details.

## SPA Usage

### Angular

It's possible to use this captcha with modern Angular apps too. Here is a sample to demonstrate it:

- [The server side controller](/src/DNTCaptcha.TestApiApp/Controllers/AccountController.cs)
- [The Angular DNTCaptcha component](/src/DNTCaptcha.AngularClient/src/app/dnt-captcha)
- [A sample Angular login component](/src/DNTCaptcha.AngularClient/src/app/login)
- [How to run it locally](/src/DNTCaptcha.AngularClient/README.md)

## Pure JavaScript Usage

It's possible to use this captcha with a pure JavaScript apps
too. [Here is a sample](/src/DNTCaptcha.TestApiApp/wwwroot) to demonstrate it.

## Supported Languages

Find all currently supported languages [here](/src/DNTCaptcha.Core/Language.cs). To add new language, kindly contribute
by editing the following files:

- [Language.cs](/src/DNTCaptcha.Core/Language.cs)
- [HumanReadableIntegerProvider.cs](/src/DNTCaptcha.Core/HumanReadableIntegerProvider.cs)

## How to use/create a different image provider

If you want to use another drawings library, you just need to implement
the [ICaptchaImageProvider](https://github.com/VahidN/DNTCaptcha.Core/blob/master/src/DNTCaptcha.Core/ICaptchaImageProvider.cs#L6)
service and register it as a singleton before adding `services.AddDNTCaptcha`. Your custom implementation will be used
instead of the original one.

```C#
services.AddSingleton<ICaptchaImageProvider, MyCustomCaptchaImageProvider>();
services.AddDNTCaptcha();
```

## Note

- Don't use this setting, because it will destroy the encrypted part of the captcha's token:

```C#
 services.Configure<RouteOptions>(options => { options.LowercaseQueryStrings = true; });
```
