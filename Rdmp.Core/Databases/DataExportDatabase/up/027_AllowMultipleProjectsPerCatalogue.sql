----Version: 9.0.0
----Description: Allow a catalogue to be linked to multiple projects

if exists(select 1 from sys.indexes where name='PreventDoubleAddingCatalogueIdx' and OBJECT_NAME(object_id) = 'ExtractableDataSet')
BEGIN
 DROP INDEX PreventDoubleAddingCatalogueIdx on [dbo].[ExtractableDataSet]
END

if not exists(select 1 from sys.indexes where name='PreventDoubleAddingCatalogueProjectIdx' and OBJECT_NAME(object_id) = 'ExtractableDataSet')
BEGIN
CREATE UNIQUE NONCLUSTERED INDEX [PreventDoubleAddingCatalogueIdx] ON [dbo].[ExtractableDataSet]
(
	[Catalogue_ID] ASC,
	[Project_ID] ASC
)
END

