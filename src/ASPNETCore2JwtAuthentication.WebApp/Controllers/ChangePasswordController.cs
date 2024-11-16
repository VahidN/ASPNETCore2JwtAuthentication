using ASPNETCore2JwtAuthentication.Models;
using ASPNETCore2JwtAuthentication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace ASPNETCore2JwtAuthentication.WebApp.Controllers;

[Authorize]
[Route(template: "api/[controller]")]
[EnableCors(policyName: "CorsPolicy")]
public class ChangePasswordController(IUsersService usersService) : Controller
{
    private readonly IUsersService
        _usersService = usersService ?? throw new ArgumentNullException(nameof(usersService));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Post([FromBody] ChangePasswordViewModel model)
    {
        if (model == null)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _usersService.GetCurrentUserAsync();

        if (user == null)
        {
            return BadRequest(error: "NotFound");
        }

        var (succeeded, error) = await _usersService.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

        if (succeeded)
        {
            return Ok();
        }

        return BadRequest(error);
    }
}