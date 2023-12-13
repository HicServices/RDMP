@echo off
set RDMPVERSION=%1
set wix="C:\Program Files (x86)\WiX Toolset v3.11\bin"
cd /d %~dp0
%wix%\candle.exe rdmp.wxs -dVersion=%RDMPVERSION% -arch x64 -ext WixUtilExtension -nologo
if errorlevel 1 exit 1
%wix%\light.exe rdmp.wixobj -ext WixUtilExtension -nologo
if errorlevel 1 exit 1
move rdmp.msi ..\dist\
