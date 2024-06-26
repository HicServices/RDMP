--Version:1.40.0.1
--Description: Added field that documents which database assembly created a given ExternalDatabaseServer.  Also support for Patient Index Sets in Cohort Manager

  if not exists (select * from sys.columns where name ='CreatedByAssembly')
  begin

   alter table [ExternalDatabaseServer] add CreatedByAssembly varchar(500) null

  end


if not exists ( select * from sys.tables where name = 'JoinableCohortAggregateConfiguration') 
  begin

	CREATE TABLE [dbo].[JoinableCohortAggregateConfiguration](
		[ID] [int] IDENTITY(1,1) NOT NULL,
		[CohortIdentificationConfiguration_ID] [int] NOT NULL,
		[AggregateConfiguration_ID] [int] NOT NULL,
	 CONSTRAINT [PK_JoinableCohortAggregateConfiguration] PRIMARY KEY CLUSTERED 
	(
		[ID] ASC
	),
	CONSTRAINT [FK_JoinableCohortAggregateConfiguration_AggregateConfiguration] FOREIGN KEY([AggregateConfiguration_ID]) REFERENCES [dbo].[AggregateConfiguration] ([ID]),
	CONSTRAINT [FK_JoinableCohortAggregateConfiguration_CohortIdentificationConfiguration] FOREIGN KEY([CohortIdentificationConfiguration_ID]) REFERENCES [dbo].[CohortIdentificationConfiguration] ([ID]) ON DELETE CASCADE
	)

end


if not exists (select * from sys.tables where name = 'JoinableCohortAggregateConfigurationUse')
begin

CREATE TABLE [dbo].[JoinableCohortAggregateConfigurationUse](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[JoinableCohortAggregateConfiguration_ID] [int] NOT NULL,
	[AggregateConfiguration_ID] [int] NOT NULL,
	[JoinType] [varchar](100) NOT NULL,
 CONSTRAINT [PK_JoinableCohortAggregateConfigurationUse] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
),
CONSTRAINT [FK_JoinableCohortAggregateConfigurationUse_AggregateConfiguration] FOREIGN KEY([AggregateConfiguration_ID]) REFERENCES [dbo].[AggregateConfiguration] ([ID]) ON DELETE CASCADE,
CONSTRAINT [FK_JoinableCohortAggregateConfigurationUse_JoinableCohortAggregateConfiguration] FOREIGN KEY([JoinableCohortAggregateConfiguration_ID]) REFERENCES [dbo].[JoinableCohortAggregateConfiguration] ([ID]) ON DELETE CASCADE

)
end

if not exists (select * from sys.indexes where name = 'ix_eachAggregateCanOnlyBeJoinableOnOneProject')
begin
CREATE UNIQUE NONCLUSTERED INDEX ix_eachAggregateCanOnlyBeJoinableOnOneProject ON [dbo].[JoinableCohortAggregateConfiguration]
(
	[AggregateConfiguration_ID] ASC
)

CREATE UNIQUE NONCLUSTERED INDEX [ix_eachAggregateCanOnlyHaveOneJoinable] ON [dbo].[JoinableCohortAggregateConfigurationUse]
(
	[AggregateConfiguration_ID] ASC
)
end

