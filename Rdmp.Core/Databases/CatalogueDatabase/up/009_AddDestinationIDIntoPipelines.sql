--Version:1.7.0.0
--Description:Adds a new field DestinationPipelineComponent_ID that allows Pipelines to have a fixed destination component with its own configuration
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Pipeline' AND COLUMN_NAME='DestinationPipelineComponent_ID')
BEGIN
	ALTER TABLE Pipeline Add DestinationPipelineComponent_ID int null
END
GO

IF NOT EXISTS (SELECT 1 from sys.foreign_keys where name = 'FK_Pipeline_PipelineComponent')
BEGIN
ALTER TABLE [dbo].[Pipeline]  WITH CHECK ADD  CONSTRAINT [FK_Pipeline_PipelineComponent] FOREIGN KEY([DestinationPipelineComponent_ID])
REFERENCES [dbo].[PipelineComponent] ([ID])
END