--Version: 8.2.0
--Description: Adds linking table to allow for multiple load metadatas per catalogue
 if not exists (select 1 from sys.columns where name = 'GroupBy' and OBJECT_NAME(object_id) = 'AggregateDimension')
BEGIN
ALTER TABLE [dbo].[AggregateDimension] ADD GroupBy int DEFAULT 1
END

if exists( select 1 from [dbo].[AggregateDimension] where GroupBy is NULL)
BEGIN
UPDATE [dbo].[AggregateDimension] Set GroupBy = 1 WHERE GroupBy is NULL
END

 if not exists (select 1 from sys.columns where name = 'GroupBy' and OBJECT_NAME(object_id) = 'ExtractionInformation')
BEGIN
ALTER TABLE [dbo].[ExtractionInformation] ADD GroupBy int DEFAULT 1
END


if exists( select 1 from [dbo].[ExtractionInformation] where GroupBy is NULL)
BEGIN
UPDATE [dbo].[ExtractionInformation] Set GroupBy = 1 WHERE GroupBy is NULL
END

