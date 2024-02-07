--Version:8.1.4
--Description: Adds FK reference to a catalogue in a LoadMetadata
GO
if not exists (select 1 from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME='LoadMetaData' and COLUMN_NAME='Catalouge_ID')
BEGIN
alter table RDMP_Catalogue.dbo.LoadMetadata
ADD Catalogue_ID integer NULL
CONSTRAINT fkCatalogueID FOREIGN KEY(Catalogue_ID)
REFERENCES RDMP_CATALOGUE.dbo.Catalogue(id)
END
GO
if exists (select count(*) from RDMP_Catalogue.dbo.LoadMetadata where Catalogue_ID is null) --todo not sure this is correct
BEGIN
update RDMP_Catalogue.dbo.LoadMetadata
set Catalogue_ID = catalogue.id
from RDMP_Catalogue.dbo.Catalogue as catalogue
where catalogue.LoadMetadata_ID = RDMP_Catalogue.dbo.LoadMetadata.id
END


--TODO
--alter table RDMP_Catalogue.dbo.Catalogue
--drop constraint FK_Catalogue_LoadMetadata
--drop column LoadMetadata_ID