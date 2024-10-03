--Version: 8.4.0
--Description: Add ability to allow data loads to order inserts based on promary key

 if not exists (select 1 from sys.columns where name = 'OrderInsertsByPrimaryKey' and OBJECT_NAME(object_id) = 'LoadMetadata')
 BEGIN
 ALTER TABLE [dbo].[LoadMetadata]
 ADD OrderInsertsByPrimaryKey [bit] NOT NULL DEFAULT 0 WITH VALUES
 END