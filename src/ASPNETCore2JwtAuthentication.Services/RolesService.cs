using ASPNETCore2JwtAuthentication.DataLayer.Context;
using ASPNETCore2JwtAuthentication.DomainClasses;
using Microsoft.EntityFrameworkCore;

namespace ASPNETCore2JwtAuthentication.Services;

public class RolesService : IRolesService
{
    private readonly DbSet<Role> _roles;
    private readonly DbSet<User> _users;

    public RolesService(IUnitOfWork uow)
    {
        if (uow is null)
        {
            throw new ArgumentNullException(nameof(uow));
        }

        _roles = uow.Set<Role>();
        _users = uow.Set<User>();
    }

    public Task<List<Role>> FindUserRolesAsync(int userId)
    {
        var userRolesQuery = from role in _roles
            from userRoles in role.UserRoles
            where userRoles.UserId == userId
            select role;

        return userRolesQuery.OrderBy(x => x.Name).ToListAsync();
    }

    public async Task<bool> IsUserInRoleAsync(int userId, string roleName)
    {
        var userRolesQuery = from role in _roles
            where role.Name == roleName
            from user in role.UserRoles
            where user.UserId == userId
            select role;
        var userRole = await userRolesQuery.FirstOrDefaultAsync();
        return userRole != null;
    }

    public Task<List<User>> FindUsersInRoleAsync(string roleName)
    {
        var roleUserIdsQuery = from role in _roles
            where role.Name == roleName
            from user in role.UserRoles
            select user.UserId;
        return _users.Where(user => roleUserIdsQuery.Contains(user.Id))
            .ToListAsync();
    }
}