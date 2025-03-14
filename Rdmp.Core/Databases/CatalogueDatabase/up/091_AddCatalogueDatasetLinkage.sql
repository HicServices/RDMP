--Version: 8.4.4
--Description: Add Linkage between catalogue and dataset
if not exists (select 1 from sys.tables where name = 'CatalogueDatasetLinkage')
BEGIN
CREATE TABLE [dbo].[CatalogueDatasetLinkage](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Catalogue_ID] [int] NOT NULL,
	[Dataset_ID] [int] NOT NULL,
	[Autoupdate] [int] NOT NULL DEFAULT 0,
	CONSTRAINT FK_CatalogueDatasetLinkage_Catalogue_ID FOREIGN KEY (Catalogue_ID) REFERENCES Catalogue(ID)  ON DELETE CASCADE,
	CONSTRAINT FK_CatalogueDatasetLinkage_Dataset_ID FOREIGN KEY (Dataset_ID) REFERENCES Dataset(ID),
CONSTRAINT [PK_CatalogueDatasetLinkage] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO