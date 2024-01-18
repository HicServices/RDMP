--Version:8.1.3
--Description: Adds LastLoadTime to LoadMetadata table
 GO
  if exists (select 1 from sys.columns where name = 'LoadLastTime' and object_id = (select object_id from sys.tables where name = 'LoadMetadata'))
BEGIN       
ALTER TABLE LoadMetadata ADD LastLoadTime DATETIME NULL;
END
