--Version: 8.4.4
--Description: Add new metadata fields for catalogues
if not exists (select 1 from sys.tables where name = 'CatalogueDatasetLinkage')
BEGIN
CREATE TABLE [dbo].[CatalogueDatasetLinkage](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Catalogue_ID] [int] NOT NULL,
	[Dataset_ID] [int] NOT NULL,
	[Autoupdate] [int] NOT NULL DEFAULT 0,
	CONSTRAINT FK_CatalogueDatasetLinkage_Catalogue_ID FOREIGN KEY (Catalogue_ID) REFERENCES Catalogue(ID)  ON DELETE CASCADE,
	CONSTRAINT FK_CatalogueDatasetLinkage_Dataset_ID FOREIGN KEY (Dataset_ID) REFERENCES Dataset(ID) ON DELETE CASCADE,
CONSTRAINT [PK_CatalogueDatasetLinkage] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO

if not exists (select 1 from sys.tables where name = 'DatasetProviderConfiguration')
BEGIN
CREATE TABLE [dbo].[DatasetProviderConfiguration](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	Type [nvarchar](256) NOT NULL,
	Url [nvarchar](500) NOT NULL,
	Name [nvarchar](256) NOT NULL,
	DataAccessCredentials_ID [int] NOT NULL,
	Organisation_ID [nvarchar](256) NOT NULL
	CONSTRAINT FK_DatasetProviderConfiguration_DataAccessCredentials FOREIGN KEY (DataAccessCredentials_ID) REFERENCES DataAccessCredentials(ID)  ON DELETE CASCADE,
CONSTRAINT [PK_DatasetProviderConfiguration] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
if not exists (select 1 from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME='Dataset' and COLUMN_NAME='Type')
BEGIN
ALTER TABLE [dbo].[Dataset]
ADD [Type] [varchar](256) NULL,
	[Url] [varchar](256) NULL,
	[Provider_ID] [int] NULL
END
GO