--Version:2.11.0.1
--Description: Data export support for FAnsiSql library

UPDATE ExternalCohortTable set DatabaseType = 'MySql' where DatabaseType = 'MYSQLServer'