--Version:2.7.0.1
--Description: Adds the type of extraction to the result so the Release can identify where to pick up stuff. Also renames the "Filename" column to "DestinationDescription"
if not exists (select 1 from sys.all_columns where name ='DestinationType' and OBJECT_NAME(object_id) ='CumulativeExtractionResults')
	alter table CumulativeExtractionResults add DestinationType varchar(500) null
GO

if exists(select  1 from sys.columns where name = 'Filename' and OBJECT_NAME(object_id) ='CumulativeExtractionResults')
begin
	exec sp_rename 'CumulativeExtractionResults.Filename','DestinationDescription','COLUMN'
end
GO

-- Update destination type on CumulativeExtractions based on the existing Filename:
UPDATE CumulativeExtractionResults 
SET DestinationType = 'DataExportLibrary.ExtractionTime.ExtractionPipeline.Destinations.ExecuteFullExtractionToDatabaseMSSql' WHERE DestinationDescription NOT LIKE '%:\%'

UPDATE CumulativeExtractionResults 
SET DestinationType = 'DataExportLibrary.ExtractionTime.ExtractionPipeline.Destinations.ExecuteDatasetExtractionFlatFileDestination' WHERE DestinationDescription LIKE '%:\%'
