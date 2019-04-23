--Version:1.0.0.1
--Description:Allows you to cache files in different locations than the HIC Project directory
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='LoadSchedule' AND COLUMN_NAME='AlternativeCacheLocation')
BEGIN
	ALTER TABLE LoadSchedule ADD AlternativeCacheLocation varchar(3000) NULL
END