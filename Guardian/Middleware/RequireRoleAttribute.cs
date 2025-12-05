using System;

namespace Guardian.Middleware
{
    /// <summary>
    /// Attribute used to mark endpoints that require a specific role.
    /// Kept in its own file so controllers can reference it directly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class RequireRoleAttribute : Attribute
    {
        public string Role { get; }

        public RequireRoleAttribute(string role) => Role = role;
    }
}
