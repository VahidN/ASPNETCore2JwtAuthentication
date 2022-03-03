dotnet tool update --global dotnet-ef --version 6.0.2
dotnet build
For /f "tokens=2-4 delims=/ " %%a in ('date /t') do (set mydate=%%c_%%a_%%b)
For /f "tokens=1-2 delims=/:" %%a in ("%TIME: =0%") do (set mytime=%%a%%b)
dotnet ef migrations --startup-project ../ASPNETCore2JwtAuthentication.WebApp/ add V%mydate%_%mytime%
pause