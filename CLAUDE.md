# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

CSharpRegexTools4Npp is a Notepad++ plugin written in C# that provides regex tools using the .NET regex engine. Features include regex syntax highlighting (via AvalonEdit), match listing with groups/captures, select/extract/replace operations, C# scripting for replacements (via Roslyn/CS-Script), and Excel file support (via ClosedXML).

## Build

Open `CSharpRegexTools4Npp.slnx` in Visual Studio 2022+. Select platform (x86 or x64) matching your Notepad++ installation. Build with F5 — post-build targets automatically copy the DLL to the Notepad++ plugins directory and launch Notepad++.

```
msbuild CSharpRegexTools4Npp.slnx /p:Configuration=Debug /p:Platform=x64
```

Target framework: .NET Framework 4.8.1, C# 9. `AllowUnsafeBlocks` is enabled in the plugin project.

There are no automated tests in this project.

## Architecture

Two assemblies in the solution:

- **CSharpRegexTools4Npp** — Plugin entry point and Notepad++ integration layer. `Main.cs` registers menu commands and toolbar buttons. Uses the Plugin Infrastructure (`PluginInfrastructure/`) for IPC with Notepad++ via unmanaged DLL exports and gateway classes (`NotepadPPGateway`, `ScintillaGateway`). `Npp.cs` wraps common Notepad++ operations.

- **RegexDialog** — Core UI and logic. `RegExToolDialog.xaml/.xaml.cs` is the main WPF dialog (large file, ~116KB code-behind). Contains models (`Model/`), services (`Sevices/` — note the typo), and utilities (`Utils/`). `Config.cs` manages settings via INI files. `RoslynService.cs` handles C# code compilation for script-based replacements.

## Key Patterns

- **Fody IL weaving**: `PropertyChanged.Fody` auto-implements `INotifyPropertyChanged` on model/viewmodel classes. `Costura.Fody` merges all dependencies into a single output DLL — this is required for the plugin to load in Notepad++.
- **Dual UI frameworks**: The plugin uses both WPF (main dialog) and Windows Forms (legacy dialogs, Notepad++ interop).
- **Platform-aware builds**: x86 and x64 configurations have separate output directories (`bin\Debug` vs `bin\Debug-x64`) and separate Notepad++ install paths for post-build copy.
- **Keyboard shortcut**: Ctrl+Shift+H invokes the plugin.
