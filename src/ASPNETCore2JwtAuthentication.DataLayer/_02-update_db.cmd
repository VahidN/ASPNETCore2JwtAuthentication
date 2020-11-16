dotnet tool install --global dotnet-ef --version 5.0.0
dotnet tool update --global dotnet-ef --version 5.0.0
dotnet build
dotnet ef --startup-project ../ASPNETCore2JwtAuthentication.WebApp/ database update
pause