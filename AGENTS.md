# AGENTS.md - Coding Agent Guidelines for Skua

## Build Commands
```bash
dotnet build Skua.sln                              # Build all projects
dotnet build Skua.App.WPF/Skua.App.WPF.csproj      # Build main app only
.\Build-Skua.ps1                                   # Full release build (x64/x86 + installer)
.\Build-Skua.ps1 -Configuration Debug -SkipInstaller  # Quick debug build
```

## Project Structure
- **Skua.App.WPF**: Main WPF application (entry point)
- **Skua.Core**: Core logic, ViewModels, Scripts, Services
- **Skua.WPF**: Shared WPF views, UserControls, converters
- **Skua.Core.Interfaces**: Interfaces for dependency injection
- **Skua.Core.Models**: Data models and DTOs

## Code Style (from .editorconfig)
- **Indentation**: 4 spaces, no tabs
- **Namespaces**: File-scoped (`namespace Foo;` not `namespace Foo { }`)
- **Private fields**: Prefix with underscore `_camelCase`
- **Interfaces**: Prefix with `I` (e.g., `IScriptInterface`)
- **Types/Methods/Properties**: PascalCase
- **var usage**: Avoid - use explicit types
- **Braces**: Always required (even single-line if/else)
- **Using directives**: Outside namespace, not sorted

## Patterns
- Uses **CommunityToolkit.Mvvm** for MVVM (`[ObservableProperty]`, `[RelayCommand]`)
- DI via **Microsoft.Extensions.DependencyInjection** + `Ioc.Default`
- Register services in `Skua.Core/AppStartup/Services.cs`
- ViewModels inherit from `ObservableObject` or `ObservableRecipient`
