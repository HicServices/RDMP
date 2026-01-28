----Version: 9.2.0
----Description: Allow a catalogue to be linked to multiple projects

if not exists (select 1 from sys.columns where name = 'MostRecentCohortUsed_ID' and OBJECT_NAME(object_id) = 'ExtractionConfiguration')
BEGIN
ALTER TABLE [dbo].[ExtractionConfiguration]
ADD MostRecentCohortUsed_ID INT NULL
END
