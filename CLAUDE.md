# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

CSharpRegexTools is a .NET regex tools suite that runs in both Notepad++ and VS Code. Features include regex syntax highlighting (via AvalonEdit), match listing with groups/captures, select/extract/replace operations, C# scripting for replacements (via Roslyn/CS-Script), and Excel file support (via ClosedXML).

## Build

### Notepad++ plugin

Open `CSharpRegexTools4Npp.slnx` in Visual Studio 2022+. Select platform (x86 or x64) matching your Notepad++ installation. Build with F5 — post-build targets automatically copy the DLL to the Notepad++ plugins directory and launch Notepad++.

```
msbuild CSharpRegexTools4Npp.slnx /p:Configuration=Debug /p:Platform=x64
```

### VS Code extension

Use the provided batch scripts in `vscode-csharp-regex-tools/`:

```
dev.bat       # Build Debug + launch VS Code Extension Development Host
release.bat   # Build Release + install to ~/.vscode/extensions/
```

Or build manually:

```
msbuild CSharpRegexTools4VsCode/CSharpRegexTools4VsCode.csproj /p:Configuration=Debug /p:Platform=x64 /restore
cd vscode-csharp-regex-tools && npm install && npm run build
code --extensionDevelopmentPath=C:\Projets\CSharpRegexTools4Npp\vscode-csharp-regex-tools
```

Target framework: .NET Framework 4.8.1. `AllowUnsafeBlocks` is enabled in the plugin project. `Directory.Build.props` sets `LangVersion=latest` for all SDK-style projects.

There are no automated tests in this project.

## Architecture

Three assemblies in the solution:

- **CSharpRegexTools4Npp** — Plugin entry point and Notepad++ integration layer. `Main.cs` registers menu commands and toolbar buttons. Uses the Plugin Infrastructure (`PluginInfrastructure/`) for IPC with Notepad++ via unmanaged DLL exports and gateway classes (`NotepadPPGateway`, `ScintillaGateway`). `Npp.cs` wraps common Notepad++ operations.

- **RegexDialog** — Core UI and logic, shared between both hosts. `RegExToolDialog.xaml/.xaml.cs` is the main WPF dialog (large file, ~116KB code-behind). The dialog is decoupled from any host via 13 delegates (`GetText`, `SetText`, `SetPosition`, etc.) injected at creation time. Contains models (`Model/`), services (`Sevices/` — note the typo), and utilities (`Utils/`). `Config.cs` manages settings as JSON in `%APPDATA%`. `RoslynService.cs` handles C# code compilation for script-based replacements. `PathUtils.AppDataFolderName` controls the AppData subfolder name, allowing each host to use separate config.

- **CSharpRegexTools4VsCode** — Console+WPF exe that hosts the RegExToolDialog and communicates with VS Code via JSON-RPC (stdin/stdout). `Program.cs` creates a WPF Application and starts JSON-RPC on a background thread. `JsonRpcHandler.cs` wires the dialog's 13 delegates to JSON-RPC requests toward VS Code. Uses `JoinableTaskFactory` for sync-over-async calls on the WPF UI thread.

### VS Code extension (`vscode-csharp-regex-tools/`)

TypeScript extension that spawns the C# host exe and bridges JSON-RPC to VS Code editor APIs. `src/extension.ts` registers the `csharpRegexTools.open` command and handles all `editor/*` requests from the C# host.

### JSON-RPC protocol

Communication via stdin/stdout with `StreamJsonRpc` (.NET) and `vscode-jsonrpc` (Node.js):

- **VS Code -> C# Host** (notifications): `window/show`, `window/hide`, `shutdown`
- **C# Host -> VS Code** (requests): `editor/getText`, `editor/setText`, `editor/setTextInNew`, `editor/getSelectedText`, `editor/setSelectedText`, `editor/setPosition`, `editor/setSelection`, `editor/getSelectionStartIndex`, `editor/getSelectionLength`, `editor/saveCurrentDocument`, `editor/setCSharpHighlighting`, `editor/tryOpen`, `editor/getCurrentFileName`

## Key Patterns

- **Fody IL weaving**: `PropertyChanged.Fody` auto-implements `INotifyPropertyChanged` on model/viewmodel classes. `Costura.Fody` merges all dependencies into a single output DLL/EXE — required for the Npp plugin and for the VS Code host exe.
- **Dual UI frameworks**: The plugin uses both WPF (main dialog) and Windows Forms (legacy dialogs, Notepad++ interop).
- **Platform-aware builds**: x86 and x64 configurations have separate output directories (`bin\Debug` vs `bin\Debug-x64`) and separate Notepad++ install paths for post-build copy.
- **Per-host config isolation**: `PathUtils.AppDataFolderName` defaults to `CSharpRegexTools4Npp`. The VS Code host sets it to `CSharpRegexTools4VsCode` so each IDE has separate settings. Dialog title is prefixed with `[VS Code]` or left as-is for Npp.
- **Keyboard shortcuts**: Ctrl+Shift+H in Notepad++, Alt+Shift+H in VS Code.
