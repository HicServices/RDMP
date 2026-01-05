--Version:8.2.0
--Description: Allow for dispirate locations for data load directories
if not exists (select 1 from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME='LoadMetadata' and COLUMN_NAME='LocationOfForLoadingDirectory')
BEGIN       
	ALTER TABLE [dbo].[LoadMetadata] ADD LocationOfForLoadingDirectory varchar(3000) NULL;
	ALTER TABLE [dbo].[LoadMetadata] ADD LocationOfForArchivingDirectory [varchar](3000) NULL;
	ALTER TABLE [dbo].[LoadMetadata] ADD LocationOfExecutablesDirectory [varchar](3000) NULL;
	ALTER TABLE [dbo].[LoadMetadata] ADD LocationOfCacheDirectory [varchar](3000) NULL;
END
GO

if exists(select 1 from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME='LoadMetadata' and COLUMN_NAME='LocationOfFlatFiles')
BEGIN
	Declare @SplitMetaDataSQL varchar(max)= '
	update [dbo].[LoadMetadata] 
	set LocationOfForLoadingDirectory = LocationOfFlatFiles +''\Data\ForLoading\''
	where LocationOfFlatFiles is not null
	update [dbo].[LoadMetadata]
	set LocationOfForArchivingDirectory = LocationOfFlatFiles +''\Data\ForArchiving\''
	where LocationOfFlatFiles is not null
	update [dbo].[LoadMetadata]
	set LocationOfExecutablesDirectory = LocationOfFlatFiles +''\Executables\''
	where LocationOfFlatFiles is not null
	update [dbo].[LoadMetadata]
	set LocationOfCacheDirectory = LocationOfFlatFiles +''\Data\Cache\''
	where LocationOfFlatFiles is not null
	'
	EXEC(@SplitMetaDataSQL)
END
GO