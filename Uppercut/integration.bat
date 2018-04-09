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

if exist "%ProgramFiles(x86)%\Microsoft Visual Studio 11.0\VC\vcvarsall.bat" goto initialize2k8on64Dev11
if exist "%ProgramFiles%\Microsoft Visual Studio 11.0\VC\vcvarsall.bat" goto initialize2k8Dev11
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

:build
::Project UppercuT - http://uppercut.googlecode.com
::No edits to this file are required - http://uppercut.pbwiki.com

SET ENV=LOCAL
IF NOT '%1' == '' SET ENV=%1

SET DIR=%cd%
SET BUILD_DIR=%~d0%~p0%
SET PROJECT_DIR=%BUILD_DIR%..\
SET LIBS_DIR=%PROJECT_DIR%libs\
SET NANT="%LIBS_DIR%Nant\nant.exe"
SET build.config.settings="%DIR%\settings\UppercuT.config"

SET CODE_DROP=%BUILD_DIR%code_drop\
SET DEPLOYMENT_DIR=%CODE_DROP%deployment\

:: Restore missing packages
nuget restore %PROJECT_DIR%HIC.DataManagementPlatform.sln
if %ERRORLEVEL% NEQ 0 goto errors

:: But this will nuke the build_output directory after completion...
%NANT% -logger:NAnt.Core.DefaultLogger -quiet /f:%BUILD_DIR%build\default.build -D:build.config.settings=%build.config.settings%
if %ERRORLEVEL% NEQ 0 goto errors

:: Next, copy the ENV app config settings to the build output dir for all the *Tests*.dlls, stripping %ENV%. from the start of each file
:: (FILENAME:%ENV%.=) substitutes %ENV% with an empty string in FILENAME
setlocal enableDelayedExpansion
for /f %%a in ('dir %CODE_DROP%environment.files\%ENV%\*.config /b /a-d ') do (
	set FILENAME=%%a
	echo Copying %CODE_DROP%environment.files\%ENV%\%%a to %CODE_DROP%Tests\!FILENAME:%ENV%.=!
	xcopy %CODE_DROP%environment.files\%ENV%\%%a %CODE_DROP%Tests\!FILENAME:%ENV%.=! /Y
)

:: Also need to copy CommitAssembly exe config
xcopy %CODE_DROP%environment.files\%ENV%\%ENV%.CommitAssembly.exe.config %CODE_DROP%Tests\CommitAssembly.exe.config /Y

if %ERRORLEVEL% NEQ 0 goto errors

:: Bring the ENV's integration database up to the most recent version
ECHO Setting up the DB for integration tests
CALL ..\Tools\DatabaseCreation\Release\DatabaseCreation.exe %DBSERVER% %DBPREFIX% -D
REM ECHO Catalogue DB...
REM ECHO "%DEPLOYMENT_DIR%%ENV%.IntegrationTestDBDeployment.RESTORE.bat"
REM CALL "%DEPLOYMENT_DIR%%ENV%.IntegrationTestDBDeployment.RESTORE.bat"
REM if %ERRORLEVEL% NEQ 0 goto errors

REM ECHO Logging DB...
REM CALL "%DEPLOYMENT_DIR%%ENV%.IntegrationTestDBDeployment.Logging.RESTORE.bat"
REM if %ERRORLEVEL% NEQ 0 goto errors

REM ECHO DataExportManager DB...
REM CALL "%DEPLOYMENT_DIR%%ENV%.IntegrationTestDBDeployment.DataExportManager.RESTORE.bat"
REM if %ERRORLEVEL% NEQ 0 goto errors

REM ECHO DataQualityEngine DB...
REM CALL "%DEPLOYMENT_DIR%%ENV%.IntegrationTestDBDeployment.DataQualityEngine.RESTORE.bat"
REM if %ERRORLEVEL% NEQ 0 goto errors

REM ECHO ANO DB...
REM CALL "%DEPLOYMENT_DIR%%ENV%.IntegrationTestDBDeployment.ANO.RESTORE.bat"
REM if %ERRORLEVEL% NEQ 0 goto errors

:: integration.build simply runs build\analyzers\nunit.integration.test.step
ECHO -------------------------------------------------------------
ECHO Integration environment set up, now running integration tests
ECHO -------------------------------------------------------------
%NANT% -logger:NAnt.Core.DefaultLogger -quiet /f:%BUILD_DIR%build\integration.build -D:build.config.settings=%build.config.settings%

:: We always return zero here, even if nunit has found errors (and returned non-zero) otherwise the Jenkins post-build task to produce the nunit report won't fire
::if %ERRORLEVEL% NEQ 0 goto errors

goto finish

:usage
echo.
echo Usage: build.bat
echo.
goto finish

:errors
EXIT /B %ERRORLEVEL%

:finish