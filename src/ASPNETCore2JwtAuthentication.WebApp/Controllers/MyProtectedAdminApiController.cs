using System.Security.Claims;
using ASPNETCore2JwtAuthentication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace ASPNETCore2JwtAuthentication.WebApp.Controllers;

[Route(template: "api/[controller]")]
[EnableCors(policyName: "CorsPolicy")]
[Authorize(Policy = CustomRoles.Admin)]
public class MyProtectedAdminApiController(IUsersService usersService) : Controller
{
    private readonly IUsersService
        _usersService = usersService ?? throw new ArgumentNullException(nameof(usersService));

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var claimsIdentity = User.Identity as ClaimsIdentity;
        var userDataClaim = claimsIdentity?.FindFirst(ClaimTypes.UserData);
        var userId = userDataClaim?.Value;

        return Ok(new
        {
            Id = 1,
            Title = "Hello from My Protected Admin Api Controller! [Authorize(Policy = CustomRoles.Admin)]",
            Username = User.Identity?.Name,
            UserData = userId,
            TokenSerialNumber =
                await _usersService.GetSerialNumberAsync(int.Parse(userId ?? "0", NumberStyles.Number,
                    CultureInfo.InvariantCulture)),
            Roles = claimsIdentity?.Claims.Where(x => string.Equals(x.Type, ClaimTypes.Role, StringComparison.Ordinal))
                .Select(x => x.Value)
                .ToList()
        });
    }
}