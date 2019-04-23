--Version:2.12.0.1
--Description: Database constraints Name properties are never null
update ExternalCohortTable set Name = 'NoName' where Name is null
GO

if exists(select 1 from sys.columns where name = 'Name' and OBJECT_NAME(object_id) = 'ExternalCohortTable' and is_nullable = 1)
	alter table ExternalCohortTable alter column Name varchar(1000) not null

update Project set Name = 'NoName' where Name is null
GO

if exists(select 1 from sys.columns where name = 'Name' and OBJECT_NAME(object_id) = 'Project' and is_nullable = 1)
	alter table Project alter column Name varchar(1000) not null