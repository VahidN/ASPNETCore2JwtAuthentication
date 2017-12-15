using System.Security.Claims;
using ASPNETCore2JwtAuthentication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ASPNETCore2JwtAuthentication.Common;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;

namespace ASPNETCore2JwtAuthentication.WebApp.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("CorsPolicy")]
    [Authorize(Policy = CustomRoles.Admin)]
    public class MyProtectedAdminApiController : Controller
    {
        private readonly IUsersService _usersService;

        public MyProtectedAdminApiController(IUsersService usersService)
        {
            _usersService = usersService;
            _usersService.CheckArgumentIsNull(nameof(usersService));
        }

        public async Task<IActionResult> Get()
        {
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var userDataClaim = claimsIdentity.FindFirst(ClaimTypes.UserData);
            var userId = userDataClaim.Value;

            return Ok(new
            {
                Id = 1,
                Title = "Hello from My Protected Admin Api Controller!",
                Username = this.User.Identity.Name,
                UserData = userId,
                TokenSerialNumber = await _usersService.GetSerialNumberAsync(int.Parse(userId)).ConfigureAwait(false)
            });
        }
    }
}