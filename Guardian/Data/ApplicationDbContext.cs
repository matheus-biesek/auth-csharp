using Guardian.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Guardian.Data;

/// <summary>
/// DbContext padrão do ASP.NET Identity com User e Role customizados.
/// Herança de IdentityDbContext&lt;User, Role&gt; mantém todas as tabelas necessárias
/// para o sistema de autenticação e autorização do Identity Framework.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<User, Role, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Define schema padrão
        builder.HasDefaultSchema("auth");

        // ============================================================
        // Configuração da tabela Users
        // ============================================================
        builder.Entity<User>(entity =>
        {
            entity.ToTable("users", "auth");

            // Configuração de campos obrigatórios
            entity.Property(e => e.UserName)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(e => e.NormalizedUserName)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(e => e.NormalizedEmail)
                .IsRequired()
                .HasMaxLength(256);

            // Campos customizados
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Índices para performance
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.IsActive);
        });

        // ============================================================
        // Configuração da tabela Roles
        // ============================================================
        builder.Entity<Role>(entity =>
        {
            entity.ToTable("roles", "auth");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(e => e.NormalizedName)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(e => e.Description)
                .HasMaxLength(500);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Seed roles padrão
            entity.HasData(
                new Role
                {
                    Id = "1",
                    Name = "User",
                    NormalizedName = "USER",
                    Description = "Usuário padrão com permissões limitadas",
                    IsActive = true
                },
                new Role
                {
                    Id = "2",
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    Description = "Administrador com acesso total ao sistema",
                    IsActive = true
                }
            );
        });

        // ============================================================
        // Configuração da tabela UserRoles (relacionamento M:M)
        // ============================================================
        builder.Entity<IdentityUserRole<string>>(entity =>
        {
            entity.ToTable("user_roles", "auth");

            entity.HasKey(e => new { e.UserId, e.RoleId });

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Role>()
                .WithMany()
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ============================================================
        // Configuração de outras tabelas do Identity (necessárias para o framework)
        // ============================================================
        builder.Entity<IdentityUserClaim<string>>(entity =>
        {
            entity.ToTable("user_claims", "auth");
        });

        builder.Entity<IdentityUserLogin<string>>(entity =>
        {
            entity.ToTable("user_logins", "auth");
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });
        });

        builder.Entity<IdentityUserToken<string>>(entity =>
        {
            entity.ToTable("user_tokens", "auth");
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });
        });

        builder.Entity<IdentityRoleClaim<string>>(entity =>
        {
            entity.ToTable("role_claims", "auth");
        });
    }
}
