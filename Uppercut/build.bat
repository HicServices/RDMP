@echo off

::Project UppercuT - http://uppercut.googlecode.com

if '%1' == '/?' goto usage
if '%1' == '-?' goto usage
if '%1' == '?' goto usage
if '%1' == '/help' goto usage

if "%WindowsSdkDir%" neq "" goto build
if exist "%ProgramFiles(x86)%\Microsoft Visual Studio 12.0\VC\vcvarsall.bat" goto initialize2k8on64Dev12
if exist "%ProgramFiles%\Microsoft Visual Studio 12.0\VC\vcvarsall.bat" goto initialize2k8Dev12

if exist "%ProgramFiles(x86)%\Microsoft Visual Studio 10.0\VC\vcvarsall.bat" goto initialize2k8on64
if exist "%ProgramFiles%\Microsoft Visual Studio 10.0\VC\vcvarsall.bat" goto initialize2k8

if exist "%ProgramFiles(x86)%\Microsoft Visual Studio 14.0\VC\vcvarsall.bat" goto initializeDev15
if exist "%ProgramFiles%\Microsoft Visual Studio 14.0\VC\vcvarsall.bat" goto initializeDev15on64
echo "Unable to detect suitable environment. Build may not succeed."
goto build

:initialize2k8Dev12
call "%ProgramFiles%\Microsoft Visual Studio 12.0\VC\vcvarsall.bat" x86
goto build

:initialize2k8on64Dev12
call "%ProgramFiles(x86)%\Microsoft Visual Studio 12.0\VC\vcvarsall.bat" x86
goto build

:initialize2k8
call "%ProgramFiles%\Microsoft Visual Studio 10.0\VC\vcvarsall.bat" x86
goto build

:initialize2k8on64
call "%ProgramFiles(x86)%\Microsoft Visual Studio 10.0\VC\vcvarsall.bat" x86
goto build

:initializeDev15on64
call "%ProgramFiles%\Microsoft Visual Studio 14.0\VC\vcvarsall.bat" x86
goto build

:initializeDev15
call "%ProgramFiles(x86)%\Microsoft Visual Studio 14.0\VC\vcvarsall.bat" x86
goto build

:build
::Project UppercuT - http://uppercut.googlecode.com

SET ENV=LOCAL
IF NOT '%1' == '' SET ENV=%1
for /f "tokens=1,* delims= " %%a in ("%*") do set EXTRA_FLAGS=%%b
SET DIR=%cd%
SET BUILD_DIR=%~d0%~p0%
SET PROJECT_DIR=%BUILD_DIR%..\
SET NANT="%PROJECT_DIR%libs\Nant\nant.exe"
SET build.config.settings="%BUILD_DIR%settings\UppercuT.config"

ECHO Reading settings from %build.config.settings%

:: Restore missing packages
ECHO nuget sources
nuget sources
ECHO nuget restore %PROJECT_DIR%HIC.DataManagementPlatform.sln
nuget restore %PROJECT_DIR%HIC.DataManagementPlatform.sln
if %ERRORLEVEL% NEQ 0 goto errors

ECHO Now running build (%BUILD_DIR%build\default.build)
%NANT% -logger:NAnt.Core.DefaultLogger -quiet /f:%BUILD_DIR%build\default.build -D:build.config.settings=%build.config.settings% %EXTRA_FLAGS%

:: We always return zero here, even if nunit has found errors (and returned non-zero) otherwise the Jenkins post-build task to produce the nunit report won't fire
:: if %ERRORLEVEL% NEQ 0 goto errors

goto finish

:usage
echo.
echo Usage: build.bat
echo.
goto finish

:errors
EXIT /B %ERRORLEVEL%

:finish