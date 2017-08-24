--Version:2.2.0.1
--Description: Adds the ability to store a default Pipeline to execute for an ExtractionConfiguration
if not exists (select 1 from sys.all_columns where name ='DefaultPipeline_ID')
	alter table ExtractionConfiguration add DefaultPipeline_ID int null

if not exists (select 1 from sys.all_columns where name ='CohortIdentificationConfiguration_ID')
	alter table ExtractionConfiguration add CohortIdentificationConfiguration_ID int null

if not exists (select 1 from sys.all_columns where name ='CohortRefreshPipeline_ID')
	alter table ExtractionConfiguration add CohortRefreshPipeline_ID int null