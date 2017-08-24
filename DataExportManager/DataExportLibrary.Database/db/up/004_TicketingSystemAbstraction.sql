--Version:1.4.0.0
--Description: Coincides with Catalogue patch 022 (1.16.0.0), removes the explicit JIRA names in all Ticketing columns
 --rename all the JIRATicket fields to ticket
 if exists(select  1 from sys.columns where name = 'JIRAMasterTicket' and OBJECT_NAME(object_id) ='Project')
begin
	exec sp_rename 'Project.JIRAMasterTicket','MasterTicket','COLUMN'
end

 if exists(select  1 from sys.columns where name = 'JIRAReleaseTicket' and OBJECT_NAME(object_id) ='ExtractionConfiguration')
begin
	exec sp_rename 'ExtractionConfiguration.JIRAReleaseTicket','ReleaseTicket','COLUMN'
end

 if exists(select  1 from sys.columns where name = 'JIRARequestTicket' and OBJECT_NAME(object_id) ='ExtractionConfiguration')
begin
	exec sp_rename 'ExtractionConfiguration.JIRARequestTicket','RequestTicket','COLUMN'
end
