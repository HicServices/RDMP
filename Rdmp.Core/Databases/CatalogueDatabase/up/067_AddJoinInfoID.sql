--Version:2.13.0.1
--Description: Adds a column called ID to JoinInfo allowing it to be created/deleted more easily.

if( exists (select * from sys.key_constraints where type ='PK' AND OBJECT_NAME(parent_object_id) = 'JoinInfo'))
	ALTER TABLE JoinInfo DROP CONSTRAINT PK_JoinInfo;  

  if(not exists (select * from sys.all_columns where name ='ID' AND OBJECT_NAME(object_id) = 'JoinInfo'))
  	ALTER TABLE JoinInfo ADD ID INT IDENTITY(1,1)

  if(not exists (select * from sys.key_constraints where type ='PK' AND OBJECT_NAME(parent_object_id) = 'JoinInfo'))
	ALTER TABLE JoinInfo ADD CONSTRAINT PK_JoinInfo PRIMARY KEY (ID);

  if not exists (select 1 from sys.indexes where name = 'ix_JoinColumnsMustBeUnique')
	CREATE UNIQUE NONCLUSTERED INDEX [ix_JoinColumnsMustBeUnique] ON [JoinInfo]
	(
		[ForeignKey_ID] ASC,
		[PrimaryKey_ID] ASC
	)
		

--No more Software Version / Scalar Function

if exists (select 1 from sys.default_constraints where name ='DF_AnyTableSqlParameter_SoftwareVersion')
begin
ALTER TABLE AnyTableSqlParameter DROP CONSTRAINT DF_AnyTableSqlParameter_SoftwareVersion
ALTER TABLE AnyTableSqlParameter			  drop column SoftwareVersion 
end

if exists (select 1 from sys.default_constraints where name ='DF_ANOTable_SoftwareVersion')
begin
ALTER TABLE ANOTable DROP CONSTRAINT DF_ANOTable_SoftwareVersion
ALTER TABLE ANOTable drop column SoftwareVersion 
end


if exists (select 1 from sys.default_constraints where name ='DF_AggregateConfiguration_SoftwareVersion')
begin
ALTER TABLE AggregateConfiguration			  DROP CONSTRAINT DF_AggregateConfiguration_SoftwareVersion
ALTER TABLE AggregateConfiguration			  drop column SoftwareVersion 
end

if exists (select 1 from sys.default_constraints where name ='DF_AggregateDimension_SoftwareVersion')
begin
ALTER TABLE AggregateDimension				  DROP CONSTRAINT DF_AggregateDimension_SoftwareVersion
ALTER TABLE AggregateDimension				  drop column SoftwareVersion 
end

if exists (select 1 from sys.default_constraints where name ='DF_AggregateFilter_SoftwareVersion')
begin
ALTER TABLE AggregateFilter					  DROP CONSTRAINT DF_AggregateFilter_SoftwareVersion
ALTER TABLE AggregateFilter					  drop column SoftwareVersion 
end

if exists (select 1 from sys.default_constraints where name ='DF_AggregateFilterContainer_SoftwareVersion')
begin
ALTER TABLE AggregateFilterContainer		  DROP CONSTRAINT DF_AggregateFilterContainer_SoftwareVersion
ALTER TABLE AggregateFilterContainer		  drop column SoftwareVersion 
end

if exists (select 1 from sys.default_constraints where name ='DF_AggregateFilterParameter_SoftwareVersion')
begin
ALTER TABLE AggregateFilterParameter		  DROP CONSTRAINT DF_AggregateFilterParameter_SoftwareVersion
ALTER TABLE AggregateFilterParameter		  drop column SoftwareVersion 
end

if exists (select 1 from sys.default_constraints where name ='DF_Catalogue_SoftwareVersion')
begin
ALTER TABLE Catalogue						  DROP CONSTRAINT DF_Catalogue_SoftwareVersion
ALTER TABLE Catalogue						  drop column SoftwareVersion 
end

if exists (select 1 from sys.default_constraints where name ='DF_CatalogueItem_SoftwareVersion')
begin
ALTER TABLE CatalogueItem					  DROP CONSTRAINT DF_CatalogueItem_SoftwareVersion
ALTER TABLE CatalogueItem					  drop column SoftwareVersion 
end

if exists (select 1 from sys.default_constraints where name ='DF_CatalogueItemIssue_SoftwareVersion')
begin
ALTER TABLE CatalogueItemIssue				  DROP CONSTRAINT DF_CatalogueItemIssue_SoftwareVersion
ALTER TABLE CatalogueItemIssue				  drop column SoftwareVersion 
end

