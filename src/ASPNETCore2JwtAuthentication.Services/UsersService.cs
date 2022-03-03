using System.Security.Claims;
using ASPNETCore2JwtAuthentication.DataLayer.Context;
using ASPNETCore2JwtAuthentication.DomainClasses;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ASPNETCore2JwtAuthentication.Services;

public class UsersService : IUsersService
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly ISecurityService _securityService;
    private readonly IUnitOfWork _uow;
    private readonly DbSet<User> _users;

    public UsersService(
        IUnitOfWork uow,
        ISecurityService securityService,
        IHttpContextAccessor contextAccessor)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        _users = _uow.Set<User>();
        _securityService = securityService ?? throw new ArgumentNullException(nameof(securityService));
        _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
    }

    public ValueTask<User> FindUserAsync(int userId)
    {
        return _users.FindAsync(userId);
    }

    public Task<User> FindUserAsync(string username, string password)
    {
        var passwordHash = _securityService.GetSha256Hash(password);
        return _users.FirstOrDefaultAsync(x => x.Username == username && x.Password == passwordHash);
    }

    public async Task<string> GetSerialNumberAsync(int userId)
    {
        var user = await FindUserAsync(userId);
        return user.SerialNumber;
    }

    public async Task UpdateUserLastActivityDateAsync(int userId)
    {
        var user = await FindUserAsync(userId);
        if (user.LastLoggedIn != null)
        {
            var updateLastActivityDate = TimeSpan.FromMinutes(2);
            var currentUtc = DateTimeOffset.UtcNow;
            var timeElapsed = currentUtc.Subtract(user.LastLoggedIn.Value);
            if (timeElapsed < updateLastActivityDate)
            {
                return;
            }
        }

        user.LastLoggedIn = DateTimeOffset.UtcNow;
        await _uow.SaveChangesAsync();
    }

    public int GetCurrentUserId()
    {
        var claimsIdentity = _contextAccessor.HttpContext?.User.Identity as ClaimsIdentity;
        var userDataClaim = claimsIdentity?.FindFirst(ClaimTypes.UserData);
        var userId = userDataClaim?.Value;
        return string.IsNullOrWhiteSpace(userId)
            ? 0
            : int.Parse(userId, NumberStyles.Number, CultureInfo.InvariantCulture);
    }

    public ValueTask<User> GetCurrentUserAsync()
    {
        var userId = GetCurrentUserId();
        return FindUserAsync(userId);
    }

    public async Task<(bool Succeeded, string Error)> ChangePasswordAsync(User user, string currentPassword,
        string newPassword)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        var currentPasswordHash = _securityService.GetSha256Hash(currentPassword);
        if (!string.Equals(user.Password, currentPasswordHash, StringComparison.Ordinal))
        {
            return (false, "Current password is wrong.");
        }

        user.Password = _securityService.GetSha256Hash(newPassword);
        // user.SerialNumber = Guid.NewGuid().ToString("N"); // To force other logins to expire.
        await _uow.SaveChangesAsync();
        return (true, string.Empty);
    }
}