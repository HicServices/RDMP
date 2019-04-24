--Version:1.7.0.0
--Description: Changes [FK_ExtractionConfiguration_Cohort] from Cascade set null to No Action to prevent accidentally deleting cohorts which are used in configurations
if exists (select * from sys.foreign_keys where name='FK_ExtractionConfiguration_Cohort' and delete_referential_action = 2 /*SET_NULL*/)
begin

--to start with we know it is delete_referential_action = 2 so we want it to be 0 (the default) so drop it
ALTER TABLE [ExtractionConfiguration] drop constraint [FK_ExtractionConfiguration_Cohort]

--and recreate it without any CASCADE / SET_NULL
ALTER TABLE [ExtractionConfiguration]  WITH CHECK ADD CONSTRAINT [FK_ExtractionConfiguration_Cohort] FOREIGN KEY([Cohort_ID])
REFERENCES [ExtractableCohort] ([ID])

end


   