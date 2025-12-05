# Guardian

**Projeto:** Guardian — serviço de autenticação e autorização em ASP.NET Core 8 com Identity, JWT (access + refresh + CSRF), e armazenamento de refresh tokens em Redis.

**Resumo:**
- Autenticação baseada em JWT com rotação de refresh tokens.
- Integração com `ASP.NET Core Identity` (User/Role).
- Refresh tokens persistidos em Redis com mapeamento bidirecional para permitir revogação e lookup eficiente.
- Endpoints para registro, login, refresh, logout e operações administrativas (revogar/listar tokens).
- Rate limiting distribuído (middleware) usando Redis (fixed window).

**Stack principal:**
- .NET 8 / ASP.NET Core 8
- Entity Framework Core (PostgreSQL via Npgsql)
- Microsoft.AspNetCore.Identity
- StackExchange.Redis
- FluentValidation
- AutoMapper

**Arquitetura (visão rápida):**
- `Guardian/` — projeto principal
- `Controllers/v1/` — endpoints HTTP versionados
- `Services/v1/` — lógica de autenticação e integração com Redis/Identity
- `Middleware/` — middlewares personalizados (CSRF, RateLimit, etc.)
- `Models/Auth/v1/` — DTOs e modelos de transporte

**Requisitos de desenvolvimento:**
- .NET 8 SDK
- PostgreSQL acessível (configurar em `appsettings.json`)
- Redis acessível (configurar `ConnectionStrings:Redis` em `appsettings*.json`)

**Configuração (local):**
1. Copie o arquivo de configuração de exemplo (se houver) e ajuste conexões:

```bash
cp Guardian/appsettings.Development.json.example Guardian/appsettings.Development.json
# Editar a conexão com Redis e PostgreSQL
```

2. Defina `ConnectionStrings` e `RateLimiting` conforme necessário no `appsettings.*.json`:

- `ConnectionStrings:Redis` — ex.: `localhost:6379`
- `ConnectionStrings:DefaultConnection` — string do PostgreSQL
- `RateLimiting:RequestsPerWindow` — int (padrão 60)
- `RateLimiting:WindowSeconds` — int (padrão 60)

**Executando localmente:**

```bash
# Restaurar dependências
dotnet restore

# Build
dotnet build

# Executar
dotnet run --project Guardian
```

A aplicação expõe endpoints versionados em `/api/v1/...`.

**Migrações EF Core:**
- Para criar uma migration:

```bash
dotnet ef migrations add NomeDaMigration -p Guardian -s Guardian
```

- Para aplicar migrations:

```bash
dotnet ef database update -p Guardian -s Guardian
```

(Ajuste parâmetros de projeto/solução conforme seu layout.)

**Autenticação e tokens:**
- Fluxo: `POST /api/v1/auth/register` (criar usuário) → `POST /api/v1/auth/login` (gera `accessToken` + `refreshToken` + `csrfToken`).
- `accessToken` e `refreshToken` são tratados via cookies (`HttpOnly`), `csrfToken` é exposto como cookie legível para JS.
- Refresh tokens são armazenados em Redis com chaves:
  - `refresh_token:{userId}` => `{refreshToken}`
  - `refresh_lookup:{refreshToken}` => `{userId}`
- Rotação: quando um refresh é usado, o sistema gera um novo refresh e remove o lookup antigo.
- Revogação: existe endpoint admin para revogar o refresh token de um usuário por email.

**Endpoints importantes (resumo):**
- `POST /api/v1/auth/register` — registrar usuário (público)
- `POST /api/v1/auth/login` — autenticar (público)
- `POST /api/v1/auth/refresh` — renovar tokens (usa cookie `refreshToken`)
- `POST /api/v1/auth/logout` — encerrar sessão
- `POST /api/v1/auth/revoke-token` — revogar refresh token (Admin)
- `GET  /api/v1/auth/refresh-tokens` — listar emails que têm refresh token ativo (Admin)

**Rate limiting:**
- Implementado como middleware `GuardianRateLimitMiddleware` usando Redis.
- Comportamento padrão:
  - Identificador por usuário autenticado (claim `NameIdentifier`) ou por IP quando anônimo.
  - Fixed window: `RateLimiting:RequestsPerWindow` requisições por `RateLimiting:WindowSeconds` segundos.
  - Cabeçalho `Retry-After` retornado em `429 Too Many Requests`.
- Configurações podem ser alteradas em `appsettings.*.json`.

**Considerações de segurança e operação:**
- Proteja `appsettings.*.json` que contenha segredos (usar variáveis de ambiente ou secret manager em produção).
- Cookies com tokens são configurados com `HttpOnly` e `SameSite` — revise `Program.cs` para ajustar política de `Secure` conforme ambiente.
- O middleware de rate limiting usa Redis; quando Redis estiver indisponível o comportamento atual é "fail open" (permite requests) e registra avisos — altere conforme necessidade operacional.

**Testes:**
- Existem testes básicos em `Guardian.Tests/` — rode com:

```bash
dotnet test
```

**Contribuição:**
- Use branches temáticas (`feature/`, `fix/`, `hotfix/`).
- Faça commits claros em português (padrão do repositório) e abra PRs para revisão.

**Push para remoto:**
- Para empurrar `main` ao remoto (ex.: `origin`):

```bash
git push --set-upstream origin main
```

**Contato / Suporte:**
- Para dúvidas sobre a arquitetura ou fluxos de autenticação, abra uma issue no repositório ou envie mensagem para o mantenedor.

---

Este README foi adicionado automaticamente — posso ajustar o conteúdo (mais detalhes de operação, exemplos de requests, documentação OpenAPI/Swagger) se desejar.