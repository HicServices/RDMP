--Version: 9.3.0
--Description: Add new metadata fields for catalogues
if not exists (select 1 from sys.columns where name = 'ExtractionName' and OBJECT_NAME(object_id) = 'Catalogue')
BEGIN
ALTER TABLE [dbo].[Catalogue]
ADD
[ExtractionName] [varchar](1000) NULL
END
GO


