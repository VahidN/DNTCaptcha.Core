rmdir /S /Q bin
rmdir /S /Q obj
dotnet tool update -g Microsoft.Web.LibraryManager.Cli
libman install bootstrap --provider unpkg --destination wwwroot/lib/bootstrap
libman install bootstrap-icons --provider unpkg --destination wwwroot/lib/bootstrap-icons
libman install jquery --provider unpkg --destination wwwroot/lib/jquery
libman install jquery-ajax-unobtrusive --provider unpkg --destination wwwroot/lib/jquery-ajax-unobtrusive
libman install jquery-validation --provider unpkg --destination wwwroot/lib/jquery-validation
libman install jquery-validation-unobtrusive --provider unpkg --destination wwwroot/lib/jquery-validation-unobtrusive
libman restore
dotnet restore
pause