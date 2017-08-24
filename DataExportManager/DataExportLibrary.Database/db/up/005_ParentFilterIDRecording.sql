--Version:1.5.0.0
--Description: Adds new field ClonedFromExtractionFilter_ID that allows us to monitor differences between Catalogue and Extraction Filter versions
if not exists (select 1 from sys.columns where name = 'ClonedFromExtractionFilter_ID' and OBJECT_NAME(object_id) = 'DeployedExtractionFilter')
	alter table DeployedExtractionFilter add ClonedFromExtractionFilter_ID int null

if (select max_length from sys.columns where name = 'Description' and OBJECT_NAME(object_id) = 'DeployedExtractionFilter') <> -1
	alter table DeployedExtractionFilter alter column Description varchar(max)