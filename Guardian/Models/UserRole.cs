namespace Guardian.Models;

/// <summary>
/// Enum de roles do sistema para validação e referência em lógica de negócio.
/// As roles são persistidas em banco via IdentityRole, mas este enum garante
/// tipagem forte no código.
/// </summary>
public enum UserRoleEnum
{
    /// <summary>
    /// Usuário padrão com permissões limitadas.
    /// </summary>
    User = 1,

    /// <summary>
    /// Administrador com acesso total ao sistema.
    /// </summary>
    Admin = 2
}

