@echo off

SET DIR=%cd%\
SET database.name="TestDataExportManager"
SET sql.files.directory="..\db\"
SET server.database="(local)"
SET rh.path= %DIR%..\..\..\Tools\RoundhousE\

SET restore.path=%DIR%..\backups\
SET restore.name=DataExportManager.bak
SET database.filepath=C:\Program Files\Microsoft SQL Server\MSSQL11.MSSQLSERVER\MSSQL\Backup\

:: Run RH in RestoreRun mode
%rh.path%\rh.exe /s=%server.database% /d=%database.name% -f %sql.files.directory% --debug --transaction /restore /rfp=%restore.path%\%restore.name% /simple /vf="_BuildInfo.xml" /vx="//buildInfo/version" /r="https://hic.dundee.ac.uk/svn/hicservices/RDMP" /restorecustomoptions="MOVE '%BACKUP_DB_NAME%' TO '%database.filepath%{{DatabaseName}}.mdf', MOVE '%BACKUP_DB_NAME%_log' TO '%database.filepath%{{DatabaseName}}_log.ldf'" --silent