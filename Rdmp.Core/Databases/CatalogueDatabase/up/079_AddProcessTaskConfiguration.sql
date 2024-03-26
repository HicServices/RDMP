--Version:8.1.5
--Description: Adds serialisable configuration to the process task
 GO
if not exists (select 1 from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME='ProcessTask' and COLUMN_NAME='SerialisableConfiguration')
BEGIN       
ALTER TABLE ProcessTask ADD SerialisableConfiguration [varchar](max);
END
