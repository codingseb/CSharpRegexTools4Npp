@echo off
setlocal

set MSBUILD="C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe"
set PROJECT=%~dp0..\CSharpRegexTools4VsCode\CSharpRegexTools4VsCode.csproj
rem Remove trailing backslash to avoid \" escaping issues
set EXT_DIR=%~dp0
set EXT_DIR=%EXT_DIR:~0,-1%

echo === Building C# host (Debug x64) ===
%MSBUILD% "%PROJECT%" /p:Configuration=Debug /p:Platform=x64 /restore /v:minimal
if errorlevel 1 (
    echo Build failed.
    pause
    exit /b 1
)

echo === Installing npm dependencies ===
cd /d "%EXT_DIR%"
call npm install

echo === Building TypeScript extension ===
call npm run build
if errorlevel 1 (
    echo Extension build failed.
    pause
    exit /b 1
)

echo === Launching VS Code Extension Development Host ===
code --extensionDevelopmentPath="%EXT_DIR%"
