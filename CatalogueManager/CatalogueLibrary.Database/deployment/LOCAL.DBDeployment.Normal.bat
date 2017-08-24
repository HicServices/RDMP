@echo off

SET DIR=%cd%\
SET database.name="LocalCatalogue_IntegrationTests"
SET sql.files.directory="..\db"
SET server.database="(local)"
SET rh.path= %DIR%..\..\..\Tools\RoundhousE\

:: Run RH in RestoreRun mode
%rh.path%\rh.exe /s=%server.database% /d=%database.name% -f %sql.files.directory% --debug --transaction --environment=LOCAL /simple /vf="_BuildInfo.xml" /vx="//buildInfo/version" /r="https://hic.dundee.ac.uk/svn/hicservices/RDMP" --silent --schema=dbo