--Version:1.8.0.0
--Description:Adds a new field SourcePipelineComponent_ID that allows Pipelines to have a fixed source component with its own configuration
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Pipeline' AND COLUMN_NAME='SourcePipelineComponent_ID')
BEGIN
	ALTER TABLE Pipeline Add SourcePipelineComponent_ID int null
END
GO

IF NOT EXISTS (SELECT 1 from sys.foreign_keys where name = 'FK_Pipeline_SourcePipelineComponent_ID_PipelineComponent')
BEGIN
ALTER TABLE [dbo].[Pipeline]  WITH CHECK ADD  CONSTRAINT FK_Pipeline_SourcePipelineComponent_ID_PipelineComponent FOREIGN KEY(SourcePipelineComponent_ID)
REFERENCES [dbo].[PipelineComponent] ([ID])
END