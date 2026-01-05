----Version: 9.0.0
----Description: Allow a catalogue to be linked to multiple projects

if exists(select 1 from sys.indexes where name='PreventDoubleAddingCatalogueIdx' and OBJECT_NAME(object_id) = 'ExtractableDataSet')
BEGIN
 DROP INDEX PreventDoubleAddingCatalogueIdx on [dbo].[ExtractableDataSet]
END

if exists(select 1 from sys.columns where name = 'Project_ID' and OBJECT_NAME(object_id) = 'ExtractableDataSet')
BEGIN
ALTER TABLE [dbo].[ExtractableDataSet]
DROP CONSTRAINT [FK_ExtractableDataSet_Project]
END

-- not droping this makes it backwards compatable
--if exists(select 1 from sys.columns where name = 'Project_ID' and OBJECT_NAME(object_id) = 'ExtractableDataSet')
--ALTER TABLE [dbo].[ExtractableDataSet]
--DROP COLUMN [Project_ID]

if not exists(select 1 from sys.indexes where name='PreventDoubleAddingCatalogueProjectIdx' and OBJECT_NAME(object_id) = 'ExtractableDataSet')
BEGIN
CREATE UNIQUE NONCLUSTERED INDEX [PreventDoubleAddingCatalogueIdx] ON [dbo].[ExtractableDataSet]
(
	[Catalogue_ID] ASC
)
END

if not exists (select 1 from sys.tables where name='ExtractableDataSetProject')
BEGIN
CREATE TABLE [dbo].[ExtractableDataSetProject](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Project_ID] [int] NOT NULL,
	[ExtractableDataSet_ID] [int] NOT NULL,
	CONSTRAINT [fk_ExtractableDataSetProject] FOREIGN KEY([ExtractableDataSet_ID])
 REFERENCES [dbo].[ExtractableDataSet] ([ID])
 ON DELETE CASCADE,
	CONSTRAINT [PK_ExtractableDataSetProject] PRIMARY KEY CLUSTERED(
		[Project_ID], [ExtractableDataSet_ID]	
	)
)
END

if (((select count(*) from [ExtractableDataSetProject]) = 0) AND (select count(*) from sys.columns where name = 'Project_ID' and OBJECT_NAME(object_id) = 'ExtractableDataSet')!=0)
BEGIN
DECLARE @SQLString AS NVARCHAR (500);
set @SQLString = 'INSERT INTO [dbo].[ExtractableDataSetProject](ExtractableDataSet_ID,Project_ID) SELECT  [ID], [Project_ID] FROM [dbo].[ExtractableDataSet] WHERE [Project_ID] is not null'
EXEC (@SQLString)
END