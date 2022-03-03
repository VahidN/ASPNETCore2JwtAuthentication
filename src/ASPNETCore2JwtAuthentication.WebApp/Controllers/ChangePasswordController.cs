using ASPNETCore2JwtAuthentication.Services;
using ASPNETCore2JwtAuthentication.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace ASPNETCore2JwtAuthentication.WebApp.Controllers;

[Authorize, Route("api/[controller]"), EnableCors("CorsPolicy")]
public class ChangePasswordController : Controller
{
    private readonly IUsersService _usersService;

    public ChangePasswordController(IUsersService usersService)
    {
        _usersService = usersService ?? throw new ArgumentNullException(nameof(usersService));
    }

    [HttpPost, ValidateAntiForgeryToken]
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
            return BadRequest("NotFound");
        }

        var (succeeded, error) = await _usersService.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
        if (succeeded)
        {
            return Ok();
        }

        return BadRequest(error);
    }
}