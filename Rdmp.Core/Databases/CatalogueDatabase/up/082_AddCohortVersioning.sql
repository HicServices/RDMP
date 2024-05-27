--Version: 8.2.0
--Description: Adds cohort version numbers
if not exists(select 1 from sys.columns where name = 'Version' and OBJECT_NAME(object_id) = 'CohortIdentificationConfiguration')
BEGIN
  alter table [dbo].[CohortIdentificationConfiguration]
  add Version [int]
  END