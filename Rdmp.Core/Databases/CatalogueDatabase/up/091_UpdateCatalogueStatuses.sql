﻿--Version: 8.4.3
--Description: Add new metadata fields for catalogues
--update Catalogue
--set IsInternalDataset=1
--FROM RDMP_Catalogue.dbo.Catalogue as Catalogue
--left join RDMP_DataExport.dbo.ExtractableDataSet as eds on Catalogue.ID = eds.Catalogue_ID
--where eds.ID is NULL