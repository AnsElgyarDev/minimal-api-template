# Minimal API Template

[![NuGet](https://img.shields.io/nuget/v/AnsElgyar.Templates?logo=nuget)](https://www.nuget.org/packages/AnsElgyar.Templates)
[![NuGet downloads](https://img.shields.io/nuget/dt/AnsElgyar.Templates?logo=nuget)](https://www.nuget.org/packages/AnsElgyar.Templates)
[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)](https://dotnet.microsoft.com/download)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

A `dotnet new` template that scaffolds a ready-to-run ASP.NET Core Minimal API (.NET 10) with JWT authentication, EF Core on SQL Server, tests and a Dockerfile. The project name you pass with `-n` is applied to the solution, folders, project files, namespaces, Dockerfile, database name, and JWT issuer/audience.

```
dotnet new my-minimal-api -n ShopApp
```

## What you get

- Minimal API with endpoint groups (`/auth`, `/users`) and a `/health` endpoint
- Register and login with hashed passwords (ASP.NET Core `PasswordHasher`) and JWT bearer tokens
- Role-based policies (`AdminOnly`, `AuthenticatedUser`)
- EF Core + SQL Server with an `InitialCreate` migration (`Users` table)
- Global exception handler (RFC 7807 problem details) and request logging middleware
- AES-256-GCM `EncryptionService` with a validated key
- Settings validated at startup (`ValidateOnStart`): the app refuses to start with a missing or weak key
- Scalar API reference at `/scalar/v1` (Development only)
- xUnit tests that run the real API against an in-memory SQLite database
- Multi-stage `Dockerfile` (non-root user) and `.dockerignore`
- A fresh `UserSecretsId` and random local ports for every generated project

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server. The default connection string uses **LocalDB (Windows only)**; see [Database](#database) to use another instance.
- `dotnet-ef` tool: `dotnet tool install --global dotnet-ef`
- Docker (optional)

## Install

From source:

```
git clone https://github.com/AnsElgyarDev/minimal-api-template.git
cd minimal-api-template
dotnet new install ./templates/minimal-api
```

From NuGet (works once the package is published):

```
dotnet new install AnsElgyar.Templates
```

Check that it is registered:

```
dotnet new list my-minimal-api
```

## Create a project

```
dotnet new my-minimal-api -n ShopApp
cd ShopApp
dotnet ef database update --project src/ShopApp
dotnet run --project src/ShopApp
```

> All commands in the sections below are run from the **generated project's root** (the `ShopApp/` folder), which is why they use `--project src/ShopApp`.

Open the URL printed in the console, then `/scalar/v1`. In Development, `/` redirects there.

**Project name rules:** use letters, digits and dots only (for example `ShopApp` or `Acme.Shop`). Hyphens and spaces break the generated solution, because file names keep the hyphen while the namespaces do not.

### Template parameters

| Parameter        | Default            | Description                                               |
| ---------------- | ------------------ | --------------------------------------------------------- |
| `-n`, `--name`   | folder name        | Project name. Replaces `MyCompany.MinimalApi` everywhere. |
| `-o`, `--output` | current folder     | Output directory.                                         |
| `--HttpPort`     | random (5000-5300) | HTTP port in `launchSettings.json`.                       |
| `--HttpsPort`    | random (7000-7300) | HTTPS port in `launchSettings.json`.                      |

Preview without writing files: `dotnet new my-minimal-api -n ShopApp --dry-run`

## Generated structure

```
ShopApp/
├── ShopApp.slnx
├── Dockerfile
├── .dockerignore
├── .gitignore
├── src/ShopApp/
│   ├── Program.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json     # development-only keys
│   ├── Data/                            # AppDbContext, entity configurations
│   ├── Dtos/
│   ├── Endpoints/                       # AuthEndpoints, UserEndpoints
│   ├── Helpers/                         # policies, JWT setup
│   ├── Middlewares/                     # exception handler, request log
│   ├── Migrations/
│   ├── Models/                          # User
│   ├── Properties/launchSettings.json
│   ├── Repository/
│   └── Services/                        # TokenService, EncryptionService, settings
└── tests/ShopApp.Tests/
    ├── ApiFactory.cs                    # real API + in-memory SQLite
    ├── AuthFlowTests.cs
    ├── EncryptionServiceTests.cs
    └── HealthEndpointTests.cs
```

## Endpoints

| Method | Path             | Access             | Description                                                                       |
| ------ | ---------------- | ------------------ | --------------------------------------------------------------------------------- |
| GET    | `/health`        | public             | Liveness check (does not touch the database).                                     |
| POST   | `/auth/register` | public             | Body: `{ "email": "...", "password": "..." }`. Password must be 8-128 characters. |
| POST   | `/auth/login`    | public             | Returns `{ "token", "expiresAt" }`.                                               |
| GET    | `/users/me`      | any signed-in user | Returns the current user's `id`, `email`, `role`.                                 |
| GET    | `/users`         | `Admin` role       | Lists all users.                                                                  |

Send the token as `Authorization: Bearer <token>`.

## Configuration

| Key                                               | Environment variable                                 | Where it comes from                                                             |
| ------------------------------------------------- | ---------------------------------------------------- | ------------------------------------------------------------------------------- |
| `ConnectionStrings:DefaultConnection`             | `ConnectionStrings__DefaultConnection`               | `appsettings.json` (LocalDB)                                                    |
| `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpiryMinutes` | `Jwt__Issuer`, `Jwt__Audience`, `Jwt__ExpiryMinutes` | `appsettings.json`                                                              |
| `Jwt:Key` (at least 32 characters)                | `Jwt__Key`                                           | `appsettings.Development.json` in Development; **you must supply it elsewhere** |
| `EncryptionSettings:Key` (Base64 of 32 bytes)     | `EncryptionSettings__Key`                            | `appsettings.Development.json` in Development; **you must supply it elsewhere** |

`appsettings.Development.json` contains keys that are public in this repository. They are loaded only when `ASPNETCORE_ENVIRONMENT=Development`. **Never reuse them outside local development.** In any other environment the app fails at startup until real keys are provided.

### Generate keys

```powershell
# JWT key (64 characters)
[Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(48))

# Encryption key (Base64 of exactly 32 bytes)
[Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
```

On Linux or macOS: `openssl rand -base64 48` and `openssl rand -base64 32`.

### User secrets (local) and environment variables (production)

```
dotnet user-secrets set "Jwt:Key" "<your-key>" --project src/ShopApp
dotnet user-secrets set "EncryptionSettings:Key" "<your-key>" --project src/ShopApp
```

```
# Linux / macOS
export Jwt__Key="<your-key>"

# Windows PowerShell
$env:Jwt__Key = "<your-key>"
```

## Database

The default connection string targets `(localdb)\MSSQLLocalDB` with a database named after your project. LocalDB exists on Windows only. To use another SQL Server instance, override the connection string without touching any file in the repo:

```
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=.\SQLEXPRESS;Database=ShopApp;Trusted_Connection=True;TrustServerCertificate=True" --project src/ShopApp
```

Apply migrations and add new ones:

```
dotnet ef database update --project src/ShopApp
dotnet ef migrations add AddProducts --project src/ShopApp
```

### Make a user an admin

Registration always creates a normal user. Promote one in SQL, then log in again (the role is stored inside the token):

```sql
UPDATE Users SET Role = 'Admin' WHERE Email = 'you@example.com';
```

## Tests

```
dotnet test
```

The tests start the real API in-process (`ApiFactory`) with test keys and an in-memory SQLite database, so they never touch your SQL Server or user secrets. SQLite is not SQL Server: avoid SQL Server-specific column types in entity configurations, or the tests will fail to create the schema.

## Docker

Build from the generated project's root:

```
docker build -t shopapp .
```

The image runs in the **Production** environment, so you must pass the keys and a connection string that works from inside a container (a SQL login, not `Trusted_Connection`; Windows authentication does not work in Linux containers).

PowerShell:

```powershell
docker run --rm -p 8080:8080 `
  -e "Jwt__Key=<your-jwt-key>" `
  -e "EncryptionSettings__Key=<your-encryption-key>" `
  -e "ConnectionStrings__DefaultConnection=Server=host.docker.internal,1433;Database=ShopApp;User Id=sa;Password=<password>;TrustServerCertificate=True" `
  shopapp
```

Bash (Linux / macOS):

```bash
docker run --rm -p 8080:8080 \
  -e "Jwt__Key=<your-jwt-key>" \
  -e "EncryptionSettings__Key=<your-encryption-key>" \
  -e "ConnectionStrings__DefaultConnection=Server=host.docker.internal,1433;Database=ShopApp;User Id=sa;Password=<password>;TrustServerCertificate=True" \
  shopapp
```

Notes:

- The container listens on HTTP port 8080 and expects TLS to be terminated by a reverse proxy. `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true` is set in the image, so it trusts the proxy's forwarded headers.
- `appsettings.Development.json` is excluded by `.dockerignore`, so the development keys never reach the image.
- Scalar is not exposed in Production.
- The Docker host needs TCP/IP enabled on your SQL Server instance.
- On Linux, `host.docker.internal` may need `--add-host=host.docker.internal:host-gateway` added to `docker run`.

## What gets renamed

| In the template                         | In the generated project                |
| --------------------------------------- | --------------------------------------- |
| `MyCompany.MinimalApi.slnx`             | `ShopApp.slnx`                          |
| `src/MyCompany.MinimalApi/`, `*.csproj` | `src/ShopApp/`, `ShopApp.csproj`        |
| `tests/MyCompany.MinimalApi.Tests/`     | `tests/ShopApp.Tests/`                  |
| Namespaces and `using` lines            | `ShopApp...`                            |
| Dockerfile paths and entry point        | `ShopApp...`                            |
| Database name, JWT issuer and audience  | `ShopApp`, `ShopApp.Clients`            |
| `UserSecretsId`                         | New GUID per project                    |
| Ports in `launchSettings.json`          | Random, or `--HttpPort` / `--HttpsPort` |

## Developing the template

`templates/minimal-api` is a real, working project. Build, test and run it directly:

```
cd templates/minimal-api
dotnet test
dotnet run --project src/MyCompany.MinimalApi
```

Test the template itself (from the repository root):

```
dotnet new install ./templates/minimal-api
dotnet new my-minimal-api -n TestApp -o ../TestApp
cd ../TestApp
dotnet build
dotnet test
```

After changing the template, reinstall it:

```
dotnet new uninstall ./templates/minimal-api
dotnet new install ./templates/minimal-api
```

If the template engine shows stale results: `dotnet new --debug:reinit`.

Keep the text `MyCompany.MinimalApi` as the only form of the project name in the template. The engine replaces that exact text; other spellings are left untouched.

## Packaging and publishing

```
dotnet pack -c Release -o ./artifacts
dotnet new install ./artifacts/AnsElgyar.Templates.<version>.nupkg
```

Generate a project from the installed package and build it before publishing. Then:

```
dotnet nuget push ./artifacts/AnsElgyar.Templates.<version>.nupkg --api-key <YOUR_API_KEY> --source https://api.nuget.org/v3/index.json
```

A published version cannot be overwritten. Increase `<Version>` in `MyTemplates.csproj` for every release. Never commit your API key.

## Update and uninstall

```
dotnet new update
dotnet new uninstall AnsElgyar.Templates
```

## Troubleshooting

| Problem                                                                   | Solution                                                                                                   |
| ------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------- |
| `Unable to locate a Local Database Runtime installation`                  | LocalDB is not installed. Install it, or override the connection string (see [Database](#database)).       |
| App stops at startup with a `Jwt:Key` or `EncryptionSettings:Key` message | The key is missing or invalid. Set it with user secrets or an environment variable.                        |
| `dotnet build` cannot find `Shop_App.csproj`                              | The project name contained a hyphen. Use letters, digits and dots only.                                    |
| Template not found after install                                          | Run `dotnet new list my-minimal-api`. If empty, check that `.template.config/template.json` is valid JSON. |
| `MyCompany` still appears in a generated project                          | Search the output for it and make sure that file uses exactly `MyCompany.MinimalApi`.                      |
| Changes to the template are not applied                                   | Uninstall, reinstall, then run `dotnet new --debug:reinit`.                                                |
| `Failed to determine the https port for redirect` warning                 | Harmless when running over HTTP only or behind a proxy.                                                    |
| `dotnet ef` says the tools are older than the runtime                     | `dotnet tool update --global dotnet-ef`.                                                                   |

## License

MIT. See the [LICENSE](LICENSE) file for details.

## Contributing

Issues and pull requests are welcome.

1. Fork the repository and create a branch from `main`.
2. Make your change inside `templates/minimal-api` and run `dotnet test` there.
3. Generate a project from the template and make sure it builds (see [Developing the template](#developing-the-template)).
4. Open a pull request describing what changed and why.

For bigger changes, please open an issue first so we can discuss the idea.

## Author

Created by [AnsElgyarDev](https://github.com/AnsElgyarDev).

If this template saved you time, a star on the repository is appreciated.
