--Version: 9.2.0
--Description: Add new metadata fields for catalogues
if not exists (select 1 from sys.columns where name = 'InternalNote' and OBJECT_NAME(object_id) = 'Catalogue')
BEGIN
ALTER TABLE [dbo].[Catalogue]
ADD
[InternalNote] [nvarchar](max) NULL
END
GO


