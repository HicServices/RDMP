--Version:1.5.0.0
--Description:Removes the obsolete 'RawDataSource' column from the LoadMetadata table
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='LoadMetadata' AND COLUMN_NAME='RawDataSource')
BEGIN
	ALTER TABLE LoadMetadata DROP COLUMN RawDataSource
END