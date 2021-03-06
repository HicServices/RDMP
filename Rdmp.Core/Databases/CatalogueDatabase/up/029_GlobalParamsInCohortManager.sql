--Version:1.23.0.0
--Description: Adds the ability to define global parameters which allows overriding of values at a configuration level even though the params are used in multiple filters throughout the config.  E.g. if you reference project start date it might be useful to be able to edit that in one place only
if not exists (select 1 from sys.tables where name = 'CohortAggregateGlobalFilterParameter')
begin
CREATE TABLE [dbo].[CohortAggregateGlobalFilterParameter](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	CohortIdentificationConfiguration_ID [int] NULL,
	[Value] [varchar](500) NULL,
	[ParameterSQL] [varchar](500) NULL,
	[Comment] [varchar](500) NULL,
 CONSTRAINT [PK_GlobalExtractionFilterParameter] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

end

if not exists (select 1 from sys.foreign_keys where name = 'FK_CohortAggregateGlobalFilterParameter_CohortIdentificationConfiguration')
begin

ALTER TABLE [dbo].[CohortAggregateGlobalFilterParameter]  WITH CHECK ADD CONSTRAINT [FK_CohortAggregateGlobalFilterParameter_CohortIdentificationConfiguration] FOREIGN KEY([CohortIdentificationConfiguration_ID])
REFERENCES [dbo].[CohortIdentificationConfiguration] ([ID])
ON DELETE CASCADE

end