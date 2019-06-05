--Version:2.4.0.1
--Description: Adds the ability to associate a Cohort Identification Configuration with a Project
if not exists( select 1 from sys.tables where name ='ProjectCohortIdentificationConfigurationAssociation')
begin

	CREATE TABLE [dbo].[ProjectCohortIdentificationConfigurationAssociation](
		[ID] [int] IDENTITY(1,1) NOT NULL,
		[Project_ID] [int] NOT NULL,
		[CohortIdentificationConfiguration_ID] [int] NOT NULL,
		CONSTRAINT [FK_ProjectCohortIdentificationConfigurationAssociation_Project] FOREIGN KEY([Project_ID]) REFERENCES [dbo].[Project] ([ID]) ON DELETE CASCADE,
	 CONSTRAINT [PK_ProjectCohortIdentificationConfigurationAssociation] PRIMARY KEY CLUSTERED 
	(
		[ID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
end

if not exists (select 1 from sys.indexes where name ='ix_NoCicProjectDuplicates')
begin

	CREATE UNIQUE NONCLUSTERED INDEX [ix_NoCicProjectDuplicates] ON [dbo].[ProjectCohortIdentificationConfigurationAssociation]
	(
		[Project_ID] ASC,
		[CohortIdentificationConfiguration_ID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	
end