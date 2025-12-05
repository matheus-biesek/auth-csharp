using System;
using Microsoft.AspNetCore.Identity;

namespace Guardian.Models;

/// <summary>
/// Modelo de usuário que estende IdentityUser do ASP.NET Identity.
/// Permite adicionar campos customizados além dos padrão do framework.
/// As roles são gerenciadas via IdentityRole e IdentityUserRole pelo framework.
/// </summary>
public class User : IdentityUser
{
    /// <summary>
    /// Data e hora de criação da conta (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data e hora da última atualização do perfil (UTC).
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Indica se a conta está ativa (soft delete).
    /// </summary>
    public bool IsActive { get; set; } = true;
}

