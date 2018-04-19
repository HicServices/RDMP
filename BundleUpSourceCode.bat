@echo off

if exist "%ProgramFiles(x86)%\Microsoft Visual Studio 14.0\VC\vcvarsall.bat" goto initializeDev14
if exist "%ProgramFiles%\Microsoft Visual Studio 14.0\VC\vcvarsall.bat" goto initializeDev14on64

:initializeDev14on64
call "%ProgramFiles%\Microsoft Visual Studio 14.0\VC\vcvarsall.bat" x86
goto buildDocumentationCache

:initializeDev14
call "%ProgramFiles(x86)%\Microsoft Visual Studio 14.0\VC\vcvarsall.bat" x86
goto buildDocumentationCache

:buildDocumentationCache
msbuild HIC.DataManagementPlatform.sln /t:Tools\BundleUpSourceIntoZip:Rebuild /p:Configuration="Debug" /p:Platform="Any CPU"
