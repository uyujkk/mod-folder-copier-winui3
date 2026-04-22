@echo off
setlocal

set "ROOT=%~dp0"
if "%ROOT:~-1%"=="\" set "ROOT=%ROOT:~0,-1%"

set "DOTNET=%ProgramFiles%\dotnet\x64\dotnet.exe"
if not exist "%DOTNET%" set "DOTNET=%ProgramFiles%\dotnet\dotnet.exe"

set "CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC%" set "CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe"

set "PROJECT=%ROOT%\WinUI3\ModFolderCopier.WinUI.csproj"
set "SOURCE=%ROOT%\WinUI3\bin\x64\Release\net8.0-windows10.0.19041.0\win-x64"
set "OUTPUT=%ROOT%\dist\WinUI3"
set "LAUNCHER_SOURCE=%ROOT%\WinUILauncher.cs"
set "LAUNCHER_OUTPUT=%ROOT%\dist\ModFolderCopier.exe"

if not exist "%DOTNET%" (
  echo dotnet SDK not found.
  exit /b 1
)

"%DOTNET%" build "%PROJECT%" -c Release -p:Platform=x64 || exit /b 1

if not exist "%ROOT%\dist" mkdir "%ROOT%\dist"
if not exist "%OUTPUT%" mkdir "%OUTPUT%"

robocopy "%SOURCE%" "%OUTPUT%" /E /NFL /NDL /NJH /NJS /NC /NS /NP >nul
if errorlevel 8 exit /b %errorlevel%

if exist "%ROOT%\dist\config.ini" copy /Y "%ROOT%\dist\config.ini" "%OUTPUT%\config.ini" >nul

"%CSC%" /nologo /target:winexe /out:"%LAUNCHER_OUTPUT%" /reference:System.dll /reference:System.Windows.Forms.dll "%LAUNCHER_SOURCE%" || exit /b 1

if exist "%OUTPUT%\startup.log" del /f /q "%OUTPUT%\startup.log"
if exist "%OUTPUT%\ModFolderCopier.WinUI.pdb" del /f /q "%OUTPUT%\ModFolderCopier.WinUI.pdb"

echo WinUI 3 build completed.
echo Launcher: %LAUNCHER_OUTPUT%
echo Runtime:  %OUTPUT%
endlocal
