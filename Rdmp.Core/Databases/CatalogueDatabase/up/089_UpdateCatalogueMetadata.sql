--Version: 8.4.3
--Description: Add new metadata fields for catalogues
if not exists (select 1 from sys.columns where name = 'ControlledVocabulary' and OBJECT_NAME(object_id) = 'Catalogue')
BEGIN
ALTER TABLE dbo.[Catalogue]
ADD
ShortDescription [nvarchar](250),
DataType [nvarchar](30),
DataSubType [nvarchar](30),
DataSource [nvarchar](100),
DataSourceSetting [nvarchar](100),
DatasetReleaseDate [datetime],
StartDate [datetime],
EndDate [datetime],
UpdateLag [nvarchar](255),
Juristiction [nvarchar](255),
DataController [nvarchar](255),
DataProcessor [nvarchar](255),
ControllerVocabulary [nvarchar](MAX),
AssociatedPeople [nvarchar](MAX)
END
GO


