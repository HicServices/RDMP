--Version:1.13.0.0
--Description:Adds ChunkPeriod, Pipeline_ID and PipelineContextField to Cache Progress, adds missing 'SoftwareVersion' default and LoadSchedule FK

-- Add new columns
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='CacheProgress' AND COLUMN_NAME='ChunkPeriod')
BEGIN
	ALTER TABLE CacheProgress ADD ChunkPeriod time(0) NOT NULL CONSTRAINT [DF_CacheProgress_ChunkPeriod]  DEFAULT ('01:00:00')
END

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='CacheProgress' AND COLUMN_NAME='Pipeline_ID')
BEGIN
	ALTER TABLE CacheProgress ADD Pipeline_ID int NULL
END

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='CacheProgress' AND COLUMN_NAME='PipelineContextField')
BEGIN
	ALTER TABLE CacheProgress ADD PipelineContextField varchar(3000) NULL
END

-- Default for SoftwareVersion
IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_CacheProgress_SoftwareVersion')
BEGIN
	ALTER TABLE [dbo].[CacheProgress] ADD  CONSTRAINT [DF_CacheProgress_SoftwareVersion] DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
END

-- Add FK for LoadSchedule in CacheProgress table
IF NOT EXISTS (SELECT * FROM sys.objects o WHERE o.object_id = object_id(N'[dbo].[FK_CacheProgress_LoadSchedule]') AND OBJECTPROPERTY(o.object_id, N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE [dbo].[CacheProgress]  WITH CHECK ADD CONSTRAINT [FK_CacheProgress_LoadSchedule] FOREIGN KEY([LoadSchedule_ID])
	REFERENCES [dbo].[LoadSchedule] ([ID])

	ALTER TABLE [dbo].[CacheProgress] CHECK CONSTRAINT [FK_CacheProgress_LoadSchedule]
END

