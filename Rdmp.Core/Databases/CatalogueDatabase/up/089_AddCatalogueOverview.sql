--Version: 8.4.1
--Description: Add catalogue Overview


if not exists (select 1 from sys.tables where name = 'CatalogueOverview')
BEGIN
CREATE TABLE [dbo].[CatalogueOverview](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Catalogue_ID] [int] NOT NULL,
	[LastDataLoad] [datetime] NULL,
	[LastExtractionTime] [datetime] NULL,
	[NumberOfRecords] [int] NOT NULL,
	[NumberOfPeople] [int] NOT NULL,
	[DateColumn_ID] [int] NOT NULL,
	[StartDate] [datetime] NULL,
	[EndDate] [datetime] NULL,
	CONSTRAINT FK_Catalogue_Overview_Catalogue FOREIGN KEY (Catalogue_ID) REFERENCES Catalogue(ID) ON DELETE CASCADE,
CONSTRAINT PK_CatalogueOverview PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO

if not exists (select 1 from sys.tables where name = 'CatalogueOverviewDataPoint')
BEGIN
CREATE TABLE [dbo].[CatalogueOverviewDataPoint](
    [ID] [int] IDENTITY(1,1) NOT NULL,
	[CatalogueOverview_ID] [int] NOT NULL,
	[Date] [datetime] NOT NULL,
	[Count] int NOT NULL,
	CONSTRAINT FK_Catalogue_Overview_DataPoint_Catalogue_Overview FOREIGN KEY (CatalogueOverview_ID) REFERENCES CatalogueOverview(ID) ON DELETE CASCADE,
	CONSTRAINT PK_CatalogueOverviewDataPoin2t PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
