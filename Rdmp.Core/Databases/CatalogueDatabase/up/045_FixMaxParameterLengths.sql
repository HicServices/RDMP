--Version:1.39.0.0
--Description: Removes limitation on SQL Parameter values being 500 characters, they can now be as long as you like
if exists (select 1 from sys.columns where name = 'Value' and max_length = 500)
begin
	alter table AnyTableSqlParameter alter column Value varchar(max)
	alter table AggregateFilterParameter alter column Value varchar(max)
	alter table ExtractionFilterParameter alter column Value varchar(max)
end

--Prevent duplicate pipeline names
if not exists (Select 1 from sys.indexes where name = 'ix_preventDuplicatePipelineNames')
begin

--start by finding any duplicates
update 
Pipeline
set Name  = Name + CONVERT(varchar(max),ID) --and append ID to the end of the dupilcate named records
where
Name in (select Name from Pipeline group by Name HAVING count(*)>1) --records with duplicate names

--now create a constraint to prevent users from creating any new ones!
CREATE UNIQUE NONCLUSTERED INDEX [ix_preventDuplicatePipelineNames] ON [dbo].[Pipeline]
(
	[Name] ASC
)
end


--Enable default ANOStore server
if not exists (select 1 from sys.default_constraints where name ='DF_ANOTable_Server_ID')
begin
ALTER TABLE [dbo].[ANOTable] ADD  CONSTRAINT [DF_ANOTable_Server_ID]  DEFAULT ([dbo].[GetDefaultExternalServerIDFor]('ANOTable.Server_ID')) FOR [Server_ID]
end
