# Minimal API Template

A `dotnet new` template that scaffolds a ready-to-run ASP.NET Core Minimal API project with a single command. The project name you choose is applied automatically to the solution, project folder, project file, and namespaces.

```bash
dotnet new my-minimal-api -n ShopApp
```

## Features

- ASP.NET Core Minimal API, ready to build and run
- Health check endpoint at `/health`
- Configuration placeholders in `appsettings.json` (no real secrets committed)
- Startup warning when the API key is still the placeholder value
- Project name replaced everywhere automatically (solution, folders, project file, namespaces, launch profile)
- Packaged as a standard NuGet template package

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) 8.0 or later

Verify your installation:

```bash
dotnet --version
```

## Installation

### Option 1: From NuGet

```bash
dotnet new install MyTemplates
```

### Option 2: From source

```bash
git clone https://github.com/AnsElgyarDev/minimal-api-template.git
cd minimal-api-template
dotnet new install ./templates/minimal-api
```

Confirm the template is registered:

```bash
dotnet new list my-minimal-api
```

## Usage

Create a new project:

```bash
dotnet new my-minimal-api -n ShopApp
cd ShopApp
dotnet run --project src/ShopApp
```

The console output shows the URL the app is listening on. Then open:

- `/` returns a hello message
- `/health` returns `{"status":"healthy"}`

Create the project in a specific folder:

```bash
dotnet new my-minimal-api -n ShopApp -o D:\Projects\ShopApp
```

Preview what would be generated without writing any files:

```bash
dotnet new my-minimal-api -n ShopApp --dry-run
```

## Template parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `-n`, `--name` | text | folder name | Project name. Replaces `MyCompany.MinimalApi` everywhere. |
| `-o`, `--output` | path | current folder | Output directory. |
| `--ApiKeyPlaceholder` | text | `CHANGE_ME` | Initial value of `ApiKey` in `appsettings.json`. |

List all available options:

```bash
dotnet new my-minimal-api --help
```

## Configuration and secrets

`appsettings.json` ships with a placeholder:

```json
{
  "ApiKey": "CHANGE_ME"
}
```

Do not commit real secrets. Set the real value with user-secrets for local development:

```bash
dotnet user-secrets init --project src/ShopApp
dotnet user-secrets set "ApiKey" "my-real-key" --project src/ShopApp
```

For production, use environment variables:

```bash
# Linux / macOS
export ApiKey="my-real-key"

# Windows PowerShell
$env:ApiKey = "my-real-key"
```

If `ApiKey` is missing or still `CHANGE_ME`, the app logs a warning at startup.

## How it works

The repository holds a real, working project plus a small config file. The template engine turns it into a new project with your chosen name.

```
  templates/minimal-api/         working project + template.json
              │
              │   dotnet pack
              ▼
  MyTemplates.0.1.0-beta.nupkg   NuGet package
              │
              │   dotnet new install
              ▼
  my-minimal-api                 template registered on your machine
              │
              │   dotnet new my-minimal-api -n ShopApp
              ▼
  ShopApp/                       your new project
```

## Generated project structure

Running `dotnet new my-minimal-api -n ShopApp` produces:

```
ShopApp/
├── ShopApp.slnx
└── src/
    └── ShopApp/
        ├── ShopApp.csproj
        ├── Program.cs
        ├── appsettings.json
        ├── appsettings.Development.json
        └── Properties/
            └── launchSettings.json
```

### What gets renamed

| In the template | In the generated project |
|---|---|
| `MyCompany.MinimalApi.slnx` | `ShopApp.slnx` |
| `src/MyCompany.MinimalApi/` | `src/ShopApp/` |
| `MyCompany.MinimalApi.csproj` | `ShopApp.csproj` |
| `Hello from MyCompany.MinimalApi` in `Program.cs` | `Hello from ShopApp` |
| Launch profile name in `launchSettings.json` | `ShopApp` |

## Repository structure

```
minimal-api-template/
├── MyTemplates.csproj                   # Packaging project (PackageType=Template)
├── README.md
├── .gitignore
└── templates/
    └── minimal-api/                     # Template root
        ├── .template.config/
        │   └── template.json            # sourceName, symbols, shortName
        ├── MyCompany.MinimalApi.slnx
        └── src/
            └── MyCompany.MinimalApi/    # Folder name is replaced by -n
                ├── MyCompany.MinimalApi.csproj
                ├── Program.cs
                ├── appsettings.json     # Placeholders: CHANGE_ME
                ├── appsettings.Development.json
                └── Properties/
                    └── launchSettings.json
```

How the pieces fit together:

- `templates/minimal-api` is a real, working project. You can build and run it directly while developing.
- `.template.config/template.json` tells the template engine to replace the text `MyCompany.MinimalApi` with the name passed through `-n`.
- `MyTemplates.csproj` exists only to package everything under `templates/` into a NuGet package.

## Development

Build and run the original project:

```bash
cd templates/minimal-api
dotnet build
dotnet run --project src/MyCompany.MinimalApi
```

Test the template locally:

```bash
dotnet new install ./templates/minimal-api
dotnet new my-minimal-api -n TestApp -o ../TestApp
cd ../TestApp
dotnet build
```

After changing the template, reinstall it:

```bash
dotnet new uninstall ./templates/minimal-api
dotnet new install ./templates/minimal-api
```

If the template engine shows stale results, reset its cache:

```bash
dotnet new --debug:reinit
```

## Packaging and publishing

Create the package:

```bash
dotnet pack -c Release -o ./artifacts
```

Test the package before publishing:

```bash
dotnet new install ./artifacts/MyTemplates.0.1.0-beta.nupkg
dotnet new my-minimal-api -n PackageTest
```

Publish to nuget.org:

```bash
dotnet nuget push ./artifacts/MyTemplates.0.1.0-beta.nupkg \
  --api-key YOUR_API_KEY \
  --source https://api.nuget.org/v3/index.json
```

A published version cannot be overwritten or re-uploaded. Increase `<Version>` in `MyTemplates.csproj` for every release.

## Update and uninstall

```bash
dotnet new update
dotnet new uninstall MyTemplates
```

## Troubleshooting

| Problem | Solution |
|---|---|
| Template not found after install | Run `dotnet new list my-minimal-api`. If empty, check that `.template.config/template.json` exists and is valid JSON. |
| Old project name still appears in generated files | Search the output for `MyCompany.MinimalApi` and make sure every file uses exactly that text. |
| `bin` or `obj` folders inside the package | Check the `Exclude` patterns in `MyTemplates.csproj` and `template.json`. |
| Changes to the template are not applied | Uninstall and reinstall the template, then run `dotnet new --debug:reinit`. |
| Warning NU5128 during pack | Already suppressed through `NoWarn` in `MyTemplates.csproj`. |

## Contributing

Issues and pull requests are welcome. Please test the template locally (install, generate, build) before opening a pull request.

## License

MIT
