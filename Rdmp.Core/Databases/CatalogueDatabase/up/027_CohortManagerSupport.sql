--Version:1.21.0.0
--Description: Adds CohortManager related tables including the new Folder property for Catalogues

----------------------------------------------------------------------------------------------------
--Cohort Identification tables
----------------------------------------------------------------------------------------------------
--Tables
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CohortIdentificationConfiguration')
BEGIN
CREATE TABLE [dbo].[CohortAggregateContainer](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Operation] [varchar](20) NOT NULL,
 CONSTRAINT [PK_CohortAggregateContainer] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[CohortAggregateSubContainer](
	[CohortAggregateContainer_ParentID] [int] NOT NULL,
	[CohortAggregateContainer_ChildID] [int] NOT NULL
	CONSTRAINT [PK_CohortAggregateSubContainer] PRIMARY KEY CLUSTERED 
	(
		[CohortAggregateContainer_ParentID] ASC,
		[CohortAggregateContainer_ChildID] ASC
	)
) ON [PRIMARY]


CREATE TABLE [dbo].[CohortIdentificationConfiguration](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](500) NOT NULL,
	[Ticket] [varchar](20) NULL,
	[Description] [varchar](5000) NULL,
	[RootCohortAggregateContainer_ID] [int] NULL,
	[Version] [int] NULL
 CONSTRAINT [PK_CohortIdentificationConfiguration] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[CohortAggregateContainer_AggregateConfiguration](
	[CohortAggregateContainer_ID] [int] NOT NULL,
	[AggregateConfiguration_ID] [int] NOT NULL,
	[Order] int not null
	 CONSTRAINT [PK_CohortAggregateContainer_AggregateConfiguration] PRIMARY KEY CLUSTERED 
(
	[AggregateConfiguration_ID] ASC
)
) ON [PRIMARY]
END
GO
--Relationships
if not exists (select 1 from sys.foreign_keys where name = 'FK_CohortAggregateSubContainer_CohortAggregateContainer_ChildID')
BEGIN
ALTER TABLE [dbo].[CohortAggregateSubContainer]  WITH CHECK ADD  CONSTRAINT [FK_CohortAggregateSubContainer_CohortAggregateContainer_ChildID] FOREIGN KEY([CohortAggregateContainer_ChildID])
REFERENCES [dbo].[CohortAggregateContainer] ([ID])ON DELETE CASCADE --cascade deletes from child into subcontainer relationships

ALTER TABLE [dbo].[CohortAggregateSubContainer]  WITH CHECK ADD  CONSTRAINT [FK_CohortAggregateSubContainer_CohortAggregateContainer_ParentID] FOREIGN KEY([CohortAggregateContainer_ParentID])
REFERENCES [dbo].[CohortAggregateContainer] ([ID]) 

ALTER TABLE [dbo].[CohortIdentificationConfiguration]  WITH CHECK ADD  CONSTRAINT [FK_CohortAggregateContainer_CohortAggregateContainer] FOREIGN KEY([RootCohortAggregateContainer_ID])
REFERENCES [dbo].[CohortAggregateContainer] ([ID]) ON DELETE CASCADE --Cascade deleting configuration to delete root container

ALTER TABLE [dbo].[CohortAggregateContainer_AggregateConfiguration]  WITH CHECK ADD  CONSTRAINT [FK_CohortAggregateContainer_AggregateConfiguration_AggregateConfiguration] FOREIGN KEY([AggregateConfiguration_ID])
REFERENCES [dbo].[AggregateConfiguration] ([ID])
 
--link table - specifies that a given aggregate is used in cohort identification
ALTER TABLE [dbo].[CohortAggregateContainer_AggregateConfiguration]  WITH CHECK ADD  CONSTRAINT [FK_CohortAggregateContainer_AggregateConfiguration_CohortAggregateContainer] FOREIGN KEY([CohortAggregateContainer_ID])
REFERENCES [dbo].[CohortAggregateContainer] ([ID]) ON DELETE CASCADE --Cascade deleting contents of container when container is deleted

END


--------------------------------------------------------------------------------------------------------
--Catalogue Folder Support
--------------------------------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.columns where name = 'Folder' and OBJECT_NAME(object_id)='Catalogue')
BEGIN
alter table Catalogue add Folder varchar(1000) null
END
GO

IF (SELECT columnproperty(object_id('Catalogue'), 'Folder', 'AllowsNull')) = 1
BEGIN
	UPDATE Catalogue set Folder = '\'
	alter table Catalogue alter column Folder varchar(1000) not null
	alter table Catalogue add constraint DF_Folder default '\' for Folder

END
GO