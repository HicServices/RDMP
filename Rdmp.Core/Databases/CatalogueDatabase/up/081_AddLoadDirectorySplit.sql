--Version:8.1.5
--Description: Allow for dispirate locations for data load directories
if not exists (select 1 from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME='LoadMetaData' and COLUMN_NAME='LocationOfForLoadingDirectory')
BEGIN       
	ALTER TABLE LoadMetadata ADD LocationOfForLoadingDirectory varchar(3000) NULL;
	ALTER TABLE LoadMetadata ADD LocationOfForArchivingDirectory [varchar](3000) NULL;
	ALTER TABLE LoadMetadata ADD LocationOfExecutablesDirectory [varchar](3000) NULL;
	ALTER TABLE LoadMetadata ADD LocationOfCacheDirectory [varchar](3000) NULL;
END


if exisits(select 1 from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME='LoadMetaData' and COLUMN_NAME='LocationOfFlatFiles')
BEGIN
update RDMP_Catalogue.dbo.LoadMetadata 
set LocationOfForLoadingDirectory = LocationOfFlatFiles +'\Data\ForLoading'
set LocationOfForArchivingDirectory = LocationOfFlatFiles +'\Data\ForArchiving'
set LocationOfExecutablesDirectory = LocationOfFlatFiles +'\Executables'
set LocationOfCacheDirectory = LocationOfFlatFiles +'\Data\Cache'
where LocationOfFlatFiles is not null
END
