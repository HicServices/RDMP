@echo off

SET DIR=%cd%\
SET BACKUP_DB_NAME=Test_Project_Catalogue
SET database.name="LocalCatalogue_IntegrationTests"
SET sql.files.directory="..\db"
SET server.database="(local)"
SET rh.path= %DIR%..\..\..\Tools\RoundhousE\

SET restore.path=..\backups\
SET restore.name=Test_Catalogue.bak
SET database.filepath=C:\Program Files\Microsoft SQL Server\MSSQL11.MSSQLSERVER\MSSQL\Backup\

:: Run RH in RestoreRun mode
%rh.path%rh.exe /s=%server.database% /d=%database.name% -f %sql.files.directory% --debug --transaction /restore /rfp="%restore.path%%restore.name%" /simple /vf="_BuildInfo.xml" /vx="//buildInfo/version" /r="https://hic.dundee.ac.uk/svn/hicservices/RDMP" /restorecustomoptions="MOVE '%BACKUP_DB_NAME%' TO '%database.filepath%{{DatabaseName}}.mdf', MOVE '%BACKUP_DB_NAME%_log' TO '%database.filepath%{{DatabaseName}}_log.ldf'" --silent --schema=dbo