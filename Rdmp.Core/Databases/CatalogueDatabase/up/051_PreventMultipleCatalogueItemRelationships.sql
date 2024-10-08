--Version:1.45.0.1
--Description: Removes the ability to have multiple ColumnInfo objects associated with a single CatalogueItem, this hasn't been a thing for a while although it was originally a requirement. Now it is a requirement that we don't have it! it just makes things needlessly complicated.  Also adds support for ExtractionFilterParameterSet which are currated 'pre canned' options for populating parameters in Catalogue Filters.  Finally we also increased the maximum length of lots of Name fields that were very short before


if not exists (select * from sys.key_constraints where name = 'PK_CatalogueItem_ID' )
begin 
	delete from 
	ColumnInfo_CatalogueItem
	where
	CatalogueItem_ID in (select CatalogueItem_ID from
	ColumnInfo_CatalogueItem 
	group by 
	CatalogueItem_ID
	having count(*) > 1)
	and ExtractionInformation_ID is null

	DELETE FROM ColumnInfo_CatalogueItem WHERE CatalogueItem_ID is null

	ALTER TABLE ColumnInfo_CatalogueItem ALTER COLUMN CatalogueItem_ID int NOT NULL
end
GO

if not exists (select * from sys.key_constraints where name = 'PK_CatalogueItem_ID' )
begin
	
	ALTER TABLE ColumnInfo_CatalogueItem ADD CONSTRAINT PK_CatalogueItem_ID PRIMARY KEY (CatalogueItem_ID)

end
GO

if not exists (select * from sys.columns where name = 'ColumnInfo_ID' and OBJECT_NAME(object_id) = 'CatalogueItem')
begin

	ALTER TABLE CatalogueItem add ColumnInfo_ID int null
	ALTER TABLE ExtractionInformation add CatalogueItem_ID int null
end
GO

if exists (select * from sys.foreign_keys where name = 'FK_Catalogue_ExtractionInformation')
begin

--Fix the name of this Foreign key which would introduce circular dependency and fix its CASCADE
ALTER TABLE Catalogue drop FK_Catalogue_ExtractionInformation

ALTER TABLE [dbo].[Catalogue]  WITH CHECK ADD CONSTRAINT [FK_TimeCoverageCategory_ExtractionInformation_ID] FOREIGN KEY(TimeCoverage_ExtractionInformation_ID)
REFERENCES [dbo].[ExtractionInformation] ([ID])


--Remove cascading on this relationship too
ALTER TABLE AggregateDimension  DROP [FK_AggregateDimension_ExtractionInformation]

ALTER TABLE [dbo].[AggregateDimension]  WITH CHECK ADD  CONSTRAINT [FK_AggregateDimension_ExtractionInformation] FOREIGN KEY([ExtractionInformation_ID])
REFERENCES [dbo].[ExtractionInformation] ([ID])

end

if exists (select * from sys.tables where name = 'ColumnInfo_CatalogueItem')
begin
UPDATE CatalogueItem SET ColumnInfo_ID = (select top 1 ColumnInfo_ID from [ColumnInfo_CatalogueItem] where CatalogueItem_ID = CatalogueItem.ID)
UPDATE ExtractionInformation SET CatalogueItem_ID = (select top 1 CatalogueItem_ID from [ColumnInfo_CatalogueItem] where ExtractionInformation_ID = ExtractionInformation.ID)

drop table [ColumnInfo_CatalogueItem]
  
ALTER TABLE [dbo].[CatalogueItem]  WITH CHECK ADD  CONSTRAINT [FK_CatalogueItem_ColumnInfo] FOREIGN KEY([ColumnInfo_ID])
REFERENCES [dbo].[ColumnInfo] ([ID]) ON DELETE SET NULL

ALTER TABLE [dbo].ExtractionInformation  WITH CHECK ADD CONSTRAINT [FK_ExtractionInformation_CatalogueItem] FOREIGN KEY([CatalogueItem_ID])
REFERENCES [dbo].CatalogueItem ([ID]) ON DELETE CASCADE
end
GO

DELETE FROM ExtractionInformation WHERE CatalogueItem_ID is null
Alter table ExtractionInformation alter column CatalogueItem_ID int not null


if not exists (select 1 from sys.tables where name ='ExtractionFilterParameterSet')
begin

	CREATE TABLE [dbo].[ExtractionFilterParameterSet](
		[ID] [int] IDENTITY(1,1) NOT NULL,
		[Name] [varchar](max) NOT NULL,
		[Description] [varchar](max) NULL,
		[ExtractionFilter_ID] [int] NOT NULL,
		CONSTRAINT [FK_ExtractionFilterParameterSet_ExtractionFilter] FOREIGN KEY([ExtractionFilter_ID]) REFERENCES [dbo].[ExtractionFilter] ([ID]),
	 CONSTRAINT [PK_ExtractionFilterParameterSet] PRIMARY KEY CLUSTERED 
	(
		[ID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]



	CREATE TABLE [dbo].[ExtractionFilterParameterSetValue](
		[ID] [int] IDENTITY(1,1) NOT NULL,
		[ExtractionFilterParameterSet_ID] [int] NOT NULL,
		[ExtractionFilterParameter_ID] [int] NOT NULL,
		[Value] [varchar](max) NULL,
		CONSTRAINT [FK_ExtractionFilterParameterSetValue_ExtractionFilterParameter] FOREIGN KEY([ExtractionFilterParameter_ID]) REFERENCES [dbo].[ExtractionFilterParameter] ([ID]),
		 CONSTRAINT [FK_ExtractionFilterParameterSetValue_ExtractionFilterParameterSet] FOREIGN KEY([ExtractionFilterParameterSet_ID]) REFERENCES [dbo].[ExtractionFilterParameterSet] ([ID]),
	 CONSTRAINT [PK_ExtractionFilterParameterSetValue] PRIMARY KEY CLUSTERED 
	(
		[ID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

end



alter table AggregateFilter		alter column Name	varchar (1000) not null
alter table ExtractionFilter	alter column Name	varchar (1000) not null
alter table SupportingDocument	alter column Name	varchar (1000) not null
alter table SupportingSQLTable	alter column Name	varchar (1000) not null