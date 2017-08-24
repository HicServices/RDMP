@echo off

SET DIR=%~d0%~p0%
SET BUILD_DIR=%DIR%..\..\
CALL %BUILD_DIR%\rh_vars.bat


SET BACKUP_DB_NAME=TestDataQualityEngine

SET database.name=${test.database.dataQualityEngine.name}
SET database.security=${database.dataQualityEngine.security}
SET sql.files.directory="%PROJECT_ROOT%${dirs.db.dataQualityEngine}"
SET backup.file="${dataQualityEngine.restore.from.path}"
SET server.database=${server.database.dataQualityEngine}
SET repository.path="${repository.path}"
SET version.file="%DIR%_BuildInfo.xml"
SET version.xpath="//buildInfo/version"
SET environment="${environment}"
SET database.filepath=${test.database.dataQualityEngine.filepath}

SET connection_string="Server=%server.database%;Database=%database.name%;%database.security%"

:: DropCreate Roundhouse workflow
:: Drop
"%RH_EXE%" /c=%connection_string% /env=%environment% /drop /silent

:: then Create
"%RH_EXE%" /c=%connection_string% /f=%sql.files.directory% /vf=%version.file% /vx=%version.xpath% /r=%repository.path% /env=%environment% /simple --silent