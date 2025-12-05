using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Guardian.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure Identity tables with schema if needed
        builder.HasDefaultSchema("auth");

        // Example: Customize Identity table names if required
        builder.Entity<IdentityUser>().ToTable("users", "auth");
        builder.Entity<IdentityRole>().ToTable("roles", "auth");
        builder.Entity<IdentityUserRole<string>>().ToTable("user_roles", "auth");
        builder.Entity<IdentityUserClaim<string>>().ToTable("user_claims", "auth");
        builder.Entity<IdentityUserLogin<string>>().ToTable("user_logins", "auth");
        builder.Entity<IdentityUserToken<string>>().ToTable("user_tokens", "auth");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("role_claims", "auth");
    }
}
