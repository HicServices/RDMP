--Version: 9.1.0
--Description: Add new metadata fields for catalogues
if not exists (select 1 from sys.columns where name = 'IsTemplate' and OBJECT_NAME(object_id) = 'CohortIdentificationConfiguration')
BEGIN
ALTER TABLE [dbo].[CohortIdentificationConfiguration]
ADD
[IsTemplate] [bit] NOT NULL DEFAULT 0
END
GO


