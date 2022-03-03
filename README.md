Jwt Authentication without ASP.NET Core Identity 6.0.102
===========

![jwtauth](/src/ASPNETCore2JwtAuthentication.WebApp/wwwroot/images/jwtauth.png)

A Jwt based authentication sample for ASP.NET Core 6.0.102 without using the Identity system. It includes:

- Users and Roles tables with a many-to-may relationship.
- A separated EF Core data layer with enabled migrations.
- An EF Core 6.0.102 based service layer.
- A Db initializer to seed the default database values.
- An account controller with Jwt and DB based login, custom logout and refresh tokens capabilities.
- 2 sample API controllers to show how user-roles can be applied and used.
- A Jwt validator service to show how to react to the server side changes to a user's info immediately.
- [An Angular 7.0+ Client](/src/ASPNETCore2JwtAuthentication.AngularClient/). It contains:
  - A JWT-based Login page.
  - Handling how and where to store the tokens.
  - How to handle refresh tokens mechanism, using reactive timers.
  - How to validate the expiration date of a token.
  - How to decode an access token and extract the user's roles from it.
  - Authorizing access to a module via a route guard based on the user's roles.
  - How to hide or display different parts of a page based on the user's roles.
  - Adding a JWT token to the HTTP requests required authorized access automatically.
  - Handling server side unauthorized errors automatically.



How to run the Angular 7.0+ Client
-------------

- Update all of the outdated global dependencies using the `npm update -g` command.
- Install the `Angular-CLI`.
- Open a command prompt console and then navigate to src/ASPNETCore2JwtAuthentication.AngularClient/ folder.
- Now run the following commands:

```PowerShell
npm update -g
npm install
_2-ng-build-dev.bat
```

- Then open another command prompt console and navigate to src/ASPNETCore2JwtAuthentication.WebApp/ folder.
- Now run the following commands:

```PowerShell
dotnet restore
dotnet watch run
```

- Finally to browse the application, navigate to https://localhost:5001/AngularClient
