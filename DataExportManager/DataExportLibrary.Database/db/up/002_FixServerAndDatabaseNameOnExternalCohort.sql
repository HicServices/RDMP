--Version:1.2.0.0
--Description: Fixes ExternalCohortTable.ServerName and ExternalCohortTable.DatabaseName naming to match Database and Server so that it can implement IDataAccessPoint
if exists (select 1 from sys.columns where name = 'ServerName' and object_id = OBJECT_ID('ExternalCohortTable'))
begin
exec sp_rename 'ExternalCohortTable.ServerName','Server','COLUMN'
exec sp_rename 'ExternalCohortTable.DatabaseName','Database','COLUMN'
END
