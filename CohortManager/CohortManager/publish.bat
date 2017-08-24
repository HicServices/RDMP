@echo off

if "%1"=="" goto usage
if "%2"=="" goto usage

if exist "%ProgramFiles(x86)%\Microsoft Visual Studio 14.0\VC\vcvarsall.bat" goto initializeDev14
if exist "%ProgramFiles%\Microsoft Visual Studio 14.0\VC\vcvarsall.bat" goto initializeDev14on64

:initializeDev14on64
call "%ProgramFiles%\Microsoft Visual Studio 14.0\VC\vcvarsall.bat" x86
goto publish

:initializeDev14
call "%ProgramFiles(x86)%\Microsoft Visual Studio 14.0\VC\vcvarsall.bat" x86
goto publish

:publish
msbuild /t:publish /p:ApplicationVersion=%1 /p:PublishDir=%2 /p:MinimumRequiredVersion=%1
goto end

:usage
echo.
echo publish.bat [version] [directory]
echo.
echo(	[version] 	The version you wish to publish Cohort Manager as.
echo(			It will also be used for the MinimumRequiredVersion property
echo(			(so as to force clients to update)
echo(	[directory] 	The directory to which the ClickOnce package will be published.
echo.

:end