--Version:7.0.0
--Description: Updates database to support custom folders on CohortIdentificationConfiguration and LoadMetadata
 
-- Add to LoadMetadata table
	alter table Catalogue drop constraint FK_Catalogue_ExternalDatabaseServer
-- this might be better by making specific serveres ignorable rather than deleting the FK
-- would have to check if a logging server is always generated on launch