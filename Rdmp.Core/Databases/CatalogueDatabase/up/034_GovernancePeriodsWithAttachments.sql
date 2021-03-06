--Version:1.28.0.0
--Description: Adds the ability to have 0 or more GovernancePeriod objects associated with each Catalogue e.g. you might have 'ISD general approvals 2001-2015' which is associated with datasets SMR01, SMR02 and SMR05.
if not exists (select 1 from sys.tables where name = 'GovernanceDocument')
begin

CREATE TABLE [dbo].[GovernanceDocument](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[GovernancePeriod_ID] [int] NOT NULL,
	[URL] [varchar](500) NOT NULL,
	[Description] [varchar](max) NULL,
	[Name] [varchar](500) NOT NULL,
 CONSTRAINT [PK_GovernanceDocument] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[GovernancePeriod](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](500) NOT NULL,
	[StartDate] [date] NOT NULL,
	[EndDate] [date] NULL,
	[Description] [varchar](max) NULL,
	[Ticket] [varchar](20) NULL,
 CONSTRAINT [PK_GovernancePeriod] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

CREATE TABLE [dbo].[GovernancePeriod_Catalogue](
	[Catalogue_ID] [int] NOT NULL,
	[GovernancePeriod_ID] [int] NOT NULL,
 CONSTRAINT [PK_GovernancePeriod_Catalogue] PRIMARY KEY CLUSTERED 
(
	[Catalogue_ID] ASC,
	[GovernancePeriod_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

end

if not exists (select 1 from sys.foreign_keys where name = 'FK_GovernanceDocument_GovernancePeriod')
begin

CREATE UNIQUE NONCLUSTERED INDEX [idxGovernancePeriodNameMustBeUnique] ON [dbo].[GovernancePeriod]
(
	[Name] ASC
)

ALTER TABLE [dbo].[GovernanceDocument]  WITH CHECK ADD  CONSTRAINT [FK_GovernanceDocument_GovernancePeriod] FOREIGN KEY([GovernancePeriod_ID])
REFERENCES [dbo].[GovernancePeriod] ([ID])

ALTER TABLE [dbo].[GovernancePeriod_Catalogue]  WITH CHECK ADD  CONSTRAINT [FK_GovernancePeriod_Catalogue_Catalogue] FOREIGN KEY([Catalogue_ID])
REFERENCES [dbo].[Catalogue] ([ID])

ALTER TABLE [dbo].[GovernancePeriod_Catalogue]  WITH CHECK ADD  CONSTRAINT [FK_GovernancePeriod_Catalogue_GovernancePeriod] FOREIGN KEY([GovernancePeriod_ID])
REFERENCES [dbo].[GovernancePeriod] ([ID])

end