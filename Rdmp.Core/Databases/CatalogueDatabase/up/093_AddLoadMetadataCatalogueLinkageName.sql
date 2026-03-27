--Version: 9.3.0
--Description: Add name to load metadata catalogue linkage
if not exists (select 1 from sys.columns where name = 'Name' and OBJECT_NAME(object_id) = 'LoadMetadataCatalogueLinkage')
BEGIN
ALTER TABLE [dbo].[LoadMetadataCatalogueLinkage]
ADD
[Name] [nvarchar](max) NOT NULL  DEFAULT 'Unknown Logging Task';
END
GO

if exists(select 1 from [dbo].[LoadMetadataCatalogueLinkage] where [Name]='Unknown Logging Task')
BEGIN
 UPDATE [dbo].[LoadMetadataCatalogueLinkage]
  SET [dbo].[LoadMetadataCatalogueLinkage].[Name] = [C].[LoggingDataTask]
  FROM [dbo].[LoadMetadataCatalogueLinkage] [LMDCL]
  INNER JOIN [dbo].[Catalogue] [C]
  ON [C].[ID] = [LMDCL].[CatalogueID]
  WHERE [C].[LoggingDataTask] IS NOT NULL AND [LMDCL].[Name] = 'Unknown Logging Task'
END
GO