----Version: 9.2.0
----Description: Allow a catalogue to be linked to multiple projects

if not exists (select 1 from sys.columns where name = 'MostRecentCohortUsed_ID' and OBJECT_NAME(object_id) = 'ExtractionConfiguration')
BEGIN
ALTER TABLE [dbo].[ExtractionConfiguration]
ADD MostRecentCohortUsed_ID INT NULL
END

if exists(select 1 from sys.columns where name='ExtractionInformation_ID' and OBJECT_NAME(object_id) = 'ExtractionProgress' and is_nullable=0)
BEGIN
alter table [dbo].[ExtractionProgress]
alter column ExtractionInformation_ID int null
END

if not exists (select 1 from sys.columns where name = 'IsDeltaExtraction' and OBJECT_NAME(object_id) = 'ExtractionProgress')
BEGIN
ALTER TABLE [dbo].[ExtractionProgress]
ADD IsDeltaExtraction bit not null default 0
END
