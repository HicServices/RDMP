--Version:8.1.4
--Description: Adds LastLoadTime to LoadMetadata table
 GO
if not exists (select 1 from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME='LoadMetadata' and COLUMN_NAME='LastLoadTime')
BEGIN       
ALTER TABLE LoadMetadata ADD LastLoadTime DATETIME NULL;
END
