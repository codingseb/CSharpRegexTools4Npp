@echo off
setlocal

set MSBUILD="C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe"
set PROJECT=%~dp0..\CSharpRegexTools4VsCode\CSharpRegexTools4VsCode.csproj
rem Remove trailing backslash to avoid path issues
set EXT_DIR=%~dp0
set EXT_DIR=%EXT_DIR:~0,-1%

echo === Building C# host (Release x64) ===
%MSBUILD% "%PROJECT%" /p:Configuration=Release /p:Platform=x64 /restore /v:minimal
if errorlevel 1 (
    echo Build failed.
    pause
    exit /b 1
)

echo === Installing npm dependencies ===
cd /d "%EXT_DIR%"
call npm install

echo === Building TypeScript extension (minified) ===
call npm run package
if errorlevel 1 (
    echo Extension build failed.
    pause
    exit /b 1
)

echo === Creating VSIX package ===
call npm run vscode:package
if errorlevel 1 (
    echo VSIX creation failed.
    pause
    exit /b 1
)

echo === Installing extension to VS Code ===
for %%f in (*.vsix) do (
    echo Installing %%f...
    code --install-extension "%%f" --force
)

echo === Done ===
echo Extension installed. Restart VS Code to load the extension.
pause
