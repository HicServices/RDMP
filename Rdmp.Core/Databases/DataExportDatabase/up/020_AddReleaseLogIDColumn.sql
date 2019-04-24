--Version:2.13.0.1
--Description: Adds a column called ID to ReleaseLog allowing it to be created/deleted more easily.

  if( exists (select * from sys.key_constraints where type ='PK' AND OBJECT_NAME(parent_object_id) = 'ReleaseLog'))
	ALTER TABLE ReleaseLog DROP CONSTRAINT PK_ReleaseLog;  

  if(not exists (select * from sys.all_columns where name ='ID' AND OBJECT_NAME(object_id) = 'ReleaseLog'))
  	ALTER TABLE ReleaseLog ADD ID INT IDENTITY(1,1)

  if(not exists (select * from sys.key_constraints where type ='PK' AND OBJECT_NAME(parent_object_id) = 'ReleaseLog'))
	ALTER TABLE ReleaseLog ADD CONSTRAINT PK_ReleaseLog PRIMARY KEY (ID);


  if not exists (select 1 from sys.indexes where name = 'ix_CumulativeExtractionResultsMustBeUnique')
	CREATE UNIQUE NONCLUSTERED INDEX ix_CumulativeExtractionResultsMustBeUnique ON ReleaseLog
	(
		CumulativeExtractionResults_ID ASC
	)


	--No more Software Version / Scalar Function
	
	if exists (select 1 from sys.default_constraints where name ='DF_CumulativeExtractionResults_SoftwareVersion')
begin
Alter TABLE CumulativeExtractionResults				  DROP CONSTRAINT DF_CumulativeExtractionResults_SoftwareVersion
Alter TABLE CumulativeExtractionResults				  DROP COLUMN SoftwareVersion
end

if exists (select 1 from sys.default_constraints where name ='DF_DataUser_SoftwareVersion')
begin
Alter TABLE DataUser								  DROP CONSTRAINT DF_DataUser_SoftwareVersion
Alter TABLE DataUser								  DROP COLUMN SoftwareVersion
end

if exists (select 1 from sys.default_constraints where name ='DF_DeployedExtractionFilter_SoftwareVersion')
begin
Alter TABLE DeployedExtractionFilter				  DROP CONSTRAINT DF_DeployedExtractionFilter_SoftwareVersion
Alter TABLE DeployedExtractionFilter				  DROP COLUMN SoftwareVersion
end

if exists (select 1 from sys.default_constraints where name ='DF_DeployedExtractionFilterParameter_SoftwareVersion')
begin
Alter TABLE DeployedExtractionFilterParameter		  DROP CONSTRAINT DF_DeployedExtractionFilterParameter_SoftwareVersion
Alter TABLE DeployedExtractionFilterParameter		  DROP COLUMN SoftwareVersion
end

if exists (select 1 from sys.default_constraints where name ='DF_ExternalCohortTable_SoftwareVersion')
begin
Alter TABLE ExternalCohortTable						  DROP CONSTRAINT DF_ExternalCohortTable_SoftwareVersion
Alter TABLE ExternalCohortTable						  DROP COLUMN SoftwareVersion
end

if exists (select 1 from sys.default_constraints where name ='DF_ExtractableCohort_SoftwareVersion')
begin
Alter TABLE ExtractableCohort						  DROP CONSTRAINT DF_ExtractableCohort_SoftwareVersion
Alter TABLE ExtractableCohort						  DROP COLUMN SoftwareVersion
end

if exists (select 1 from sys.default_constraints where name ='DF_ExtractableColumn_SoftwareVersion')
begin
Alter TABLE ExtractableColumn						  DROP CONSTRAINT DF_ExtractableColumn_SoftwareVersion
Alter TABLE ExtractableColumn						  DROP COLUMN SoftwareVersion
end

if exists (select 1 from sys.default_constraints where name ='DF_ExtractableDataSet_SoftwareVersion')
begin
Alter TABLE ExtractableDataSet						  DROP CONSTRAINT DF_ExtractableDataSet_SoftwareVersion
Alter TABLE ExtractableDataSet						  DROP COLUMN SoftwareVersion
end

if exists (select 1 from sys.default_constraints where name ='DF_ExtractionConfiguration_SoftwareVersion')
begin
Alter TABLE ExtractionConfiguration					  DROP CONSTRAINT DF_ExtractionConfiguration_SoftwareVersion
Alter TABLE ExtractionConfiguration					  DROP COLUMN SoftwareVersion
end

if exists (select 1 from sys.default_constraints where name ='DF_GlobalExtractionFilterParameter_SoftwareVersion')
begin
Alter TABLE GlobalExtractionFilterParameter			  DROP CONSTRAINT DF_GlobalExtractionFilterParameter_SoftwareVersion
Alter TABLE GlobalExtractionFilterParameter			  DROP COLUMN SoftwareVersion
end

if exists (select 1 from sys.default_constraints where name ='DF_Project_SoftwareVersion')
begin
Alter TABLE Project									  DROP CONSTRAINT DF_Project_SoftwareVersion
Alter TABLE Project									  DROP COLUMN SoftwareVersion
end

if exists (select 1 from sys.default_constraints where name ='DF_ReleaseLog_SoftwareVersion')
begin
Alter TABLE ReleaseLog								  DROP CONSTRAINT DF_ReleaseLog_SoftwareVersion
Alter TABLE ReleaseLog								  DROP COLUMN SoftwareVersion
end

if exists (select OBJECT_NAME(object_id),* from sys.sql_modules  where OBJECT_NAME(object_id) ='GetSoftwareVersion')
	DROP FUNCTION dbo.GetSoftwareVersion