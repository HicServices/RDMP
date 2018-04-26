--Version:2.8.0.1
--Description: Deprecates existing CustomData tables in favour of 'Project Catalogues'
if exists (select 1 from sys.tables where name ='CohortCustomColumn')
	drop table CohortCustomColumn
GO

if not exists(select  1 from sys.columns where name = 'Project_ID' and OBJECT_NAME(object_id) ='ExtractableDataSet')
begin
	alter table ExtractableDataSet add Project_ID int null
end
GO

if exists (select 1 from sys.columns where name = 'CustomTablesTableName' and OBJECT_NAME(object_id) ='ExternalCohortTable')
begin
alter table ExternalCohortTable drop column CustomTablesTableName
end
go

if not exists (select 1 from sys.foreign_keys where name = 'FK_ExtractableDataSet_Project')
begin
ALTER TABLE [dbo].[ExtractableDataSet]  WITH CHECK ADD  CONSTRAINT [FK_ExtractableDataSet_Project] FOREIGN KEY([Project_ID])
REFERENCES [dbo].[Project] ([ID])
end
GO

--Support for 'join to these tables too' on extraction (Has similar role to AggregateForcedJoin)
if not exists (select 1 from sys.tables where name = 'SelectedDatasetsForcedJoin')
begin
CREATE TABLE [SelectedDatasetsForcedJoin](
	[ID] [int] NOT NULL,
	[SelectedDatasets_ID] [int] NOT NULL,
	[TableInfo_ID] [int] NOT NULL,
 CONSTRAINT [PK_SelectedDatasetsForcedJoin] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)
)
end
go
if not exists (select 1 from sys.foreign_keys where name = 'FK_SelectedDatasetsForcedJoin_SelectedDataSets') 
begin
ALTER TABLE [SelectedDatasetsForcedJoin]  WITH CHECK ADD  CONSTRAINT [FK_SelectedDatasetsForcedJoin_SelectedDataSets] FOREIGN KEY([SelectedDatasets_ID])
REFERENCES [SelectedDataSets] ([ID])
ON DELETE CASCADE

CREATE UNIQUE NONCLUSTERED INDEX [ix_SelectedDatasetsForceJoinsMustBeUnique] ON [dbo].[SelectedDatasetsForcedJoin]
(
	[SelectedDatasets_ID] ASC,
	[TableInfo_ID] ASC
)
end
GO




