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

if not exists (select 1 from sys.foreign_keys where name = 'FK_ExtractableDataSet_Project')
begin
ALTER TABLE [dbo].[ExtractableDataSet]  WITH CHECK ADD  CONSTRAINT [FK_ExtractableDataSet_Project] FOREIGN KEY([Project_ID])
REFERENCES [dbo].[Project] ([ID])
end
GO
