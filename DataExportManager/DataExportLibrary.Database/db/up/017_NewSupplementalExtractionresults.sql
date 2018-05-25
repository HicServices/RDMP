--Version:2.9.0.1
--Description: New SupplementalExtractionResults child of CumulativeExtractionResult

if not exists (select 1 from sys.tables where name = 'SupplementalExtractionResults')
begin
    CREATE TABLE [dbo].[SupplementalExtractionResults](
	    [ID] [int] IDENTITY(1,1) NOT NULL,
	    [CumulativeExtractionResults_ID] [int] NULL,
	    [ExtractionConfiguration_ID] [int] NULL,
	    [DestinationDescription] [varchar](max) NULL,
	    [RecordsExtracted] [int] NULL,
	    [Exception] [varchar](max) NULL,
	    [SQLExecuted] [varchar](max) NULL,
        [ExtractedName] [varchar](max) NULL,
	    [ExtractedType] [varchar](max) NULL,
	    [ExtractedId] [int] NULL,
	    [RepositoryType] [varchar](max) NULL,
     CONSTRAINT [PK_SupplementalExtractionResults] PRIMARY KEY CLUSTERED 
    (
	    [ID] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
end
go

if not exists (select 1 from sys.foreign_keys where name = 'FK_SupplementalExtractionResults_CumulativeExtractionResults') 
begin
    ALTER TABLE [dbo].[SupplementalExtractionResults]  WITH CHECK ADD  CONSTRAINT [FK_SupplementalExtractionResults_CumulativeExtractionResults] FOREIGN KEY([CumulativeExtractionResults_ID])
    REFERENCES [dbo].[CumulativeExtractionResults] ([ID])
    ON DELETE CASCADE
end
GO

if not exists (select 1 from sys.foreign_keys where name = 'FK_SupplementalExtractionResults_ExtractionConfiguration') 
begin
	ALTER TABLE [dbo].[SupplementalExtractionResults]  WITH CHECK ADD  CONSTRAINT [FK_SupplementalExtractionResults_ExtractionConfiguration] FOREIGN KEY([ExtractionConfiguration_ID])
	REFERENCES [dbo].[ExtractionConfiguration] ([ID])
end
GO



