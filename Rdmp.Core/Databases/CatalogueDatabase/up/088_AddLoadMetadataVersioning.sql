----Version: 8.4.0
----Description: Add Load Metadata Versioning
if not exists (select 1 from sys.columns where name = 'Version' and OBJECT_NAME(object_id) = 'LoadMetadata')
BEGIN
ALTER TABLE [dbo].[LoadMetadata]
ADD [Version] [int]
END

if not exists (select 1 from sys.columns where name = 'RootLoadMetadata' and OBJECT_NAME(object_id) = 'LoadMetadata')
BEGIN
ALTER TABLE [dbo].[LoadMetadata]
ADD [RootLoadMetadata] [int] ---fk cascade delete?
END