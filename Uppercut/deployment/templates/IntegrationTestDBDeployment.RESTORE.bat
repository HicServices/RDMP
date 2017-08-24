@echo off

SET DIR=%~d0%~p0%
SET BUILD_DIR=%DIR%..\..\
CALL %BUILD_DIR%\rh_vars.bat

:: purely because that I backed up a database that had a different name
SET BACKUP_DB_NAME=Test_Project_Catalogue

SET database.name=${test.database.name}
SET database.security=${database.security}
SET sql.files.directory="%PROJECT_ROOT%${dirs.db}"
SET backup.file="${restore.from.path}"
SET server.database=${server.database}
SET repository.path="${repository.path}"
SET version.file="%DIR%_BuildInfo.xml"
SET version.xpath="//buildInfo/version"
SET environment="${environment}"
SET database.filepath=${test.database.filepath}

SET connection_string="Server=%server.database%;Database=%database.name%;%database.security%"

:: DropCreate Roundhouse workflow
:: Drop
"%RH_EXE%" /c=%connection_string% /env=%environment% /drop /silent

:: then Create
"%RH_EXE%" /c=%connection_string% /f=%sql.files.directory% /vf=%version.file% /vx=%version.xpath% /r=%repository.path% /env=%environment% /simple --silent