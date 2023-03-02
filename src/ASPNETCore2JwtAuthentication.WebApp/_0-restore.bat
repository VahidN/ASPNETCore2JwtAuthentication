rmdir /S /Q bin
rmdir /S /Q obj
dotnet restore
dotnet tool update -g Microsoft.Web.LibraryManager.Cli
libman restore
pause