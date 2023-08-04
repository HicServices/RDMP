--Version:7.0.0
--Description: Updates database to support custom folders on CohortIdentificationConfiguration and LoadMetadata
 
-- Add to LoadMetadata table
	alter table Catalogue drop constraint FK_Catalogue_ExternalDatabaseServer
