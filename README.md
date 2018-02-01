DNTCaptcha.Core
=======
`DNTCaptcha.Core` is a captcha generator and validator for ASP.NET Core applications.

![dntcaptcha](/src/DNTCaptcha.TestWebApp/Content/dntcaptcha.png)


Install via NuGet
-----------------
To install DNTCaptcha.Core, run the following command in the Package Manager Console:

```
PM> Install-Package DNTCaptcha.Core
```

You can also view the [package page](http://www.nuget.org/packages/DNTCaptcha.Core/) on NuGet.



Usage:
-----------------
- After installing the DNTCaptcha.Core package, add the following definition to the [_ViewImports.cshtml](/src/DNTCaptcha.TestWebApp/Views/_ViewImports.cshtml) file:
```csharp
@addTagHelper *, DNTCaptcha.Core
```

- Then to use it, add its new tag-helper to [your view](/src/DNTCaptcha.TestWebApp/Views/Home/_LoginFormBody.cshtml):
```xml
<dnt-captcha asp-captcha-generator-max="9000"
             asp-captcha-generator-min="1"
             asp-captcha-generator-language="English"
             asp-placeholder="Security code as a number"
             asp-validation-error-message="Please enter the security code as a number."
             asp-font-name="Tahoma"
             asp-font-size="20"
             asp-fore-color="#333333"
             asp-back-color="#ccc"
             asp-text-box-class="text-box single-line form-control col-md-4"
             asp-text-box-template="<div class='input-group col-md-4'><span class='input-group-addon'><span class='glyphicon glyphicon-lock'></span></span>{0}</div>"
             asp-validation-message-class="text-danger"
             asp-refresh-button-class="glyphicon glyphicon-refresh btn-sm"
             />
```

- To register its default providers, call `services.AddDNTCaptcha();` method in your [Startup class](/src/DNTCaptcha.TestWebApp/Startup.cs).
```csharp
using DNTCaptcha.Core;

namespace DNTCaptcha.TestWebApp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDNTCaptcha();
        }
```

- Now you can add the `ValidateDNTCaptcha` attribute [to your action method](/src/DNTCaptcha.TestWebApp/Controllers/HomeController.cs) to verify the entered security code:
```csharp
[HttpPost, ValidateAntiForgeryToken]
[ValidateDNTCaptcha(ErrorMessage = "Please enter the security code as a number.",
                    IsNumericErrorMessage = "The input value should be a number.",
                    CaptchaGeneratorLanguage = Language.English)]
public IActionResult Index([FromForm]AccountViewModel data)
{
    if (ModelState.IsValid)
    {
        //TODO: Save data
        return RedirectToAction(nameof(Thanks), new { name = data.Username });
    }
    return View();
}
```

- This library uses unobtrusive Ajax library for the refresh button. Make sure you have included its related scripts too:
  *  [bower.json](https://github.com/VahidN/DNTCaptcha.Core/blob/master/src/DNTCaptcha.TestWebApp/bower.json#L9)
  *  [bundleconfig.json](https://github.com/VahidN/DNTCaptcha.Core/blob/master/src/DNTCaptcha.TestWebApp/bundleconfig.json#L15)
  * Or include it from a CDN (It requires jQuery 1.4.4+):
  ```javascript
  <script type="text/javascript" src="http://ajax.aspnetcdn.com/ajax/mvc/3.0/jquery.unobtrusive-ajax.min.js"></script>
  ```

Please follow the [DNTCaptcha.TestWebApp](/src/DNTCaptcha.TestWebApp) sample for more details.



SPA Usage:
----------

It's possible to use this captcha with Angular 4.3+ apps too. Here is a sample to demonstrate it:
- [The server side controller](/src/DNTCaptcha.TestWebApp/Controllers/NgxController.cs)
- [The Angular 4.3+ component](/src/DNTCaptcha.AngularClient/src/app/dnt-captcha)
- [A sample Angular 4.3+ login page](/src/DNTCaptcha.AngularClient/src/app/users-login)



Note:
-----------------
To run this project on non-Windows-based operating systems, you will need to install `libgdiplus` too:
- Ubuntu 16.04 and above:
	- apt-get install libgdiplus
	- cd /usr/lib
	- ln -s libgdiplus.so gdiplus.dll
- Fedora 23 and above:
	- dnf install libgdiplus
	- cd /usr/lib64/
	- ln -s libgdiplus.so.0 gdiplus.dll
- CentOS 7 and above:
	- yum install autoconf automake libtool
	- yum install freetype-devel fontconfig libXft-devel
	- yum install libjpeg-turbo-devel libpng-devel giflib-devel libtiff-devel libexif-devel
	- yum install glib2-devel cairo-devel
	- git clone https://github.com/mono/libgdiplus
	- cd libgdiplus
	- ./autogen.sh
	- make
	- make install
	- cd /usr/lib64/
	- ln -s /usr/local/lib/libgdiplus.so gdiplus.dll
- Docker
	- RUN apt-get update \\

      && apt-get install -y libgdiplus
- MacOS
	- brew install mono-libgdiplus

      After installing the [Mono MDK](http://www.mono-project.com/download/#download-mac), Copy Mono MDK Files:
	   - /Library/Frameworks/Mono.framework/Versions/4.6.2/lib/libgdiplus.0.dylib
	   - /Library/Frameworks/Mono.framework/Versions/4.6.2/lib/libgdiplus.0.dylib.dSYM
	   - /Library/Frameworks/Mono.framework/Versions/4.6.2/lib/libgdiplus.dylib
	   - /Library/Frameworks/Mono.framework/Versions/4.6.2/lib/libgdiplus.la

      And paste them to: /usr/local/lib
