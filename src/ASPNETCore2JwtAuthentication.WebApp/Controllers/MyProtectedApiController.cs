using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace ASPNETCore2JwtAuthentication.WebApp.Controllers;

[Route("api/[controller]"), EnableCors("CorsPolicy"), Authorize]
public class MyProtectedApiController : Controller
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            Id = 1,
            Title = "Hello from My Protected Controller! [Authorize]",
            Username = User.Identity?.Name
        });
    }
}