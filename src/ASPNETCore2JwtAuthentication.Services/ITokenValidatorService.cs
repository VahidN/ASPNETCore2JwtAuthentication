using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace ASPNETCore2JwtAuthentication.Services;

public interface ITokenValidatorService
{
    Task ValidateAsync(TokenValidatedContext context);
}