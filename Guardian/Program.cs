using Guardian.Extensions;
using Guardian.Swagger;

var builder = WebApplication.CreateBuilder(args);

// Serviços essenciais
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// API Versioning
builder.Services.AddGuardianApiVersioning();

// Swagger
builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

// Database
builder.Services.AddGuardianDatabase(builder.Configuration);

// Identity
builder.Services.AddGuardianIdentity();

// Redis
builder.Services.AddGuardianRedis(builder.Configuration);

// Rate limiting (middleware customizado que usa Redis)
// Configuração padrão em appsettings: RateLimiting:RequestsPerWindow (int), RateLimiting:WindowSeconds (int)
builder.Services.AddSingleton<Guardian.Middleware.GuardianRateLimitMiddleware>();

// JWT Authentication
builder.Services.AddGuardianJwtAuthentication(builder.Configuration);

// AutoMapper
builder.Services.AddGuardianAutoMapper();

// FluentValidation
builder.Services.AddGuardianFluentValidation();

// Services
builder.Services.AddGuardianServices();

var app = builder.Build();

// Swagger
app.UseGuardianSwagger();

app.UseHttpsRedirection();

// Authentication & Authorization (Framework ASP.NET Core)
// UseAuthentication() valida JWT (lê de cookies ou header Authorization)
app.UseAuthentication();
app.UseAuthorization();

// Rate limiting: inserir antes de middlewares sensíveis para proteger endpoints
app.UseMiddleware<Guardian.Middleware.GuardianRateLimitMiddleware>();

// CSRF Validation (proteção adicional para requisições sensíveis)
app.UseGuardianMiddleware();

app.MapControllers();

app.Run();
