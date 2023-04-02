namespace ASPNETCore2JwtAuthentication.DomainClasses;

public class Role
{
    public Role() => UserRoles = new HashSet<UserRole>();

    public int Id { get; set; }
    public required string Name { get; set; } = default!;

    public virtual ICollection<UserRole> UserRoles { get; set; }
}