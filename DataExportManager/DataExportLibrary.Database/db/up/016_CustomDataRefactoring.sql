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
if not exists (select 1 from sys.tables where name = 'SelectedDataSetsForcedJoin')
begin
CREATE TABLE [SelectedDataSetsForcedJoin](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SelectedDataSets_ID] [int] NOT NULL,
	[TableInfo_ID] [int] NOT NULL,
 CONSTRAINT [PK_SelectedDataSetsForcedJoin] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)
)
end
go
if not exists (select 1 from sys.foreign_keys where name = 'FK_SelectedDataSetsForcedJoin_SelectedDataSets') 
begin
ALTER TABLE [SelectedDataSetsForcedJoin]  WITH CHECK ADD  CONSTRAINT [FK_SelectedDataSetsForcedJoin_SelectedDataSets] FOREIGN KEY([SelectedDataSets_ID])
REFERENCES [SelectedDataSets] ([ID])
ON DELETE CASCADE

CREATE UNIQUE NONCLUSTERED INDEX [ix_SelectedDataSetsForceJoinsMustBeUnique] ON [dbo].[SelectedDataSetsForcedJoin]
(
	[SelectedDataSets_ID] ASC,
	[TableInfo_ID] ASC
)
end
GO




