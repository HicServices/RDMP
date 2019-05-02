--Version: 1.4.0.0
--Description: Fix naming of columns in TableInfo to Server and Database instead of DataAccess and Database_name and the same to ExternalDatabaseServer
--if a column exists called Database_access, rename database_access and database_name
  if exists (select 1 from sys.columns where name = 'Database_access' and object_id = (select object_id from sys.tables where name = 'TableInfo'))
  begin
  exec sp_rename 'TableInfo.Database_access','Server','COLUMN'
exec sp_rename 'TableInfo.Database_name','Database','COLUMN'

exec sp_rename 'ExternalDatabaseServer.ServerName','Server','COLUMN'
exec sp_rename 'ExternalDatabaseServer.DatabaseName','Database','COLUMN'

  end
