# .NET 10 Migration Report

**Status: HISTORICAL / NON-CANONICAL MIGRATION RECORD. The current technical baseline is defined by project files, `AGENTS.md`, `docs/README.md`, `docs/architecture.md`, and `docs/sdd-architecture.md`.**

## Scope

Migrated the ERP Clean Architecture solution from .NET 8 to .NET 10.

No business logic or application features were implemented.

## Environment Verified

| Component | Detected Version |
|---|---|
| .NET SDK | 10.0.300 |
| MSBuild | 18.6.3 |
| .NET Host | 10.0.8 |
| Microsoft.NETCore.App | 10.0.8 |
| Microsoft.AspNetCore.App | 10.0.8 |
| Microsoft.WindowsDesktop.App | 10.0.8 |
| OS | Windows 10.0.26200 x64 |

## Project Framework Migration

| Project | Previous Target | New Target | Status |
|---|---|---|---|
| `ERP.Domain` | `net8.0` | `net10.0` | Migrated |
| `ERP.Application` | `net8.0` | `net10.0` | Migrated |
| `ERP.Infrastructure` | `net8.0` | `net10.0` | Migrated |
| `ERP.API` | `net8.0` | `net10.0` | Migrated |
| `ERP.WinForms` | `net8.0-windows` | `net10.0-windows` | Migrated |

The legacy project under `legacy/` was reviewed and already targets `net10.0-windows`.

## Clean Architecture References

| Project | References | Validation |
|---|---|---|
| `ERP.Domain` | None | Correct. |
| `ERP.Application` | `ERP.Domain` | Correct. |
| `ERP.Infrastructure` | `ERP.Application`, `ERP.Domain` | Correct. |
| `ERP.API` | `ERP.Application`, `ERP.Infrastructure` | Correct. |
| `ERP.WinForms` | `ERP.Application`, `ERP.Infrastructure` | Correct. |

## SDK Properties

| Project | Properties |
|---|---|
| `ERP.Domain` | `TargetFramework=net10.0`, `ImplicitUsings=enable`, `Nullable=enable`. |
| `ERP.Application` | `TargetFramework=net10.0`, `ImplicitUsings=enable`, `Nullable=enable`. |
| `ERP.Infrastructure` | `TargetFramework=net10.0`, `ImplicitUsings=enable`, `Nullable=enable`. |
| `ERP.API` | `Sdk=Microsoft.NET.Sdk.Web`, `TargetFramework=net10.0`, `ImplicitUsings=enable`, `Nullable=enable`. |
| `ERP.WinForms` | `TargetFramework=net10.0-windows`, `OutputType=WinExe`, `UseWindowsForms=true`, `ImplicitUsings=enable`, `Nullable=enable`. |

No obsolete .NET 8 framework settings remain in the ERP projects.

## Package Compatibility

No `PackageReference` entries were found in the ERP projects.

Validation command:

```text
dotnet list ERP.sln package
```

Result:

```text
No packages found for net10.0 / net10.0-windows.
```

## Validation Results

| Check | Result |
|---|---|
| `dotnet --info` | Passed. SDK/runtime versions match requested environment. |
| `dotnet restore ERP.sln` | Passed after allowing access to user NuGet configuration. |
| `dotnet sln ERP.sln list` | Passed. Solution contains the five ERP projects. |
| `dotnet list ERP.sln package` | Passed after allowing access to user NuGet configuration. No packages found. |
| `dotnet build src/ERP.Domain/ERP.Domain.csproj --no-restore` | Passed. |
| `dotnet build src/ERP.Application/ERP.Application.csproj --no-restore` | Passed. |
| `dotnet build src/ERP.Infrastructure/ERP.Infrastructure.csproj --no-restore` | Passed. |
| `dotnet build ERP.sln --no-restore` | Partially blocked by missing host entry points in `ERP.API` and `ERP.WinForms`. |

## Build Note

Historical note, now obsolete: at migration time the host projects lacked entry points. They now contain compile-ready startup scaffolding; this earlier `CS5001` status must not be treated as the current build baseline.

## Files Updated

- `src/ERP.Domain/ERP.Domain.csproj`
- `src/ERP.Application/ERP.Application.csproj`
- `src/ERP.Infrastructure/ERP.Infrastructure.csproj`
- `src/ERP.API/ERP.API.csproj`
- `src/ERP.WinForms/ERP.WinForms.csproj`
- `docs/net10-migration-report.md`
