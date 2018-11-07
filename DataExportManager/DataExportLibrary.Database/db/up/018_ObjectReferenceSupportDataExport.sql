--Version:2.10.0.1
--Description: Fixes naming for all objects that reference another object in Data Export database

--SupplementalExtractionResults
if(not exists (select 1 from sys.columns where OBJECT_NAME(object_id) = 'SupplementalExtractionResults' and name ='ReferencedObjectID'))
	EXEC sp_rename 'SupplementalExtractionResults.ExtractedId', 'ReferencedObjectID', 'COLUMN'; 

if(not exists (select 1 from sys.columns where OBJECT_NAME(object_id) = 'SupplementalExtractionResults' and name ='ReferencedObjectRepositoryType'))
	EXEC sp_rename 'SupplementalExtractionResults.RepositoryType', 'ReferencedObjectRepositoryType', 'COLUMN'; 

if(not exists (select 1 from sys.columns where OBJECT_NAME(object_id) = 'SupplementalExtractionResults' and name ='ReferencedObjectType'))
	EXEC sp_rename 'SupplementalExtractionResults.ExtractedType', 'ReferencedObjectType', 'COLUMN'; 