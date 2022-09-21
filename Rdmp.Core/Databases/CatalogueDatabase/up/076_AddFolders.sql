--Version:7.0.0
--Description: Updates database to support custom folders on CohortIdentificationConfiguration and LoadMetadata
 
-- Add to LoadMetadata table
 if not exists (select 1 from sys.columns where name = 'Folder' AND object_id = OBJECT_ID('LoadMetadata'))
 begin

ALTER TABLE LoadMetadata ADD Folder varchar(max) null

end
GO

update LoadMetadata set Folder = '\' where Folder is null


-- Add to CohortIdentificationConfiguration table
if not exists (select 1 from sys.columns where name = 'Folder' AND object_id = OBJECT_ID('CohortIdentificationConfiguration'))
begin
ALTER TABLE CohortIdentificationConfiguration ADD Folder varchar(max) null
end
GO

update CohortIdentificationConfiguration set Folder = '\' where Folder is null
