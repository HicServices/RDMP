@echo off

SET DIR=%~d0%~p0%
SET BUILD_DIR=%DIR%..\..\
CALL %BUILD_DIR%\rh_vars.bat

:: purely because that I backed up a database that had a different name
SET BACKUP_DB_NAME=Test_Project_Catalogue

SET database.name="${database.name}"
SET sql.files.directory="%PROJECT_ROOT%${dirs.db}"
SET backup.file="%PROJECT_ROOT%${restore.from.path}"
SET server.database="${server.database}"
SET repository.path="${repository.path}"
SET version.file="%DIR%_BuildInfo.xml"
SET version.xpath="//buildInfo/version"
SET environment="${environment}"
SET database.filepath=${test.database.filepath}

"%RH_EXE%" /d="%database.name%" /f=%sql.files.directory% /s=%server.database% /vf=%version.file% /vx=%version.xpath% /r=%repository.path% /env=%environment% /simple /restore /rfp=%backup.file% /rt=1200 /restorecustomoptions="MOVE '%BACKUP_DB_NAME%' TO '%database.filepath%{{DatabaseName}}.mdf', MOVE '%BACKUP_DB_NAME%_log' TO '%database.filepath%{{DatabaseName}}_log.ldf'" --silent --schema=dbo