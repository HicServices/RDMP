--Version: 8.4.3
--Description: Add data load versioning

 if not exists (select 1 from sys.columns where name = 'RootLoadMetadata_ID' and OBJECT_NAME(object_id) = 'LoadMetadata')
 BEGIN
  ALTER TABLE [dbo].[LoadMetadata]
  ADD RootLoadMetadata_ID [int] NULL,
  CONSTRAINT [fk_loadMetadataRootReference] FOREIGN KEY(RootLoadMetadata_ID) REFERENCES [dbo].[LoadMetadata](id)
 END



