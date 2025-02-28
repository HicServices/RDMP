--Version: 8.4.4
--Description: Add configuration for regex redaction


if not exists (select 1 from sys.tables where name = 'PrimaryContraint')
BEGIN
CREATE TABLE [dbo].[PrimaryContraint](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ColumnInfo_ID] [int] NOT NULL UNIQUE,
	[Constraint] [int] NOT NULL,
	[Result] [int] NOT NULL,
CONSTRAINT [PK_ColumnInfoPrimaryConstraint] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO

if not exists (select 1 from sys.tables where name = 'CatalogueValidation')
BEGIN
CREATE TABLE [dbo].[CatalogueValidation](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Catalogue_ID] [int] NOT NULL,
	[Date] [datetime] NOT NULL,
	[TimeColumn_ID] [int] NOT NULL,
	[PivotColumn_ID] [int] NOT NULL,
CONSTRAINT [PK_CatalogueValidation] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO


if not exists (select 1 from sys.tables where name = 'CatalogueValidationResult')
BEGIN
CREATE TABLE [dbo].[CatalogueValidationResult](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[CatalogueValidation_ID] [int] NOT NULL,
	[Date] [datetime] NOT NULL,
	[Correct] [int] NOT NULL,
	[Wrong] [int] NOT NULL,
	[Missing] [int] NOT NULL,
	[Invalid] [int] NOT NULL,
	[PivotCategory] [varchar](255) NOT NULL,
CONSTRAINT [PK_CatalogueValidationResult] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO

if not exists (select 1 from sys.tables where name = 'SecondaryConstraint')
BEGIN
CREATE TABLE [dbo].[SecondaryConstraint](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ColumnInfo_ID] [int] NOT NULL,
	[Constraint] [int] NOT NULL,
	[Consequence] [int] NOT NULL,
CONSTRAINT [PK_SecondaryConstraint] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO

if not exists (select 1 from sys.tables where name = 'SecondaryConstraintArgument')
BEGIN
CREATE TABLE [dbo].[SecondaryConstraintArgument](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SecondaryConstraint_ID] [int] NOT NULL,
	[Key] [nvarchar](max) NOT NULL,
	[Value] [nvarchar](max) NOT NULL,
    FOREIGN KEY ([SecondaryConstraint_ID]) REFERENCES [dbo].[SecondaryConstraint](ID) ON DELETE CASCADE,
CONSTRAINT [PK_SecondaryConstraintArgument] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO

if not exists (select 1 from sys.tables where name = 'UserDefinedChart')
BEGIN
CREATE TABLE [dbo].[UserDefinedChart](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Catalogue_ID] [int] NOT NULL,
	[ChartType] [int] NOT NULL,
	[QueryString] [nvarchar](max) NOT NULL,
	[Title] [nvarchar](256),
	[SeriesName] [nvarchar](256),
CONSTRAINT [PK_UserDefinedChart] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO

if not exists (select 1 from sys.tables where name = 'UserDefinedChartResult')
BEGIN
CREATE TABLE [dbo].[UserDefinedChartResult](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserDefinedChart_ID] [int] NOT NULL,
	[X] [nvarchar](max) NOT NULL,
	[Y] [nvarchar](max) NOT NULL,
	FOREIGN KEY ([UserDefinedChart_ID]) REFERENCES [dbo].[UserDefinedChart](ID) ON DELETE CASCADE,
CONSTRAINT [PK_UserDefinedChartResult] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
