--Version:2.12.0.1
--Description: Database constraints Name properties are never null
update Catalogue set Name = 'NoName' where Name is null
GO

if exists(select 1 from sys.columns where name = 'Name' and OBJECT_NAME(object_id) = 'Catalogue' and is_nullable = 1)
	alter table Catalogue alter column Name varchar(1000) not null

update CatalogueItemIssue set Name = 'NoName' where Name is null
GO

if exists(select 1 from sys.columns where name = 'Name' and OBJECT_NAME(object_id) = 'CatalogueItemIssue' and is_nullable = 1)
	alter table CatalogueItemIssue		  alter column Name varchar(1000) not null

update ColumnInfo set Name = 'NoName' where Name is null
GO

if exists(select 1 from sys.columns where name = 'Name' and OBJECT_NAME(object_id) = 'ColumnInfo' and is_nullable = 1)
	alter table ColumnInfo				  alter column Name varchar(1000) not null

update ExternalDatabaseServer set Name = 'NoName' where Name is null
GO

if exists(select 1 from sys.columns where name = 'Name' and OBJECT_NAME(object_id) = 'ExternalDatabaseServer' and is_nullable = 1)
	alter table ExternalDatabaseServer	  alter column Name varchar(1000) not null

update TableInfo set Name = 'NoName' where Name is null
GO

if exists(select 1 from sys.columns where name = 'Name' and OBJECT_NAME(object_id) = 'TableInfo' and is_nullable = 1)
	alter table TableInfo alter column Name varchar(1000) not null

update PermissionWindow set Name = 'NoName' where Name is null
GO

if exists(select 1 from sys.columns where name = 'Name' and OBJECT_NAME(object_id) = 'PermissionWindow' and is_nullable = 1)
	alter table PermissionWindow alter column Name varchar(1000) not null