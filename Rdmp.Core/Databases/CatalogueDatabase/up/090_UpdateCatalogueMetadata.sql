--Version: 8.4.3
--Description: Add new metadata fields for catalogues
if not exists (select 1 from sys.columns where name = 'ControlledVocabulary' and OBJECT_NAME(object_id) = 'Catalogue')
BEGIN
ALTER TABLE [dbo].[Catalogue]
ADD
[ShortDescription] [nvarchar](250) NULL,
[DataType] [nvarchar](255) NULL,
[DataSubType] [nvarchar](255) NULL,
[DataSource] [nvarchar](100) NULL,
[DataSourceSetting] [nvarchar](100) NULL,
[DatasetReleaseDate] [datetime] NULL,
[StartDate] [datetime] NULL,
[EndDate] [datetime] NULL,
[UpdateLag] [nvarchar](255) NULL,
[Juristiction] [nvarchar](255) NULL,
[DataController] [nvarchar](255) NULL,
[DataProcessor] [nvarchar](255) NULL,
[ControlledVocabulary] [nvarchar](MAX) NULL,
[AssociatedPeople] [nvarchar](MAX) NULL,
[Doi] [nvarchar](50) NULL,
[Purpose] [nvarchar](255) NULL,
[AssociatedMedia] [nvarchar](max) NULL
END
GO


