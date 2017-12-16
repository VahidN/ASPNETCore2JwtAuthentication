Jwt Authentication without ASP.NET Core Identity 2.0
===========

![jwtauth](/src/ASPNETCore2JwtAuthentication.WebApp/wwwroot/images/jwtauth.png)

A Jwt based authentication sample for ASP.NET Core 2.0 without using the Identity system. It includes:

- Users and Roles tables with a many-to-may relationship.
- A separated EF Core data layer with enabled migrations.
- An EF Core 2.0 based service layer.
- A Db initializer to seed the default database values.
- An account controller with Jwt and DB based login, custom logout and refresh tokens capabilities.
- 2 sample API controllers to show how user-roles can be applied and used.
- A Jwt validator service to show how to react to the server side changes to a user's info immediately.
- [An Angular 4.3+ Client](/src/ASPNETCore2JwtAuthentication.AngularClient/).

How to run the Angular 4.3+ Client
-------------

- Install the `Angular-CLI`.
- Open a command prompt console and then navigate to src/ASPNETCore2JwtAuthentication.AngularClient/ folder.
- Now run the following commands:

```PowerShell
npm install
ng serve -o
```

- Then open another command prompt console and navigate to src/ASPNETCore2JwtAuthentication.WebApp/ folder.
- Now run the following commands:

```PowerShell
dotnet restore
dotnet watch run
```

- Finally to browse the application, navigate to http://localhost:4200
