--Version:7.0.0
--Description: Updates database to support custom folders on Project
 
-- Add to LoadMetadata table
 if not exists (select 1 from sys.columns where name = 'Folder' AND object_id = OBJECT_ID('Project'))
 begin

ALTER TABLE Project ADD Folder varchar(max) null

end
GO

update Project set Folder = '\' where Folder is null