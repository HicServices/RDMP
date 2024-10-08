--Version:2.8.0.1
--Description: Creates the RemoteRDMP Table and Adds a Name to the AutomationServiceSlot

if not exists (select 1 from sys.columns where name = 'Name' and  object_name(object_id) = 'CacheProgress')
begin
    alter table CacheProgress add Name varchar(1000) null
end
GO

UPDATE CacheProgress SET Name = 'Cache Progress ' + CAST(ID AS VARCHAR(10)) WHERE Name IS NULL
GO


if exists (select 1 from sys.columns where name = 'Name' and OBJECT_NAME(object_id) = 'CacheProgress' and is_nullable = 1)
  begin
    alter table CacheProgress alter column Name varchar(1000) not null
  end
