dotnet tool install --global dotnet-ef --version 6.0.2
dotnet tool update --global dotnet-ef --version 6.0.2
dotnet build
dotnet ef --startup-project ../ASPNETCore2JwtAuthentication.WebApp/ database update
pause