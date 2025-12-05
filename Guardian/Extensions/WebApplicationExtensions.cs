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
        // CSRF Validation Middleware
        // Valida CSRF token em requisições sensíveis (POST, PUT, DELETE, PATCH)
        // Compara token do cookie com token do header X-CSRF-Token
        // Deve ser executado APÓS UseAuthentication() para garantir que o usuário está autenticado
        app.UseMiddleware<CsrfValidationMiddleware>();

        return app;
    }
}

