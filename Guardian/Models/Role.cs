using System;
using Microsoft.AspNetCore.Identity;

namespace Guardian.Models;

/// <summary>
/// Modelo de Role que estende IdentityRole do ASP.NET Identity.
/// Gerencia as roles (permissões) do sistema e seu relacionamento com usuários.
/// </summary>
public class Role : IdentityRole
{
    /// <summary>
    /// Descrição da role (para documentação e referência).
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Data de criação da role.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indica se a role está ativa (pode ser desativada sem deletar dados).
    /// </summary>
    public bool IsActive { get; set; } = true;
}

