--Version:1.49.0.1
--Description: Adds a Name field onto ExtractionConfiguration, adds support for Dataset Packages which are collections of ExtractableDataSets e.g. CoreDatasets, Supplemental Datasets, Expensive Datasets etc
if not exists (select 1 from sys.columns where name = 'Name' and OBJECT_NAME(object_id) = 'ExtractionConfiguration')
  begin
	alter table ExtractionConfiguration add Name varchar(1000) null
  end
  GO
  
  --Now there should be no populated Names at this point since we just created the column
  if (select count(*) from ExtractionConfiguration where Name is not null) = 0 
  begin
		--user will not have any names yet so just use the Description for the names for now
		update ExtractionConfiguration set Name = LEFT(Description,1000);

		--he might have some that don't have descriptions
		update  ExtractionConfiguration set Name = 'Unnamed Configuration' where Name is null;

		--now make it not null
		alter table ExtractionConfiguration alter column Name varchar(1000) not null
  end

  -------------------------------------------Support for ExtractableDataset Packages-----------------------
if not exists (select 1 from sys.tables where name = 'ExtractableDataSetPackage' )
begin
CREATE TABLE [dbo].[ExtractableDataSetPackage](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](1000) NOT NULL,
	[Creator] [varchar](500) NOT NULL,
	[CreationDate] [datetime] NOT NULL,
 CONSTRAINT [PK_ExtractableDataSetPackage] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)
)
end

GO

if not exists (select 1 from sys.tables where name = 'ExtractableDataSetPackage_ExtractableDataSet' )
begin
CREATE TABLE [dbo].[ExtractableDataSetPackage_ExtractableDataSet](
	[ExtractableDataSet_ID] [int] NOT NULL,
	[ExtractableDataSetPackage_ID] [int] NOT NULL,
	CONSTRAINT [PK_ExtractableDataSetPackage_ExtractableDataSet] PRIMARY KEY CLUSTERED 
(
	[ExtractableDataSet_ID] ASC,
	[ExtractableDataSetPackage_ID] ASC
)
) ON [PRIMARY]

end


if not exists (select 1 from sys.foreign_keys where name = 'FK_ExtractableDataSetPackage_ExtractableDataSet_ExtractableDataSet')
begin

ALTER TABLE [dbo].[ExtractableDataSetPackage_ExtractableDataSet]  WITH CHECK ADD  CONSTRAINT [FK_ExtractableDataSetPackage_ExtractableDataSet_ExtractableDataSet] FOREIGN KEY([ExtractableDataSet_ID])
REFERENCES [dbo].[ExtractableDataSet] ([ID]) 

ALTER TABLE [dbo].[ExtractableDataSetPackage_ExtractableDataSet]  WITH CHECK ADD  CONSTRAINT [FK_ExtractableDataSetPackage_ExtractableDataSet_ExtractableDataSetPackage] FOREIGN KEY([ExtractableDataSetPackage_ID])
REFERENCES [dbo].[ExtractableDataSetPackage] ([ID]) ON DELETE CASCADE

end

-------------------------------------------Get Rid of Null Catalogue_IDs-----------------------
if exists (select 1 from sys.columns where name = 'Catalogue_ID' and is_nullable =1)
begin
	delete from ExtractableDataSet where Catalogue_ID is null

	drop index ExtractableDataSet.[PreventDoubleAddingCatalogueIdx]

	alter table ExtractableDataSet alter column Catalogue_ID int not null
		
	CREATE UNIQUE NONCLUSTERED INDEX [PreventDoubleAddingCatalogueIdx] ON [dbo].[ExtractableDataSet]
	(
		[Catalogue_ID] ASC
	)

end

--------------------------------------Add an ID to SelectedDataSets so we can make it a proper data type -----------------
if not exists(select * from sys.columns where object_name(object_id) = 'SelectedDataSets' and name = 'ID')
begin
ALTER TABLE SelectedDataSets
   ADD ID INT IDENTITY

ALTER TABLE SelectedDataSets
   ADD CONSTRAINT PK_SelectedDataSets
   PRIMARY KEY(ID)
end