if exists (select 1 from sys.default_constraints where name ='DF_ColumnInfo_SoftwareVersion')
begin
ALTER TABLE ColumnInfo						  DROP CONSTRAINT DF_ColumnInfo_SoftwareVersion
ALTER TABLE ColumnInfo						  drop column SoftwareVersion 
end

if exists (select 1 from sys.default_constraints where name ='DF_ExternalDatabaseServer_SoftwareVersion')
begin
ALTER TABLE ExternalDatabaseServer			  DROP CONSTRAINT DF_ExternalDatabaseServer_SoftwareVersion
ALTER TABLE ExternalDatabaseServer			  drop column SoftwareVersion 
end

if exists (select 1 from sys.default_constraints where name ='DF_ExtractionFilter_SoftwareVersion')
begin
ALTER TABLE ExtractionFilter				  DROP CONSTRAINT DF_ExtractionFilter_SoftwareVersion
ALTER TABLE ExtractionFilter				  drop column SoftwareVersion 
end

if exists (select 1 from sys.default_constraints where name ='DF_ExtractionFilterParameter_SoftwareVersion')
begin
ALTER TABLE ExtractionFilterParameter		  DROP CONSTRAINT DF_ExtractionFilterParameter_SoftwareVersion
ALTER TABLE ExtractionFilterParameter		  drop column SoftwareVersion 
end

if exists (select 1 from sys.default_constraints where name ='DF_ExtractionInformation_SoftwareVersion')
begin
ALTER TABLE ExtractionInformation			  DROP CONSTRAINT DF_ExtractionInformation_SoftwareVersion
ALTER TABLE ExtractionInformation			  drop column SoftwareVersion 
end

if exists (select 1 from sys.default_constraints where name ='DF_IssueSystemUser_SoftwareVersion')
begin
ALTER TABLE IssueSystemUser					  DROP CONSTRAINT DF_IssueSystemUser_SoftwareVersion
ALTER TABLE IssueSystemUser					  drop column SoftwareVersion 
end

if exists (select 1 from sys.default_constraints where name ='DF_JoinInfo_SoftwareVersion')
begin
ALTER TABLE JoinInfo						  DROP CONSTRAINT DF_JoinInfo_SoftwareVersion
ALTER TABLE JoinInfo						  drop column SoftwareVersion 
end

if exists (select 1 from sys.default_constraints where name ='DF_LoadMetadata_SoftwareVersion')
begin
ALTER TABLE LoadMetadata					  DROP CONSTRAINT DF_LoadMetadata_SoftwareVersion
ALTER TABLE LoadMetadata					  drop column SoftwareVersion 
end

if exists (select 1 from sys.default_constraints where name ='DF_LoadModuleAssembly_SoftwareVersion')
begin
ALTER TABLE LoadModuleAssembly				  DROP CONSTRAINT DF_LoadModuleAssembly_SoftwareVersion
ALTER TABLE LoadModuleAssembly				  drop column SoftwareVersion 
end

if exists (select 1 from sys.default_constraints where name ='DF_LoadSchedule_SoftwareVersion')
begin
ALTER TABLE LoadProgress					  DROP CONSTRAINT DF_LoadSchedule_SoftwareVersion
ALTER TABLE LoadProgress					  drop column SoftwareVersion 
end

if exists (select 1 from sys.default_constraints where name ='DF_Pipeline_SoftwareVersion')
begin
ALTER TABLE Pipeline						  DROP CONSTRAINT DF_Pipeline_SoftwareVersion
ALTER TABLE Pipeline						  drop column SoftwareVersion 
end

if exists (select 1 from sys.default_constraints where name ='DF_Lookup_SoftwareVersion')
begin
ALTER TABLE Lookup							  DROP CONSTRAINT DF_Lookup_SoftwareVersion
ALTER TABLE Lookup							  drop column SoftwareVersion 
end

if exists (select 1 from sys.default_constraints where name ='DF_PipelineComponent_SoftwareVersion')
begin
ALTER TABLE PipelineComponent				  DROP CONSTRAINT DF_PipelineComponent_SoftwareVersion
ALTER TABLE PipelineComponent				  drop column SoftwareVersion 
end

if exists (select 1 from sys.default_constraints where name ='DF_LookupCompositeJoinInfo_SoftwareVersion')
begin
ALTER TABLE LookupCompositeJoinInfo			  DROP CONSTRAINT DF_LookupCompositeJoinInfo_SoftwareVersion
ALTER TABLE LookupCompositeJoinInfo			  drop column SoftwareVersion 
end

