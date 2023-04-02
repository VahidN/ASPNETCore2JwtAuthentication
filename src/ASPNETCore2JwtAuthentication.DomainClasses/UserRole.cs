namespace ASPNETCore2JwtAuthentication.DomainClasses;

public class UserRole
{
    public int UserId { get; set; }
    public int RoleId { get; set; }

    public virtual User User { get; set; } = default!;
    public virtual Role Role { get; set; } = default!;
}