--Version: 8.1.5
--Description: Adds linking table to allow for multiple load metadatas per catalogue
if not exists (select 1 from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME='LoadMetadataCatalogueLinkage')
BEGIN
CREATE TABLE [dbo].[LoadMetadataCatalogueLinkage](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[LoadMetadataID] [int] NOT NULL,
	[CatalogueID] [int] NOT NULL,
	FOREIGN KEY ([LoadMetadataID]) REFERENCES [dbo].[LoadMetadata](ID) ON DELETE CASCADE,
	FOREIGN KEY ([CatalogueID]) REFERENCES [dbo].[Catalogue](ID) ON DELETE CASCADE,
	CONSTRAINT [PK_LoadMetadataCatalogueLinkage] PRIMARY KEY CLUSTERED
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
CREATE INDEX LoadMetadataCatalogueLinkage_Index
ON [dbo].[LoadMetadataCatalogueLinkage] ([LoadMetadataID],[CatalogueID])
END

IF COL_LENGTH('dbo.Catalogue','LoadMetadata_ID') IS NOT NULL
BEGIN
insert into [dbo].[LoadMetadataCatalogueLinkage]([CatalogueID], [LoadMetadataID])
	select ID as CatalogueID, LoadMetadata_ID as LoadMetadataID
	from [dbo].[Catalogue]
	where LoadMetadata_ID is not null
END
GO
IF COL_LENGTH('dbo.Catalogue','LoadMetadata_ID') IS NOT NULL
BEGIN
IF (OBJECT_ID('FK_Catalogue_LoadMetadata') IS NOT NULL)
  ALTER TABLE [dbo].[Catalogue]
  DROP CONSTRAINT [FK_Catalogue_LoadMetadata]
END

IF COL_LENGTH('dbo.Catalogue','LoadMetadata_ID') IS NOT NULL
BEGIN
	ALTER TABLE [dbo].[Catalogue]
	DROP column LoadMetadata_ID
END