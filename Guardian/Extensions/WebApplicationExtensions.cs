using Guardian.Middleware;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Guardian.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseGuardianSwagger(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                }
            });
        }

        return app;
    }

    public static WebApplication UseGuardianMiddleware(this WebApplication app)
    {
        // Authentication Pipeline Middleware (3-stage validation split into smaller middleware)
        // Order matters: 
        // 1. FastClaims - validação rápida (sem assinatura, apenas expiração)
        // 2. JwtValidation - validação completa criptográfica do token (autenticação)
        // 3. CsrfValidation - valida CSRF token (apenas para requisições autenticadas e sensíveis)
        // 4. RoleAuthorization - valida roles/permissões (precisa de autenticação)
        app.UseMiddleware<FastClaimsMiddleware>();
        app.UseMiddleware<JwtValidationMiddleware>();
        app.UseMiddleware<CsrfValidationMiddleware>();
        app.UseMiddleware<RoleAuthorizationMiddleware>();

        return app;
    }
}

