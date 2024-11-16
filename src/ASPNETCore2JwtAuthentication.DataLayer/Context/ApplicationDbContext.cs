using ASPNETCore2JwtAuthentication.DomainClasses;
using Microsoft.EntityFrameworkCore;

namespace ASPNETCore2JwtAuthentication.DataLayer.Context;

public class ApplicationDbContext(DbContextOptions options) : DbContext(options), IUnitOfWork
{
    public virtual DbSet<User> Users { set; get; } = default!;

    public virtual DbSet<Role> Roles { set; get; } = default!;

    public virtual DbSet<UserRole> UserRoles { get; set; } = default!;

    public virtual DbSet<UserToken> UserTokens { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        // it should be placed here, otherwise it will rewrite the following settings!
        base.OnModelCreating(modelBuilder);

        // Custom application mappings
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Username).HasMaxLength(maxLength: 450).IsRequired();
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.Password).IsRequired();
            entity.Property(e => e.SerialNumber).HasMaxLength(maxLength: 450);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(maxLength: 450).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new
            {
                e.UserId,
                e.RoleId
            });

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.RoleId);
            entity.Property(e => e.UserId);
            entity.Property(e => e.RoleId);
            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles).HasForeignKey(d => d.RoleId);
            entity.HasOne(d => d.User).WithMany(p => p.UserRoles).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<UserToken>(entity =>
        {
            entity.HasOne(ut => ut.User).WithMany(u => u.UserTokens).HasForeignKey(ut => ut.UserId);

            entity.Property(ut => ut.RefreshTokenIdHash).HasMaxLength(maxLength: 450).IsRequired();
            entity.Property(ut => ut.RefreshTokenIdHashSource).HasMaxLength(maxLength: 450);
        });
    }
}