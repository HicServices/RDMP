@echo off

if '%1' == '/?' goto usage
if '%1' == '-?' goto usage
if '%1' == '?' goto usage
if '%1' == '/help' goto usage

if "%WindowsSdkDir%" neq "" goto build
if exist "%ProgramFiles(x86)%\Microsoft Visual Studio 12.0\VC\vcvarsall.bat" goto initialize2k8on64Dev12
if exist "%ProgramFiles%\Microsoft Visual Studio 12.0\VC\vcvarsall.bat" goto initialize2k8Dev12

if exist "%ProgramFiles(x86)%\Microsoft Visual Studio 10.0\VC\vcvarsall.bat" goto initialize2k8on64
if exist "%ProgramFiles%\Microsoft Visual Studio 10.0\VC\vcvarsall.bat" goto initialize2k8

if exist "%ProgramFiles(x86)%\Microsoft Visual Studio 11.0\VC\vcvarsall.bat" goto initialize2k8on64Dev11
if exist "%ProgramFiles%\Microsoft Visual Studio 11.0\VC\vcvarsall.bat" goto initialize2k8Dev11

if exist "%ProgramFiles(x86)%\Microsoft Visual Studio 14.0\VC\vcvarsall.bat" goto initializeDev14
if exist "%ProgramFiles%\Microsoft Visual Studio 14.0\VC\vcvarsall.bat" goto initializeDev14on64

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

:initialize2k8Dev11
call "%ProgramFiles%\Microsoft Visual Studio 11.0\VC\vcvarsall.bat" x86
goto build

:initialize2k8on64Dev11
call "%ProgramFiles(x86)%\Microsoft Visual Studio 11.0\VC\vcvarsall.bat" x86
goto build

:initializeDev14on64
call "%ProgramFiles%\Microsoft Visual Studio 14.0\VC\vcvarsall.bat" x86
goto publish

:initializeDev14
call "%ProgramFiles(x86)%\Microsoft Visual Studio 14.0\VC\vcvarsall.bat" x86
goto publish


:build

SET PLUGIN_PROJECT_ROOT=..\

SET NUGET_SOURCE=%1
SET NUGET_PUSH_PARAMS=%2

:: Restore missing packages
echo Restoring NuGet packages (need to do this at solution level)
echo nuget restore ..\..\HIC.DataManagementPlatform.sln
nuget restore ..\..\HIC.DataManagementPlatform.sln
if %ERRORLEVEL% NEQ 0 goto errors

:: The supplied configuration flags command the build process to push the nuget package to the ctm nuget server
echo Building the Plugin project in Release mode, if successful will pack the Plugin NuGet package

echo Now build the plugin project
msbuild Plugin.build /t:Deploy /p:ReleaseNugetPackageSource=%NUGET_SOURCE% /p:ReleaseNugetPushParams=%NUGET_PUSH_PARAMS% /p:ConfigurationName=Release

echo Now building the assembly to create plugin tests
cd ..\Plugin.Test
msbuild PluginTest.build /t:Deploy /p:ReleaseNugetPackageSource=%NUGET_SOURCE% /p:ReleaseNugetPushParams=%NUGET_PUSH_PARAMS% /p:ConfigurationName=Release

goto finish

:usage
echo.
echo Usage: build-and-deploy.bat NUGET_SOURCE NUGET_PARAMS
echo.
goto finish

:errors
EXIT /B %ERRORLEVEL%

:finish
cd ..\Plugin