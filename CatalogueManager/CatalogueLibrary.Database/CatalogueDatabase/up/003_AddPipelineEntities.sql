--Version:1.1.0.0
--Description:Adds support for customising DataFlows operations between databases e.g. (anonymisation, IDataFlowComponents).  This is different from ProcessTasks which occur either on specific databases (Adjust RAW / Adjust STAGING) or otherwise within the context of a data load.  Pipelines are reusable anywhere that data flows from A to B and can be transformed as a C# DataTable object in memory.  

if not exists (select 1 from sys.tables where name = 'Pipeline')
begin

CREATE TABLE [dbo].[Pipeline](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](500) NOT NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Pipeline] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


CREATE TABLE [dbo].[PipelineComponent](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Order] [int] NOT NULL,
	[Pipeline_ID] [int] NOT NULL,
	[Name] [varchar](500) NOT NULL,
	[Class] [varchar](500) NOT NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_PipelineComponent] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[PipelineComponentArgument](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PipelineComponent_ID] [int] NOT NULL,
	[Name] [varchar](500) NOT NULL,
	[Value] [varchar](max) NULL,
	[Type] [varchar](500) NOT NULL,
	[Description] [varchar](1000) NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_PipelineComponentArgument] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
end
GO

if not exists (select 1 from sys.objects where name = 'FK_PipelineComponent_Pipeline')
begin

ALTER TABLE [dbo].[PipelineComponent]  WITH CHECK ADD  CONSTRAINT [FK_PipelineComponent_Pipeline] FOREIGN KEY([Pipeline_ID])
REFERENCES [dbo].[Pipeline] ([ID])
ON DELETE CASCADE

ALTER TABLE [dbo].[PipelineComponent] CHECK CONSTRAINT [FK_PipelineComponent_Pipeline]

ALTER TABLE [dbo].[PipelineComponentArgument]  WITH CHECK ADD  CONSTRAINT [FK_PipelineComponentArgument_PipelineComponent] FOREIGN KEY([PipelineComponent_ID])
REFERENCES [dbo].[PipelineComponent] ([ID])
ON DELETE CASCADE

ALTER TABLE [dbo].[PipelineComponentArgument] CHECK CONSTRAINT [FK_PipelineComponentArgument_PipelineComponent]

ALTER TABLE [dbo].[Pipeline] ADD  CONSTRAINT [DF_Pipeline_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
ALTER TABLE [dbo].[PipelineComponent] ADD  CONSTRAINT [DF_PipelineComponent_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
ALTER TABLE [dbo].[PipelineComponentArgument] ADD  CONSTRAINT [DF_PipelineComponentArgument_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]

end
