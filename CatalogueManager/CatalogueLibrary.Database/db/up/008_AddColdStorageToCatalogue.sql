--Version:1.6.0.0
--Description:Adds a new flag field 
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Catalogue' AND COLUMN_NAME='IsColdStorageDataset')
BEGIN
	ALTER TABLE Catalogue Add IsColdStorageDataset bit null
END
GO

IF 'YES' = (SELECT IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = 'Catalogue' and COLUMN_NAME = 'IsColdStorageDataset')
BEGIN
update Catalogue set IsColdStorageDataset = 0 where IsColdStorageDataset is null
ALTER TABLE Catalogue alter column IsColdStorageDataset bit not null 
alter table Catalogue add constraint df_IsColdStorageDataset default 0 for IsColdStorageDataset

END