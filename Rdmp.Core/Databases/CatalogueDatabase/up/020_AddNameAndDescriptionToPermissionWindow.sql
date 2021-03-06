--Version:1.15.0.0
--Description: Adds the Name and Description column into the PermissionWindow table so that data analysts can document and identify sets of permissions for different data flow source components

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='PermissionWindow' AND COLUMN_NAME='Name')
BEGIN
	ALTER TABLE PermissionWindow ADD Name varchar(512) NULL
END

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='PermissionWindow' AND COLUMN_NAME='Description')
BEGIN
	ALTER TABLE PermissionWindow ADD [Description] varchar(MAX) NULL
END
