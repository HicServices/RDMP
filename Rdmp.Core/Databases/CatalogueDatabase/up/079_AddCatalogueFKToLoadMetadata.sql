--Version:8.1.5
--Description: Adds FK reference to a catalogue in a LoadMetadata
GO
if not exists (select 1 from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME='LoadMetaData' and COLUMN_NAME='Catalouge_ID')
BEGIN
alter table LoadMetadata
ADD Catalogue_ID integer NULL
CONSTRAINT fkCatalogueID FOREIGN KEY(Catalogue_ID)
REFERENCES Catalogue(id)
END
GO
if exists (select count(*) from RDMP_Catalogue.dbo.LoadMetadata where Catalogue_ID is null) --todo not sure this is correct
BEGIN
update LoadMetadata
set Catalogue_ID = catalogue.id
from Catalogue as catalogue
where catalogue.LoadMetadata_ID = LoadMetadata.id
END
GO
if exists (select 1 from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME='Cataqlogue' and COLUMN_NAME='LoadMetadata_ID')
BEGIN
alter table Catalogue
DROP CONSTRAINT FK_Catalogue_LoadMetadata
END
if exists (select 1 from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME='Cataqlogue' and COLUMN_NAME='LoadMetadata_ID')
BEGIN
alter table Catalogue
DROP COLUMN LoadMetadata_ID
END