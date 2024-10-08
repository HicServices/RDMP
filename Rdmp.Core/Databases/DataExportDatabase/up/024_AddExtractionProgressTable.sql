﻿--Version:7.0.0
--Description: Adds the ExtractionProgress object table.  This class stores the progress through a batch extraction

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='ExtractionProgress')
BEGIN

CREATE TABLE ExtractionProgress(
    [ID] [int] IDENTITY(1,1) NOT NULL,
    [Name] varchar(max) NOT NULL,
    [SelectedDataSets_ID] [int] NOT NULL,
    [ProgressDate] [datetime] NULL,
    [ExtractionInformation_ID] [int] NOT NULL,    
    [StartDate] [datetime] NULL,
    [EndDate] [datetime] NULL,
    [NumberOfDaysPerBatch] [int] NOT NULL,

    CONSTRAINT FK_ExtractionProgress_SelectedDataSets FOREIGN KEY ([SelectedDataSets_ID])
      REFERENCES SelectedDataSets (ID)
      ON DELETE CASCADE
      ON UPDATE CASCADE,

 CONSTRAINT [PK_ExtractionProgress] PRIMARY KEY CLUSTERED 
(
    [ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


CREATE UNIQUE INDEX ix_OneExtractionProgressPerDataset 
ON ExtractionProgress(SelectedDataSets_ID);

IF NOT EXISTS (SELECT 1 FROM sys.change_tracking_tables WHERE object_id = OBJECT_ID('ExtractionProgress')) 
    ALTER TABLE ExtractionProgress							ENABLE CHANGE_TRACKING 

END
