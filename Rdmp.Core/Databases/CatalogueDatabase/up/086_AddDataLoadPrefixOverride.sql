--Version: 8.2.4
--Description: Add ability to allow data loads to import columns with the reserved column _hic

 if not exists (select 1 from sys.columns where name = 'AllowReservedPrefix' and OBJECT_NAME(object_id) = 'LoadMetadata')
 BEGIN
 ALTER TABLE [dbo].[LoadMetadata]
 ADD AllowReservedPrefix [bit] NOT NULL DEFAULT 0 WITH VALUES
 END