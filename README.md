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