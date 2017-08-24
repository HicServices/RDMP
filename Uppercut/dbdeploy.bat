@echo off

::Project UppercuT - http://uppercut.googlecode.com
::No edits to this file are required - http://uppercut.pbwiki.com

if '%1' == '/?' goto usage
if '%1' == '-?' goto usage
if '%1' == '?' goto usage
if '%1' == '/help' goto usage

SET DIR=%cd%
ECHO %DIR%
SET BUILD_DIR=%~d0%~p0%
ECHO %BUILD_DIR%
SET NANT="%BUILD_DIR%libs\Nant\nant.exe"
SET build.config.settings="%DIR%\settings\UppercuT.config"
SET CODE_DROP=%BUILD_DIR%code_drop\
SET DEPLOYMENT_DIR=%CODE_DROP%deployment\

%NANT% -logger:NAnt.Core.DefaultLogger -quiet /f:%BUILD_DIR%build\dbdeploy.build -D:build.config.settings=%build.config.settings% %*

if %ERRORLEVEL% NEQ 0 goto errors

:: Now run the LOCAL deploy script

ECHO Now running database update: %DEPLOYMENT_DIR%

"%DEPLOYMENT_DIR%LOCAL.DBDeployment.bat"

goto finish

:usage
echo.
echo Usage: build.bat
echo.
goto finish

:errors
EXIT /B %ERRORLEVEL%

:finish