if exists (select 1 from sys.default_constraints where name ='DF_PipelineComponentArgument_SoftwareVersion')
begin
ALTER TABLE PipelineComponentArgument		  DROP CONSTRAINT DF_PipelineComponentArgument_SoftwareVersion
ALTER TABLE PipelineComponentArgument		  drop column SoftwareVersion 
end

if exists (select 1 from sys.default_constraints where name ='DF_PreLoadDiscardedColumn_SoftwareVersion')
begin
ALTER TABLE PreLoadDiscardedColumn			  DROP CONSTRAINT DF_PreLoadDiscardedColumn_SoftwareVersion
ALTER TABLE PreLoadDiscardedColumn			  drop column SoftwareVersion 
end

if exists (select 1 from sys.default_constraints where name ='DF_ProcessTask_SoftwareVersion')
begin
ALTER TABLE ProcessTask						  DROP CONSTRAINT DF_ProcessTask_SoftwareVersion
ALTER TABLE ProcessTask						  drop column SoftwareVersion 
end

if exists (select 1 from sys.default_constraints where name ='DF_ProcessTaskArgument_SoftwareVersion')
begin
ALTER TABLE ProcessTaskArgument				  DROP CONSTRAINT DF_ProcessTaskArgument_SoftwareVersion
ALTER TABLE ProcessTaskArgument				  drop column SoftwareVersion 
end

if exists (select 1 from sys.default_constraints where name ='DF_SupportingDocument_SoftwareVersion')
begin
ALTER TABLE SupportingDocument				  DROP CONSTRAINT DF_SupportingDocument_SoftwareVersion
ALTER TABLE SupportingDocument				  drop column SoftwareVersion 
end

if exists (select 1 from sys.default_constraints where name ='DF_SupportingSQLTable_SoftwareVersion')
begin
ALTER TABLE SupportingSQLTable				  DROP CONSTRAINT DF_SupportingSQLTable_SoftwareVersion
ALTER TABLE SupportingSQLTable				  drop column SoftwareVersion 
end

if exists (select 1 from sys.default_constraints where name ='DF_TableInfo_SoftwareVersion')
begin
ALTER TABLE TableInfo						  DROP CONSTRAINT DF_TableInfo_SoftwareVersion
ALTER TABLE TableInfo						  drop column SoftwareVersion 
end

if exists (select 1 from sys.default_constraints where name ='DF_CacheProgress_SoftwareVersion')
begin
ALTER TABLE CacheProgress					  DROP CONSTRAINT DF_CacheProgress_SoftwareVersion
ALTER TABLE CacheProgress					  drop column SoftwareVersion 
end

if exists (select 1 from sys.default_constraints where name ='DF_PermissionWindow_SoftwareVersion')
begin
ALTER TABLE PermissionWindow				  DROP CONSTRAINT DF_PermissionWindow_SoftwareVersion
ALTER TABLE PermissionWindow				  drop column SoftwareVersion 
end

if exists (select OBJECT_NAME(object_id),* from sys.sql_modules  where OBJECT_NAME(object_id) ='GetSoftwareVersion')
	DROP FUNCTION dbo.GetSoftwareVersion




--No more default servers	
  
  if exists (select 1 from sys.default_constraints where name ='df_IdentifierDumpServer_ID')
		alter table TableInfo drop constraint df_IdentifierDumpServer_ID

if exists (select 1 from sys.default_constraints where name ='DF_ANOTable_Server_ID')
  alter table ANOTable drop constraint DF_ANOTable_Server_ID

  if exists (select 1 from sys.default_constraints where name ='DF_CohortIdentificationConfiguration_QueryCachingServer_ID')
	alter table CohortIdentificationConfiguration Drop constraint DF_CohortIdentificationConfiguration_QueryCachingServer_ID

  if exists (select 1 from sys.default_constraints where name ='df_LiveLoggingServer_ID')
	alter table Catalogue drop constraint df_LiveLoggingServer_ID

  if exists (select 1 from sys.default_constraints where name ='df_TestLoggingServer_ID')
	alter table Catalogue drop constraint df_TestLoggingServer_ID

if exists (select OBJECT_NAME(object_id),* from sys.sql_modules  where OBJECT_NAME(object_id) ='GetDefaultExternalServerIDFor')
	drop function [dbo].[GetDefaultExternalServerIDFor]

GO
if exists ( select 1 from sys.columns where name = 'TestLoggingServer_ID')
  begin
	alter table Catalogue drop constraint FK_Catalogue_ExternalDatabaseServer1
	alter table Catalogue drop column TestLoggingServer_ID
end