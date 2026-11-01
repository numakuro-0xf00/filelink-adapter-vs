# Repository Guidelines

## Project Structure & Module Organization

- `CodeLinkAdapterExt.slnx` contains the VSIX and parser test projects.
- `CodeLinkAdapterExt/CodeLinkAdapterExt.csproj` targets .NET Framework 4.8 and references the Visual Studio SDK and VSSDK build tools through NuGet.
- `CodeLinkAdapterExt/CodeLinkAdapterExtPackage.cs` implements the `AsyncPackage` entry point.
- `OpenCodexLinkCommand.cs` handles clipboard navigation; `CodeLinkParser.cs` parses links without Visual Studio dependencies. `Commands.vsct` defines the Tools menu command.
- `CodeLinkAdapterExt/source.extension.vsixmanifest` defines extension identity, installation targets, and prerequisites. No separate asset directory exists yet.
- `bin/`, `obj/`, and `.vs/` contain generated output or local IDE state; keep them out of commits.

## Build, Test, and Development Commands

Use Windows with Visual Studio extension development tools and the .NET Framework 4.8 targeting pack. Run these commands from this directory in a Visual Studio Developer PowerShell:

```powershell
msbuild CodeLinkAdapterExt\CodeLinkAdapterExt.csproj /restore /p:Configuration=Debug
msbuild CodeLinkAdapterExt\CodeLinkAdapterExt.csproj /restore /p:Configuration=Release
dotnet test CodeLinkAdapterExt.Tests\CodeLinkAdapterExt.Tests.csproj
```

MSBuild restores dependencies and builds the selected configuration. Tests require the .NET SDK and .NET Framework 4.8 on Windows. Open `CodeLinkAdapterExt.slnx` for IDE development. Launch `devenv /RootSuffix Exp` for manual extension checks in an experimental Visual Studio instance.

## Coding Style & Naming Conventions

Follow the existing C# style: four-space indentation, braces on separate lines, block-scoped namespaces, and explicit accessibility. Use PascalCase for types, methods, and constants; camelCase for parameters and locals; and the `Async` suffix for asynchronous methods. Match filenames to their primary types. XML uses two-space indentation. No repository formatter or linter is configured.

Preserve cancellation support in asynchronous initialization and switch to the UI thread before accessing services that require it.

## Testing Guidelines

`CodeLinkAdapterExt.Tests/` uses MSTest and links parser source files to avoid loading Visual Studio. Name files `<ClassName>Tests.cs` and methods after expected behavior. Cover copied links, position boundaries, path formats, and malformed input. No coverage threshold is configured. Validate VSIX changes with Debug and Release builds, then check installation, clipboard navigation, and error messages in the experimental instance.

## Commit & Pull Request Guidelines

The default branch is `main`; create `codex/<topic>` branches for subsequent changes. Use concise Japanese commit subjects with labels such as `feat:`, `fix:`, or `chore:` and keep commits focused. Pull requests should explain the change, link relevant issues, report build and manual validation, and include screenshots for visible UI changes. Call out changes to extension identity, dependencies, or supported Visual Studio versions.
