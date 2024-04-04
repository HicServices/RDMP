--Version:1.1.0.0
--Description:Initial Creation Script
--So we can use it in a DEFAULT constraint
CREATE FUNCTION [dbo].[GetDefaultExternalServerIDFor]
(
	-- Add the parameters for the function here
	@Default varchar(50)
)
RETURNS int
AS
BEGIN
	
	RETURN (SELECT ExternalDatabaseServer_ID from ServerDefaults where DefaultType = @Default)
END

GO
/****** Object:  UserDefinedFunction [dbo].[GetSoftwareVersion]    Script Date: 09/06/2015 14:16:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

--todo dbo should be RoundhousE
CREATE FUNCTION [dbo].[GetSoftwareVersion]()
RETURNS nvarchar(50)
AS
BEGIN
	-- Return the result of the function
	RETURN (SELECT top 1 version from RoundhousE.Version order by version desc)
END

GO
/****** Object:  Table [dbo].[ANOTable]    Script Date: 09/06/2015 14:16:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ANOTable](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](500) NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
	[Server_ID] [int] NOT NULL,
 CONSTRAINT [PK_ANOTable] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[AggregateConfiguration]    Script Date: 09/06/2015 14:16:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[AggregateConfiguration](
	[Catalogue_ID] [int] NOT NULL,
	[Name] [varchar](500) NULL,
	[Description] [varchar](5000) NULL,
	[dtCreated] [datetime] NOT NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[RootFilterContainer_ID] [int] NULL,
	[CountSQL] [varchar](1000) NOT NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
	[PivotOnDimensionID] [int] NULL,
	[IsExtractable] [bit] NOT NULL,
 CONSTRAINT [PK_AggregateConfiguration] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[AggregateContinuousDateAxis]    Script Date: 09/06/2015 14:16:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[AggregateContinuousDateAxis](
	[AggregateDimension_ID] [int] NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[StartDate] [varchar](500) NULL,
	[EndDate] [varchar](500) NULL,
	[AxisIncrement] [int] NOT NULL,
 CONSTRAINT [PK_AggregateContinuousDateAxis] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[AggregateDimension]    Script Date: 09/06/2015 14:16:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[AggregateDimension](
	[AggregateConfiguration_ID] [int] NOT NULL,
	[ExtractionInformation_ID] [int] NOT NULL,
	[SelectSQL] [varchar](max) NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Alias] [varchar](100) NULL,
	[Order] [int] NOT NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_AggregateDimension] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[AggregateFilter]    Script Date: 09/06/2015 14:16:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[AggregateFilter](
	[FilterContainer_ID] [int] NULL,
	[WhereSQL] [varchar](max) NULL,
	[Description] [varchar](500) NULL,
	[Name] [varchar](100) NOT NULL,
	[IsMandatory] [bit] NOT NULL,
	[AssociatedColumnInfo_ID] [int] NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_AggregateFilter] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[AggregateFilterContainer]    Script Date: 09/06/2015 14:16:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[AggregateFilterContainer](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Operation] [varchar](10) NOT NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_AggregateFilterContainer] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[AggregateFilterParameter]    Script Date: 09/06/2015 14:16:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[AggregateFilterParameter](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[AggregateFilter_ID] [int] NOT NULL,
	[ParameterSQL] [varchar](500) NULL,
	[Value] [varchar](500) NULL,
	[Comment] [varchar](500) NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_AggregateFilterParameter] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[AggregateFilterSubContainer]    Script Date: 09/06/2015 14:16:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AggregateFilterSubContainer](
	[AggregateFilterContainer_ParentID] [int] NULL,
	[AggregateFilterContainer_ChildID] [int] NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[AggregateForcedJoin]    Script Date: 09/06/2015 14:16:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AggregateForcedJoin](
	[AggregateConfiguration_ID] [int] NOT NULL,
	[TableInfo_ID] [int] NOT NULL,
 CONSTRAINT [PK_AggregateForcedJoin] PRIMARY KEY CLUSTERED 
(
	[AggregateConfiguration_ID] ASC,
	[TableInfo_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Catalogue]    Script Date: 09/06/2015 14:16:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Catalogue](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Acronym] [varchar](50) NULL,
	[Name] [varchar](1000) NULL,
	[Description] [text] NULL,
	[Detail_Page_URL] [varchar](150) NULL,
	[Type] [varchar](50) NULL,
	[Periodicity] [varchar](50) NULL,
	[Geographical_coverage] [varchar](150) NULL,
	[Background_summary] [text] NULL,
	[Search_keywords] [varchar](150) NULL,
	[Update_freq] [varchar](50) NULL,
	[Update_sched] [varchar](50) NULL,
	[Time_coverage] [varchar](50) NULL,
	[Last_revision_date] [date] NULL,
	[Contact_details] [varchar](50) NULL,
	[Resource_owner] [varchar](50) NULL,
	[Attribution_citation] [varchar](500) NULL,
	[Access_options] [varchar](150) NULL,
	[API_access_URL] [varchar](150) NULL,
	[Browse_URL] [varchar](150) NULL,
	[Bulk_Download_URL] [varchar](150) NULL,
	[Query_tool_URL] [varchar](150) NULL,
	[Source_URL] [varchar](150) NULL,
	[Granularity] [varchar](50) NULL,
	[Country_of_origin] [varchar](150) NULL,
	[Data_standards] [varchar](500) NULL,
	[Administrative_contact_name] [varchar](50) NULL,
	[Administrative_contact_email] [varchar](255) NULL,
	[Administrative_contact_telephone] [varchar](50) NULL,
	[Administrative_contact_address] [varchar](500) NULL,
	[Explicit_consent] [bit] NULL,
	[Ethics_approver] [varchar](255) NULL,
	[Source_of_data_collection] [varchar](500) NULL,
	[SubjectNumbers] [varchar](50) NULL,
	[TimeCoverage_ExtractionInformation_ID] [int] NULL,
	[ValidatorXML] [varchar](max) NULL,
	[LoggingDataTask] [varchar](100) NULL,
	[JIRATicket] [varchar](20) NULL,
	[DatasetStartDate] [datetime] NULL,
	[IsDeprecated] [bit] NOT NULL,
	[IsInternalDataset] [bit] NOT NULL,
	[LiveLoggingServer_ID] [int] NULL,
	[TestLoggingServer_ID] [int] NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Data_Catalogue] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[CatalogueItem]    Script Date: 09/06/2015 14:16:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[CatalogueItem](
	[Catalogue_ID] [int] NOT NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](256) NOT NULL,
	[Statistical_cons] [text] NULL,
	[Research_relevance] [text] NULL,
	[Description] [text] NULL,
	[Topic] [varchar](50) NULL,
	[Periodicity] [varchar](50) NULL,
	[Agg_method] [varchar](255) NULL,
	[Limitations] [text] NULL,
	[Comments] [text] NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Catalogue_Items] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[CatalogueItemIssue]    Script Date: 09/06/2015 14:16:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[CatalogueItemIssue](
	[CatalogueItem_ID] [int] NOT NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](500) NULL,
	[Description] [varchar](max) NULL,
	[SQL] [varchar](max) NULL,
	[JIRATicket] [varchar](10) NULL,
	[Status] [varchar](20) NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[UserWhoCreated] [varchar](500) NOT NULL,
	[DateOfLastStatusChange] [datetime] NULL,
	[UserWhoLastChangedStatus] [varchar](500) NULL,
	[Severity] [varchar](100) NOT NULL,
	[ReportedBy_ID] [int] NULL,
	[ReportedOnDate] [datetime] NULL,
	[Owner_ID] [int] NULL,
	[Action] [varchar](max) NULL,
	[NotesToResearcher] [varchar](max) NULL,
	[PathToExcelSheetWithAdditionalInformation] [varchar](1000) NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_CatalogueItemIssue] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ColumnInfo]    Script Date: 09/06/2015 14:16:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ColumnInfo](
	[TableInfo_ID] [int] NOT NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Data_type] [varchar](50) NULL,
	[Format] [varchar](50) NULL,
	[Digitisation_specs] [varchar](255) NULL,
	[Name] [varchar](1000) NULL,
	[Source] [varchar](50) NULL,
	[Description] [varchar](1000) NULL,
	[Status] [varchar](10) NULL,
	[RegexPattern] [varchar](255) NULL,
	[ValidationRules] [varchar](5000) NULL,
	[IsPrimaryKey] [bit] NOT NULL,
	[ANOTable_ID] [int] NULL,
	[DuplicateRecordResolutionOrder] [int] NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
	[DuplicateRecordResolutionIsAscending] [bit] NOT NULL,
 CONSTRAINT [PK_Table_Items] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ColumnInfo_CatalogueItem]    Script Date: 09/06/2015 14:16:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ColumnInfo_CatalogueItem](
	[ColumnInfo_ID] [int] NULL,
	[CatalogueItem_ID] [int] NULL,
	[ExtractionInformation_ID] [int] NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[DLEWindowsServiceException]    Script Date: 09/06/2015 14:16:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[DLEWindowsServiceException](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MachineName] [varchar](500) NOT NULL,
	[Exception] [varchar](max) NOT NULL,
	[EventDate] [datetime] NOT NULL,
	[LoadMetadata_ID] [int] NOT NULL,
	[Explanation] [varchar](max) NULL,
 CONSTRAINT [PK_DLEWindowsServiceExceptions] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[DuplicationResolutionOrder]    Script Date: 09/06/2015 14:16:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DuplicationResolutionOrder](
	[ColumnInfo_ID] [int] NULL,
	[ResolveOrder] [int] NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ExternalDatabaseServer]    Script Date: 09/06/2015 14:16:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ExternalDatabaseServer](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](500) NULL,
	[DatabaseName] [varchar](50) NULL,
	[ServerName] [varchar](50) NULL,
	[Username] [varchar](50) NULL,
	[Password] [varchar](50) NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_ExternalDatabaseServer] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ExtractionFilter]    Script Date: 09/06/2015 14:16:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ExtractionFilter](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ExtractionInformation_ID] [int] NOT NULL,
	[WhereSQL] [varchar](max) NULL,
	[Description] [varchar](500) NULL,
	[Name] [varchar](100) NOT NULL,
	[IsMandatory] [bit] NOT NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_ExtractionFilter] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ExtractionFilterParameter]    Script Date: 09/06/2015 14:16:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ExtractionFilterParameter](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ExtractionFilter_ID] [int] NOT NULL,
	[ParameterSQL] [varchar](500) NULL,
	[Value] [varchar](500) NULL,
	[Comment] [varchar](500) NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_ExtractionFilterParameter] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ExtractionInformation]    Script Date: 09/06/2015 14:16:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ExtractionInformation](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SelectSQL] [varchar](max) NOT NULL,
	[Order] [int] NOT NULL,
	[ExtractionCategory] [varchar](30) NOT NULL,
	[Alias] [varchar](100) NULL,
	[HashOnDataRelease] [bit] NOT NULL,
	[IsExtractionIdentifier] [bit] NOT NULL,
	[IsPrimaryKey] [bit] NOT NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_ExtractionInformation] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[IssueSystemUser]    Script Date: 09/06/2015 14:16:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[IssueSystemUser](
	[Name] [varchar](200) NOT NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[EmailAddress] [varchar](500) NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_IssueSystemUser] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[JoinInfo]    Script Date: 09/06/2015 14:16:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[JoinInfo](
	[ForeignKey_ID] [int] NOT NULL,
	[PrimaryKey_ID] [int] NOT NULL,
	[ExtractionJoinType] [varchar](100) NOT NULL,
	[Collation] [varchar](50) NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_JoinInfo] PRIMARY KEY CLUSTERED 
(
	[ForeignKey_ID] ASC,
	[PrimaryKey_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[LoadMetadata]    Script Date: 09/06/2015 14:16:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[LoadMetadata](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	LocationOfForLoadingDirectory [varchar](3000) NULL,
	LocationOfForArchivingDirectory [varchar](3000) NULL,
	LocationOfExecutablesDirectory [varchar](3000) NULL,
	LocationOfCacheDirectory [varchar](3000) NULL,
	[IncludeDataset] [bit] NOT NULL,
	[UsesStandardisedLoadProcess] [bit] NOT NULL,
	[ScheduleStartDate] [datetime] NULL,
	[SchedulePeriod] [int] NULL,
	[RawDatabaseServer] [varchar](50) NULL,
	[StagingDatabaseServer] [varchar](50) NULL,
	[LiveDatabaseServer] [varchar](50) NULL,
	[AnonymisationEngineClass] [varchar](50) NULL,
	[RawDataSource] [varchar](3000) NULL,
	[Name] [varchar](500) NOT NULL,
	[Description] [varchar](max) NULL,
	[EnableAnonymisation] [bit] NOT NULL,
	[OverrideLoggingServer] [varchar](50) NULL,
	[EnableLookupPopulation] [bit] NOT NULL,
	[EnablePrimaryKeyDuplicationResolution] [bit] NOT NULL,
	[CacheFilenameDateFormat] [varchar](20) NOT NULL,
	[CacheArchiveType] [int] NOT NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_LoadMetadata] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[LoadModuleAssembly]    Script Date: 09/06/2015 14:16:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[LoadModuleAssembly](
	[Name] [varchar](250) NOT NULL,
	[Dll] [varbinary](max) NOT NULL,
	[Description] [varchar](2000) NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Committer] [varchar](2000) NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_LoadModuleAssembly] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[LoadPeriodically]    Script Date: 09/06/2015 14:16:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LoadPeriodically](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[LoadMetadata_ID] [int] NOT NULL,
	[LastLoaded] [date] NULL,
	[DaysToWaitBetweenLoads] [int] NOT NULL,
	[OnSuccessLaunchLoadMetadata_ID] [int] NULL,
 CONSTRAINT [PK_LoadPeriodically] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[LoadSchedule]    Script Date: 09/06/2015 14:16:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[LoadSchedule](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ResourceIdentifier] [varchar](500) NOT NULL,
	[Healthboard] [varchar](1) NULL,
	[OriginDate] [datetime] NULL,
	[CacheProgress] [datetime] NULL,
	[DataLoadProgress] [datetime] NULL,
	[LastSuccesfulDataLoadRunID] [int] NULL,
	[LastSuccesfulDataLoadRunIDServer] [varchar](100) NULL,
	[LoadMetadata_ID] [int] NOT NULL,
	[LockedBecauseRunning] [bit] NOT NULL,
	[LockHeldBy] [varchar](100) NULL,
	[LoadPeriodicity] [varchar](10) NOT NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_LoadSchedule] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Lookup]    Script Date: 09/06/2015 14:16:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Lookup](
	[Description_ID] [int] NOT NULL,
	[ForeignKey_ID] [int] NOT NULL,
	[PrimaryKey_ID] [int] NOT NULL,
	[ExtractionJoinType] [varchar](100) NOT NULL,
	[Collation] [varchar](50) NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Lookup] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[LookupCompositeJoinInfo]    Script Date: 09/06/2015 14:16:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[LookupCompositeJoinInfo](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[OriginalLookup_ID] [int] NOT NULL,
	[ForeignKey_ID] [int] NOT NULL,
	[PrimaryKey_ID] [int] NOT NULL,
	[Collation] [varchar](50) NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_LookupCompositeJoinInfo] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[PreLoadDiscardedColumn]    Script Date: 09/06/2015 14:16:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[PreLoadDiscardedColumn](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TableInfo_ID] [int] NOT NULL,
	[Destination] [int] NOT NULL,
	[RuntimeColumnName] [varchar](500) NOT NULL,
	[SqlDataType] [varchar](50) NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
	[DuplicateRecordResolutionOrder] [int] NULL,
	[DuplicateRecordResolutionIsAscending] [bit] NOT NULL,
 CONSTRAINT [PK_PreLoadDiscardedColumn] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ProcessTask]    Script Date: 09/06/2015 14:16:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ProcessTask](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[LoadMetadata_ID] [int] NOT NULL,
	[Path] [varchar](500) NULL,
	[ProcessTaskType] [nchar](50) NOT NULL,
	[LoadStage] [nchar](50) NOT NULL,
	[Name] [varchar](500) NOT NULL,
	[Order] [int] NOT NULL,
	[RelatesSolelyToCatalogue_ID] [int] NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
	[IsDisabled] [bit] NOT NULL,
	[SerialisableConfiguration] [varchar](max),
 CONSTRAINT [PK_ProcessTask] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ProcessTaskArgument]    Script Date: 09/06/2015 14:16:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ProcessTaskArgument](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ProcessTask_ID] [int] NOT NULL,
	[Name] [varchar](500) NOT NULL,
	[Value] [varchar](max) NULL,
	[Type] [varchar](500) NOT NULL,
	[Description] [varchar](1000) NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_ProcessTaskArgument] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ServerDefaults]    Script Date: 09/06/2015 14:16:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ServerDefaults](
	[DefaultType] [varchar](500) NOT NULL,
	[ExternalDatabaseServer_ID] [int] NULL,
 CONSTRAINT [PK_ServerDefaults] PRIMARY KEY CLUSTERED 
(
	[DefaultType] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[SupportingDocument]    Script Date: 09/06/2015 14:16:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[SupportingDocument](
	[Catalogue_ID] [int] NOT NULL,
	[URL] [varchar](500) NULL,
	[Description] [varchar](2000) NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](50) NOT NULL,
	[Extractable] [bit] NOT NULL,
	[JIRATicket] [varchar](10) NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_SupportingDocument] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[SupportingSQLTable]    Script Date: 09/06/2015 14:16:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[SupportingSQLTable](
	[Catalogue_ID] [int] NOT NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Description] [varchar](2000) NULL,
	[Name] [varchar](200) NOT NULL,
	[Extractable] [bit] NOT NULL,
	[SQL] [varchar](8000) NULL,
	[ConnectionString] [varchar](1000) NULL,
	[IsGlobal] [bit] NOT NULL,
	[JIRATicket] [varchar](10) NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_SupportingSQLTable] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[TableInfo]    Script Date: 09/06/2015 14:16:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[TableInfo](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Store_type] [varchar](50) NULL,
	[Database_access] [varchar](50) NULL,
	[Database_name] [varchar](500) NULL,
	[Name] [varchar](1000) NULL,
	[State] [varchar](50) NULL,
	[ValidationXml] [nvarchar](max) NULL,
	[IsPrimaryExtractionTable] [bit] NOT NULL,
	[IsTableValuedFunction] [bit] NOT NULL,
	[IdentifierDumpServer_ID] [int] NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Data_Tables] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]


GO
/****** Object: Table [dbo],[Dataset] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].Dataset(
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](256) NOT NULL,
	[Folder] [nvarchar](1000) NOT NULL,
	[DigitalObjectIdentifier] [varchar](256) NULL,
	[Source] [varchar](256) NULL,
	CONSTRAINT [PK_Dataset] PRIMARY KEY CLUSTERED
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE TABLE [dbo].LoadMetadataCatalogueLinkage(
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[LoadMetadataID] [int] NOT NULL,
	[CatalogueID] [int] NOT NULL,
	FOREIGN KEY ([LoadMetadataID]) REFERENCES [dbo].[LoadMetadata](ID) ON DELETE CASCADE,
	FOREIGN KEY ([CatalogueID]) REFERENCES [dbo].[Catalogue](ID) ON DELETE CASCADE,
	CONSTRAINT [PK_LoadMetadataCatalogueLinkage] PRIMARY KEY CLUSTERED
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[ColumnInfo] ADD Dataset_ID [int] NULL
GO
ALTER TABLE [dbo].[ColumnInfo] ADD CONSTRAINT [FK_Column_Info_Dataset] FOREIGN KEY([Dataset_ID]) REFERENCES [dbo].[Dataset] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
GO
ALTER TABLE [dbo].[LoadMetadata] ADD LastLoadTime [datetime] NULL;
GO
SET ANSI_PADDING OFF
GO
ALTER TABLE [dbo].[ANOTable] ADD  CONSTRAINT [DF_ANOTable_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[AggregateConfiguration] ADD  CONSTRAINT [DF_AggregateConfiguration_dtCreated]  DEFAULT (getdate()) FOR [dtCreated]
GO
ALTER TABLE [dbo].[AggregateConfiguration] ADD  CONSTRAINT [DF_AggregateConfiguration_CountSQL]  DEFAULT ('count(*)') FOR [CountSQL]
GO
ALTER TABLE [dbo].[AggregateConfiguration] ADD  CONSTRAINT [DF_AggregateConfiguration_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[AggregateConfiguration] ADD  CONSTRAINT [DF_AggregateConfiguration_IsExtractable]  DEFAULT ((0)) FOR [IsExtractable]
GO
ALTER TABLE [dbo].[AggregateContinuousDateAxis] ADD  CONSTRAINT [DF_AggregateContinuousDateAxis_StartDate]  DEFAULT ('''2001-01-01''') FOR [StartDate]
GO
ALTER TABLE [dbo].[AggregateContinuousDateAxis] ADD  CONSTRAINT [DF_AggregateContinuousDateAxis_EndDate]  DEFAULT ('getdate()') FOR [EndDate]
GO
ALTER TABLE [dbo].[AggregateContinuousDateAxis] ADD  CONSTRAINT [DF_AggregateContinuousDateAxis_AxisIncrement]  DEFAULT ((1)) FOR [AxisIncrement]
GO
ALTER TABLE [dbo].[AggregateDimension] ADD  CONSTRAINT [DF_AggregateDimension_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[AggregateFilter] ADD  CONSTRAINT [DF_AggregateFilter_IsMandatory]  DEFAULT ((0)) FOR [IsMandatory]
GO
ALTER TABLE [dbo].[AggregateFilter] ADD  CONSTRAINT [DF_AggregateFilter_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[AggregateFilterContainer] ADD  CONSTRAINT [DF_AggregateFilterContainer_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[AggregateFilterParameter] ADD  CONSTRAINT [DF_AggregateFilterParameter_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[Catalogue] ADD  CONSTRAINT [DF_Catalogue_IsDepricated]  DEFAULT ((0)) FOR [IsDeprecated]
GO
ALTER TABLE [dbo].[Catalogue] ADD  CONSTRAINT [DF_Catalogue_IsInternalDataset]  DEFAULT ((0)) FOR [IsInternalDataset]
GO
ALTER TABLE [dbo].[Catalogue] ADD  CONSTRAINT [df_LiveLoggingServer_ID]  DEFAULT ([dbo].[GetDefaultExternalServerIDFor]('Catalogue.LiveLoggingServer_ID')) FOR [LiveLoggingServer_ID]
GO
ALTER TABLE [dbo].[Catalogue] ADD  CONSTRAINT [df_TestLoggingServer_ID]  DEFAULT ([dbo].[GetDefaultExternalServerIDFor]('Catalogue.TestLoggingServer_ID')) FOR [TestLoggingServer_ID]
GO
ALTER TABLE [dbo].[Catalogue] ADD  CONSTRAINT [DF_Catalogue_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[CatalogueItem] ADD  CONSTRAINT [DF_CatalogueItem_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[CatalogueItemIssue] ADD  CONSTRAINT [DF_CatalogueItemIssue_DateCreated]  DEFAULT (getdate()) FOR [DateCreated]
GO
ALTER TABLE [dbo].[CatalogueItemIssue] ADD  CONSTRAINT [DF_CatalogueItemIssue_Severity]  DEFAULT ('Red') FOR [Severity]
GO
ALTER TABLE [dbo].[CatalogueItemIssue] ADD  CONSTRAINT [DF_CatalogueItemIssue_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[ColumnInfo] ADD  CONSTRAINT [DF_ColumnInfo_IsPrimaryKey]  DEFAULT ((0)) FOR [IsPrimaryKey]
GO
ALTER TABLE [dbo].[ColumnInfo] ADD  CONSTRAINT [DF_ColumnInfo_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[ColumnInfo] ADD  CONSTRAINT [DF_ColumnInfo_DuplicateRecordResolutionIsAscending]  DEFAULT ((1)) FOR [DuplicateRecordResolutionIsAscending]
GO
ALTER TABLE [dbo].[DLEWindowsServiceException] ADD  CONSTRAINT [DF_DLEServiceExceptions_EventDate]  DEFAULT (getdate()) FOR [EventDate]
GO
ALTER TABLE [dbo].[ExternalDatabaseServer] ADD  CONSTRAINT [DF_ExternalDatabaseServer_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[ExtractionFilter] ADD  CONSTRAINT [DF_ExtractionFilter_IsMandatory]  DEFAULT ((0)) FOR [IsMandatory]
GO
ALTER TABLE [dbo].[ExtractionFilter] ADD  CONSTRAINT [DF_ExtractionFilter_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[ExtractionFilterParameter] ADD  CONSTRAINT [DF_ExtractionFilterParameter_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[ExtractionInformation] ADD  CONSTRAINT [DF_ExtractionInformation_HashOnDataRelease]  DEFAULT ((0)) FOR [HashOnDataRelease]
GO
ALTER TABLE [dbo].[ExtractionInformation] ADD  CONSTRAINT [DF_ExtractionInformation_IsExtractionIdentifier]  DEFAULT ((0)) FOR [IsExtractionIdentifier]
GO
ALTER TABLE [dbo].[ExtractionInformation] ADD  CONSTRAINT [DF_ExtractionInformation_IsPrimaryKey]  DEFAULT ((0)) FOR [IsPrimaryKey]
GO
ALTER TABLE [dbo].[ExtractionInformation] ADD  CONSTRAINT [DF_ExtractionInformation_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[IssueSystemUser] ADD  CONSTRAINT [DF_IssueSystemUser_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[JoinInfo] ADD  CONSTRAINT [DF_JoinInfo_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[LoadMetadata] ADD  CONSTRAINT [DF_LoadMetadata_IncludeDataset]  DEFAULT ((1)) FOR [IncludeDataset]
GO
ALTER TABLE [dbo].[LoadMetadata] ADD  CONSTRAINT [DF_LoadMetadata_UsesStandardisedLoadProcess]  DEFAULT ((1)) FOR [UsesStandardisedLoadProcess]
GO
ALTER TABLE [dbo].[LoadMetadata] ADD  CONSTRAINT [DF_LoadMetadata_EnableAnonymisation]  DEFAULT ((0)) FOR [EnableAnonymisation]
GO
ALTER TABLE [dbo].[LoadMetadata] ADD  CONSTRAINT [DF_LoadMetadata_SkipLookups]  DEFAULT ((0)) FOR [EnableLookupPopulation]
GO
ALTER TABLE [dbo].[LoadMetadata] ADD  CONSTRAINT [DF_LoadMetadata_EnablePrimaryKeyDuplicationResolution]  DEFAULT ((0)) FOR [EnablePrimaryKeyDuplicationResolution]
GO
ALTER TABLE [dbo].[LoadMetadata] ADD  CONSTRAINT [DF_LoadMetadata_CacheFilenameDateFormat]  DEFAULT ('yyyy-MM-dd') FOR [CacheFilenameDateFormat]
GO
ALTER TABLE [dbo].[LoadMetadata] ADD  CONSTRAINT [DF_LoadMetadata_CacheArchiveType]  DEFAULT ((0)) FOR [CacheArchiveType]
GO
ALTER TABLE [dbo].[LoadMetadata] ADD  CONSTRAINT [DF_LoadMetadata_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[LoadModuleAssembly] ADD  CONSTRAINT [DF_LoadModuleAssembly_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[LoadSchedule] ADD  CONSTRAINT [DF_LoadSchedule_CachingInProgress]  DEFAULT ((0)) FOR [LockedBecauseRunning]
GO
ALTER TABLE [dbo].[LoadSchedule] ADD  CONSTRAINT [DF_LoadSchedule_LoadPeriodicity]  DEFAULT ((1)) FOR [LoadPeriodicity]
GO
ALTER TABLE [dbo].[LoadSchedule] ADD  CONSTRAINT [DF_LoadSchedule_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[Lookup] ADD  CONSTRAINT [DF_Lookup_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[LookupCompositeJoinInfo] ADD  CONSTRAINT [DF_LookupCompositeJoinInfo_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[PreLoadDiscardedColumn] ADD  CONSTRAINT [DF_PreLoadDiscardedColumn_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[PreLoadDiscardedColumn] ADD  CONSTRAINT [DF_PreLoadDiscardedColumn_DuplicateRecordResolutionIsAscending]  DEFAULT ((1)) FOR [DuplicateRecordResolutionIsAscending]
GO
ALTER TABLE [dbo].[ProcessTask] ADD  CONSTRAINT [DF_ProcessTask_Order]  DEFAULT ((0)) FOR [Order]
GO
ALTER TABLE [dbo].[ProcessTask] ADD  CONSTRAINT [DF_ProcessTask_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[ProcessTask] ADD  CONSTRAINT [DF_ProcessTask_IsDisabled]  DEFAULT ((0)) FOR [IsDisabled]
GO
ALTER TABLE [dbo].[ProcessTaskArgument] ADD  CONSTRAINT [DF_ProcessTaskArgument_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[SupportingDocument] ADD  CONSTRAINT [DF_SupportingDocument_Extractable]  DEFAULT ((0)) FOR [Extractable]
GO
ALTER TABLE [dbo].[SupportingDocument] ADD  CONSTRAINT [DF_SupportingDocument_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[SupportingSQLTable] ADD  CONSTRAINT [DF_SupportingSQLTable_Extractable]  DEFAULT ((0)) FOR [Extractable]
GO
ALTER TABLE [dbo].[SupportingSQLTable] ADD  CONSTRAINT [DF_SupportingSQLTable_IsGlobal]  DEFAULT ((0)) FOR [IsGlobal]
GO
ALTER TABLE [dbo].[SupportingSQLTable] ADD  CONSTRAINT [DF_SupportingSQLTable_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[TableInfo] ADD  CONSTRAINT [DF_TableInfo_IsPrimaryExtractionTable]  DEFAULT ((0)) FOR [IsPrimaryExtractionTable]
GO
ALTER TABLE [dbo].[TableInfo] ADD  CONSTRAINT [DF_TableInfo_IsTableValuedFunction]  DEFAULT ((0)) FOR [IsTableValuedFunction]
GO
ALTER TABLE [dbo].[TableInfo] ADD  CONSTRAINT [df_IdentifierDumpServer_ID]  DEFAULT ([dbo].[GetDefaultExternalServerIDFor]('TableInfo.IdentifierDumpServer_ID')) FOR [IdentifierDumpServer_ID]
GO
ALTER TABLE [dbo].[TableInfo] ADD  CONSTRAINT [DF_TableInfo_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[ANOTable]  WITH CHECK ADD  CONSTRAINT [FK_ANOTable_ExternalDatabaseServer] FOREIGN KEY([Server_ID])
REFERENCES [dbo].[ExternalDatabaseServer] ([ID])
GO
ALTER TABLE [dbo].[ANOTable] CHECK CONSTRAINT [FK_ANOTable_ExternalDatabaseServer]
GO
ALTER TABLE [dbo].[AggregateConfiguration]  WITH CHECK ADD  CONSTRAINT [FK_AggregateConfiguration_AggregateDimension] FOREIGN KEY([PivotOnDimensionID])
REFERENCES [dbo].[AggregateDimension] ([ID])
GO
ALTER TABLE [dbo].[AggregateConfiguration] CHECK CONSTRAINT [FK_AggregateConfiguration_AggregateDimension]
GO
ALTER TABLE [dbo].[AggregateConfiguration]  WITH CHECK ADD  CONSTRAINT [FK_AggregateConfiguration_AggregateFilterContainer] FOREIGN KEY([RootFilterContainer_ID])
REFERENCES [dbo].[AggregateFilterContainer] ([ID])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[AggregateConfiguration] CHECK CONSTRAINT [FK_AggregateConfiguration_AggregateFilterContainer]
GO
ALTER TABLE [dbo].[AggregateConfiguration]  WITH CHECK ADD  CONSTRAINT [FK_AggregateConfiguration_Catalogue] FOREIGN KEY([Catalogue_ID])
REFERENCES [dbo].[Catalogue] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AggregateConfiguration] CHECK CONSTRAINT [FK_AggregateConfiguration_Catalogue]
GO
ALTER TABLE [dbo].[AggregateContinuousDateAxis]  WITH CHECK ADD  CONSTRAINT [FK_AggregateContinuousDateAxis_AggregateDimension] FOREIGN KEY([AggregateDimension_ID])
REFERENCES [dbo].[AggregateDimension] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AggregateContinuousDateAxis] CHECK CONSTRAINT [FK_AggregateContinuousDateAxis_AggregateDimension]
GO
ALTER TABLE [dbo].[AggregateDimension]  WITH CHECK ADD  CONSTRAINT [FK_AggregateDimension_AggregateConfiguration] FOREIGN KEY([AggregateConfiguration_ID])
REFERENCES [dbo].[AggregateConfiguration] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AggregateDimension] CHECK CONSTRAINT [FK_AggregateDimension_AggregateConfiguration]
GO
ALTER TABLE [dbo].[AggregateDimension]  WITH CHECK ADD  CONSTRAINT [FK_AggregateDimension_ExtractionInformation] FOREIGN KEY([ExtractionInformation_ID])
REFERENCES [dbo].[ExtractionInformation] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AggregateDimension] CHECK CONSTRAINT [FK_AggregateDimension_ExtractionInformation]
GO
ALTER TABLE [dbo].[AggregateFilter]  WITH CHECK ADD  CONSTRAINT [FK_AggregateFilter_AggregateFilterContainer] FOREIGN KEY([FilterContainer_ID])
REFERENCES [dbo].[AggregateFilterContainer] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AggregateFilter] CHECK CONSTRAINT [FK_AggregateFilter_AggregateFilterContainer]
GO
ALTER TABLE [dbo].[AggregateFilterParameter]  WITH CHECK ADD  CONSTRAINT [FK_AggregateFilterParameter_AggregateFilter] FOREIGN KEY([AggregateFilter_ID])
REFERENCES [dbo].[AggregateFilter] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AggregateFilterParameter] CHECK CONSTRAINT [FK_AggregateFilterParameter_AggregateFilter]
GO
ALTER TABLE [dbo].[AggregateFilterSubContainer]  WITH CHECK ADD  CONSTRAINT [FK_AggregateFilterSubContainer_AggregateFilterContainer] FOREIGN KEY([AggregateFilterContainer_ParentID])
REFERENCES [dbo].[AggregateFilterContainer] ([ID])
GO
ALTER TABLE [dbo].[AggregateFilterSubContainer] CHECK CONSTRAINT [FK_AggregateFilterSubContainer_AggregateFilterContainer]
GO
ALTER TABLE [dbo].[AggregateFilterSubContainer]  WITH CHECK ADD  CONSTRAINT [FK_AggregateFilterSubContainer_AggregateFilterContainer1] FOREIGN KEY([AggregateFilterContainer_ChildID])
REFERENCES [dbo].[AggregateFilterContainer] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AggregateFilterSubContainer] CHECK CONSTRAINT [FK_AggregateFilterSubContainer_AggregateFilterContainer1]
GO
ALTER TABLE [dbo].[AggregateForcedJoin]  WITH CHECK ADD  CONSTRAINT [FK_AggregateForcedJoin_AggregateConfiguration] FOREIGN KEY([AggregateConfiguration_ID])
REFERENCES [dbo].[AggregateConfiguration] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AggregateForcedJoin] CHECK CONSTRAINT [FK_AggregateForcedJoin_AggregateConfiguration]
GO
ALTER TABLE [dbo].[AggregateForcedJoin]  WITH CHECK ADD  CONSTRAINT [FK_AggregateForcedJoin_TableInfo] FOREIGN KEY([TableInfo_ID])
REFERENCES [dbo].[TableInfo] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AggregateForcedJoin] CHECK CONSTRAINT [FK_AggregateForcedJoin_TableInfo]
GO
ALTER TABLE [dbo].[Catalogue]  WITH CHECK ADD  CONSTRAINT [FK_Catalogue_ExternalDatabaseServer] FOREIGN KEY([LiveLoggingServer_ID])
REFERENCES [dbo].[ExternalDatabaseServer] ([ID])
GO
ALTER TABLE [dbo].[Catalogue] CHECK CONSTRAINT [FK_Catalogue_ExternalDatabaseServer]
GO
ALTER TABLE [dbo].[Catalogue]  WITH CHECK ADD  CONSTRAINT [FK_Catalogue_ExternalDatabaseServer1] FOREIGN KEY([TestLoggingServer_ID])
REFERENCES [dbo].[ExternalDatabaseServer] ([ID])
GO
ALTER TABLE [dbo].[Catalogue] CHECK CONSTRAINT [FK_Catalogue_ExternalDatabaseServer1]
GO
ALTER TABLE [dbo].[Catalogue]  WITH CHECK ADD  CONSTRAINT [FK_Catalogue_ExtractionInformation] FOREIGN KEY([TimeCoverage_ExtractionInformation_ID])
REFERENCES [dbo].[ExtractionInformation] ([ID])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[Catalogue] CHECK CONSTRAINT [FK_Catalogue_ExtractionInformation]
GO
ALTER TABLE [dbo].[CatalogueItem]  WITH CHECK ADD  CONSTRAINT [FK_Catalogue_Items_Data_Catalogue] FOREIGN KEY([Catalogue_ID])
REFERENCES [dbo].[Catalogue] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CatalogueItem] CHECK CONSTRAINT [FK_Catalogue_Items_Data_Catalogue]
GO
ALTER TABLE [dbo].[CatalogueItemIssue]  WITH CHECK ADD  CONSTRAINT [FK_CatalogueItemIssue_CatalogueItem] FOREIGN KEY([CatalogueItem_ID])
REFERENCES [dbo].[CatalogueItem] ([ID])
GO
ALTER TABLE [dbo].[CatalogueItemIssue] CHECK CONSTRAINT [FK_CatalogueItemIssue_CatalogueItem]
GO
ALTER TABLE [dbo].[CatalogueItemIssue]  WITH CHECK ADD  CONSTRAINT [FK_CatalogueItemIssue_Owner_IssueSystemUser] FOREIGN KEY([Owner_ID])
REFERENCES [dbo].[IssueSystemUser] ([ID])
GO
ALTER TABLE [dbo].[CatalogueItemIssue] CHECK CONSTRAINT [FK_CatalogueItemIssue_Owner_IssueSystemUser]
GO
ALTER TABLE [dbo].[CatalogueItemIssue]  WITH CHECK ADD  CONSTRAINT [FK_CatalogueItemIssue_Reporter_IssueSystemUser] FOREIGN KEY([ReportedBy_ID])
REFERENCES [dbo].[IssueSystemUser] ([ID])
GO
ALTER TABLE [dbo].[CatalogueItemIssue] CHECK CONSTRAINT [FK_CatalogueItemIssue_Reporter_IssueSystemUser]
GO
ALTER TABLE [dbo].[ColumnInfo]  WITH CHECK ADD  CONSTRAINT [FK_ColumnInfo_ANOTable] FOREIGN KEY([ANOTable_ID])
REFERENCES [dbo].[ANOTable] ([ID])
GO
ALTER TABLE [dbo].[ColumnInfo] CHECK CONSTRAINT [FK_ColumnInfo_ANOTable]
GO
ALTER TABLE [dbo].[ColumnInfo]  WITH CHECK ADD  CONSTRAINT [FK_Table_Items_Data_Tables] FOREIGN KEY([TableInfo_ID])
REFERENCES [dbo].[TableInfo] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ColumnInfo] CHECK CONSTRAINT [FK_Table_Items_Data_Tables]
GO
ALTER TABLE [dbo].[ColumnInfo_CatalogueItem]  WITH CHECK ADD  CONSTRAINT [FK_ColumnInfo_CatalogueItem_CatalogueItem] FOREIGN KEY([CatalogueItem_ID])
REFERENCES [dbo].[CatalogueItem] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ColumnInfo_CatalogueItem] CHECK CONSTRAINT [FK_ColumnInfo_CatalogueItem_CatalogueItem]
GO
ALTER TABLE [dbo].[ColumnInfo_CatalogueItem]  WITH CHECK ADD  CONSTRAINT [FK_ColumnInfo_CatalogueItem_ColumnInfo] FOREIGN KEY([ColumnInfo_ID])
REFERENCES [dbo].[ColumnInfo] ([ID])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[ColumnInfo_CatalogueItem] CHECK CONSTRAINT [FK_ColumnInfo_CatalogueItem_ColumnInfo]
GO
ALTER TABLE [dbo].[ColumnInfo_CatalogueItem]  WITH CHECK ADD  CONSTRAINT [FK_ColumnInfo_CatalogueItem_ExtractionInformation] FOREIGN KEY([ExtractionInformation_ID])
REFERENCES [dbo].[ExtractionInformation] ([ID])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[ColumnInfo_CatalogueItem] CHECK CONSTRAINT [FK_ColumnInfo_CatalogueItem_ExtractionInformation]
GO
ALTER TABLE [dbo].[DLEWindowsServiceException]  WITH CHECK ADD  CONSTRAINT [FK_DLEWindowsServiceException_LoadMetadata] FOREIGN KEY([LoadMetadata_ID])
REFERENCES [dbo].[LoadMetadata] ([ID])
GO
ALTER TABLE [dbo].[DLEWindowsServiceException] CHECK CONSTRAINT [FK_DLEWindowsServiceException_LoadMetadata]
GO
ALTER TABLE [dbo].[ExtractionFilter]  WITH CHECK ADD  CONSTRAINT [FK_ExtractionFilter_ExtractionInformation] FOREIGN KEY([ExtractionInformation_ID])
REFERENCES [dbo].[ExtractionInformation] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ExtractionFilter] CHECK CONSTRAINT [FK_ExtractionFilter_ExtractionInformation]
GO
ALTER TABLE [dbo].[ExtractionFilterParameter]  WITH CHECK ADD  CONSTRAINT [FK_ExtractionFilterParameter_ExtractionFilter] FOREIGN KEY([ExtractionFilter_ID])
REFERENCES [dbo].[ExtractionFilter] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ExtractionFilterParameter] CHECK CONSTRAINT [FK_ExtractionFilterParameter_ExtractionFilter]
GO
ALTER TABLE [dbo].[JoinInfo]  WITH CHECK ADD  CONSTRAINT [FK_JoinInfo_ColumnInfo_JoinKey1] FOREIGN KEY([ForeignKey_ID])
REFERENCES [dbo].[ColumnInfo] ([ID])
GO
ALTER TABLE [dbo].[JoinInfo] CHECK CONSTRAINT [FK_JoinInfo_ColumnInfo_JoinKey1]
GO
ALTER TABLE [dbo].[JoinInfo]  WITH CHECK ADD  CONSTRAINT [FK_JoinInfo_ColumnInfo_JoinKey2] FOREIGN KEY([PrimaryKey_ID])
REFERENCES [dbo].[ColumnInfo] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[JoinInfo] CHECK CONSTRAINT [FK_JoinInfo_ColumnInfo_JoinKey2]
GO
ALTER TABLE [dbo].[LoadPeriodically]  WITH CHECK ADD  CONSTRAINT [FK_LoadPeriodically_LoadMetadata] FOREIGN KEY([LoadMetadata_ID])
REFERENCES [dbo].[LoadMetadata] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[LoadPeriodically] CHECK CONSTRAINT [FK_LoadPeriodically_LoadMetadata]
GO
ALTER TABLE [dbo].[LoadPeriodically]  WITH CHECK ADD  CONSTRAINT [FK_LoadPeriodically_LoadMetadata1] FOREIGN KEY([OnSuccessLaunchLoadMetadata_ID])
REFERENCES [dbo].[LoadMetadata] ([ID])
GO
ALTER TABLE [dbo].[LoadPeriodically] CHECK CONSTRAINT [FK_LoadPeriodically_LoadMetadata1]
GO
ALTER TABLE [dbo].[LoadSchedule]  WITH CHECK ADD  CONSTRAINT [FK_LoadSchedule_LoadMetadata] FOREIGN KEY([LoadMetadata_ID])
REFERENCES [dbo].[LoadMetadata] ([ID])
GO
ALTER TABLE [dbo].[LoadSchedule] CHECK CONSTRAINT [FK_LoadSchedule_LoadMetadata]
GO
ALTER TABLE [dbo].[Lookup]  WITH CHECK ADD  CONSTRAINT [FK_Lookup_ColumnInfo] FOREIGN KEY([Description_ID])
REFERENCES [dbo].[ColumnInfo] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Lookup] CHECK CONSTRAINT [FK_Lookup_ColumnInfo]
GO
ALTER TABLE [dbo].[Lookup]  WITH CHECK ADD  CONSTRAINT [FK_Lookup_ColumnInfo1] FOREIGN KEY([ForeignKey_ID])
REFERENCES [dbo].[ColumnInfo] ([ID])
GO
ALTER TABLE [dbo].[Lookup] CHECK CONSTRAINT [FK_Lookup_ColumnInfo1]
GO
ALTER TABLE [dbo].[Lookup]  WITH CHECK ADD  CONSTRAINT [FK_Lookup_ColumnInfo2] FOREIGN KEY([PrimaryKey_ID])
REFERENCES [dbo].[ColumnInfo] ([ID])
GO
ALTER TABLE [dbo].[Lookup] CHECK CONSTRAINT [FK_Lookup_ColumnInfo2]
GO
ALTER TABLE [dbo].[LookupCompositeJoinInfo]  WITH CHECK ADD  CONSTRAINT [FK_LookupCompositeJoinInfo_ColumnInfo] FOREIGN KEY([PrimaryKey_ID])
REFERENCES [dbo].[ColumnInfo] ([ID])
GO
ALTER TABLE [dbo].[LookupCompositeJoinInfo] CHECK CONSTRAINT [FK_LookupCompositeJoinInfo_ColumnInfo]
GO
ALTER TABLE [dbo].[LookupCompositeJoinInfo]  WITH CHECK ADD  CONSTRAINT [FK_LookupCompositeJoinInfo_ColumnInfo_FK] FOREIGN KEY([ForeignKey_ID])
REFERENCES [dbo].[ColumnInfo] ([ID])
GO
ALTER TABLE [dbo].[LookupCompositeJoinInfo] CHECK CONSTRAINT [FK_LookupCompositeJoinInfo_ColumnInfo_FK]
GO
ALTER TABLE [dbo].[LookupCompositeJoinInfo]  WITH CHECK ADD  CONSTRAINT [FK_LookupCompositeJoinInfo_Lookup] FOREIGN KEY([OriginalLookup_ID])
REFERENCES [dbo].[Lookup] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[LookupCompositeJoinInfo] CHECK CONSTRAINT [FK_LookupCompositeJoinInfo_Lookup]
GO
ALTER TABLE [dbo].[PreLoadDiscardedColumn]  WITH CHECK ADD  CONSTRAINT [FK_PreLoadDiscardedColumn_TableInfo] FOREIGN KEY([TableInfo_ID])
REFERENCES [dbo].[TableInfo] ([ID])
GO
ALTER TABLE [dbo].[PreLoadDiscardedColumn] CHECK CONSTRAINT [FK_PreLoadDiscardedColumn_TableInfo]
GO
ALTER TABLE [dbo].[ProcessTask]  WITH CHECK ADD  CONSTRAINT [FK_ProcessTask_Catalogue] FOREIGN KEY([RelatesSolelyToCatalogue_ID])
REFERENCES [dbo].[Catalogue] ([ID])
GO
ALTER TABLE [dbo].[ProcessTask] CHECK CONSTRAINT [FK_ProcessTask_Catalogue]
GO
ALTER TABLE [dbo].[ProcessTask]  WITH CHECK ADD  CONSTRAINT [FK_ProcessTask_LoadMetadata] FOREIGN KEY([LoadMetadata_ID])
REFERENCES [dbo].[LoadMetadata] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ProcessTask] CHECK CONSTRAINT [FK_ProcessTask_LoadMetadata]
GO
ALTER TABLE [dbo].[ProcessTaskArgument]  WITH CHECK ADD  CONSTRAINT [FK_ProcessTaskArgument_ProcessTask] FOREIGN KEY([ProcessTask_ID])
REFERENCES [dbo].[ProcessTask] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ProcessTaskArgument] CHECK CONSTRAINT [FK_ProcessTaskArgument_ProcessTask]
GO
ALTER TABLE [dbo].[ServerDefaults]  WITH CHECK ADD  CONSTRAINT [FK_ServerDefaults_ExternalDatabaseServer] FOREIGN KEY([ExternalDatabaseServer_ID])
REFERENCES [dbo].[ExternalDatabaseServer] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ServerDefaults] CHECK CONSTRAINT [FK_ServerDefaults_ExternalDatabaseServer]
GO
ALTER TABLE [dbo].[SupportingDocument]  WITH CHECK ADD  CONSTRAINT [FK_SupportingDocument_Catalogue] FOREIGN KEY([Catalogue_ID])
REFERENCES [dbo].[Catalogue] ([ID])
GO
ALTER TABLE [dbo].[SupportingDocument] CHECK CONSTRAINT [FK_SupportingDocument_Catalogue]
GO
ALTER TABLE [dbo].[SupportingSQLTable]  WITH CHECK ADD  CONSTRAINT [FK_SupportingSQLTable_Catalogue] FOREIGN KEY([Catalogue_ID])
REFERENCES [dbo].[Catalogue] ([ID])
GO
ALTER TABLE [dbo].[SupportingSQLTable] CHECK CONSTRAINT [FK_SupportingSQLTable_Catalogue]
GO
ALTER TABLE [dbo].[TableInfo]  WITH CHECK ADD  CONSTRAINT [FK_TableInfo_ExternalDatabaseServer] FOREIGN KEY([IdentifierDumpServer_ID])
REFERENCES [dbo].[ExternalDatabaseServer] ([ID])
GO
ALTER TABLE [dbo].[TableInfo] CHECK CONSTRAINT [FK_TableInfo_ExternalDatabaseServer]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Table ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'ID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'‘SMR01’ for example' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'Acronym'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Fully expanded name or formula title' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'Name'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'A description of the data in non-technical terms' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'Description'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Link to page describing and explaining the data' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'Detail_Page_URL'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Time series, survey, cross section, geospatial, lab etc...' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'Type'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Sample period for data: Annual, qaurter, Month ...etc' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'Periodicity'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Geographical: EU, UK, Scotland, Tayside etc...' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'Geographical_coverage'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Any notes on the limitation, derviations or characteristics of the data of potential interest to the users or that the complier/ curator feels guilty about' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'Background_summary'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Specific subject that the data deals with' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'Search_keywords'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Period of referesh: Biannual, hourly, no fixed schedule...etc' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'Update_freq'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date if bext referesh: month, week, or exact day' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'Update_sched'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date range for available data in years (1989-2013)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'Time_coverage'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Exact date on last data refersh input data' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'Last_revision_date'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Email/URL to contact data owner' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'Contact_details'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Organisation/Institution from which data originated' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'Resource_owner'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Text to use when crediting data source in articles' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'Attribution_citation'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'How can data be accessed? API, Bulk download, Query tool, Bespoke Application ...etc' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'Access_options'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'URL for SOA access to data for programmatic query' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'API_access_URL'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'URL to data display tool with filter and display option' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'Browse_URL'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'URL for data bulk download' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'Bulk_Download_URL'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'URL to data explorer and cohort indentifier tool' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'Query_tool_URL'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'URL to use when crediting data source in articles' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'Source_URL'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Unique catalogue entry number as an integer ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CatalogueItem', @level2type=N'COLUMN',@level2name=N'Catalogue_ID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Unique Catalogue entry number as an integer ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CatalogueItem', @level2type=N'COLUMN',@level2name=N'ID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Unique admin identifier for the item in the catalogue' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CatalogueItem', @level2type=N'COLUMN',@level2name=N'Name'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Basis on which the data item was collected, observed or calculated' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CatalogueItem', @level2type=N'COLUMN',@level2name=N'Statistical_cons'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Why this item in the data set?' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CatalogueItem', @level2type=N'COLUMN',@level2name=N'Research_relevance'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Definition of the concept the item represents' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CatalogueItem', @level2type=N'COLUMN',@level2name=N'Description'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Subject area that the data item belongs to' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CatalogueItem', @level2type=N'COLUMN',@level2name=N'Topic'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Sample period for item: Annual, Quarter, Month, etc' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CatalogueItem', @level2type=N'COLUMN',@level2name=N'Periodicity'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'If the item ios aggregated, how was it done?' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CatalogueItem', @level2type=N'COLUMN',@level2name=N'Agg_method'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Caveats in data item collection, observation, processing of interest to users.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CatalogueItem', @level2type=N'COLUMN',@level2name=N'Limitations'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'General comments or any other obesrvations' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CatalogueItem', @level2type=N'COLUMN',@level2name=N'Comments'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Link Item to data catalogue' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CatalogueItem', @level2type=N'CONSTRAINT',@level2name=N'FK_Catalogue_Items_Data_Catalogue'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Database data type of the column' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ColumnInfo', @level2type=N'COLUMN',@level2name=N'Data_type'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Coding technique used to generate data item' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ColumnInfo', @level2type=N'COLUMN',@level2name=N'Digitisation_specs'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Loaded from input, drived, transformed ..etc' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ColumnInfo', @level2type=N'COLUMN',@level2name=N'Source'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'SQL DB, NoSQL DB, Hadopp...etc' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TableInfo', @level2type=N'COLUMN',@level2name=N'Store_type'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'IP Address:Port number, server name ..etc' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TableInfo', @level2type=N'COLUMN',@level2name=N'Database_access'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Name of database where the table is' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TableInfo', @level2type=N'COLUMN',@level2name=N'Database_name'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Table name' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TableInfo', @level2type=N'COLUMN',@level2name=N'Name'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Active, inactive, archived...etc' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TableInfo', @level2type=N'COLUMN',@level2name=N'State'
GO


--Diagrams
CREATE TABLE [dbo].[sysdiagrams](
	[name] [sysname] NOT NULL,
	[principal_id] [int] NOT NULL,
	[diagram_id] [int] IDENTITY(1,1) NOT NULL,
	[version] [int] NULL,
	[definition] [varbinary](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[diagram_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [UK_principal_name] UNIQUE NONCLUSTERED 
(
	[principal_id] ASC,
	[name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

EXEC sys.sp_addextendedproperty @name=N'microsoft_database_tools_support', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'sysdiagrams'
GO

 SET IDENTITY_INSERT sysdiagrams ON
insert into sysdiagrams(name,	principal_id,	diagram_id,	version,	definition)
VALUES
(N'Catalogue_Data_Diagram',	1,	1,	1,	
0xD0CF11E0A1B11AE1000000000000000000000000000000003E000300FEFF0900060000000000000000000000020000000100000000000000001000005A00000001000000FEFFFFFF00000000000000005D000000FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFDFFFFFF5C000000030000000400000005000000060000000700000008000000090000000A0000000B0000000C0000000D0000000E0000000F000000100000001100000012000000130000001400000015000000160000001700000018000000FEFFFFFF1A0000001B0000001C0000001D0000001E0000001F000000200000002100000022000000230000002400000025000000260000002700000028000000290000002A0000002B0000002C0000002D0000002E0000002F000000300000003100000032000000330000003400000035000000360000003700000038000000390000003A0000003B0000003C0000003D0000003E0000003F000000400000004100000042000000430000004400000045000000460000004700000048000000490000004A0000004B0000004C0000004D0000004E0000004F00000050000000510000005200000053000000540000005500000056000000570000005800000059000000FEFFFFFFFEFFFFFF91000000FEFFFFFFFDFFFFFF5F000000600000006100000062000000630000006400000065000000660000006700000068000000690000006A0000006B0000006C0000006D0000006E0000006F000000700000007100000072000000730000007400000075000000760000007700000078000000790000007A0000007B0000007C0000007D0000007E0000007F0000008000000052006F006F007400200045006E00740072007900000000000000000000000000000000000000000000000000000000000000000000000000000000000000000016000500FFFFFFFFFFFFFFFF0200000000000000000000000000000000000000000000000000000000000000705AF83BF38BD0015B000000C00A0000000000006600000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000004000201FFFFFFFFFFFFFFFFFFFFFFFF00000000000000000000000000000000000000000000000000000000000000000000000002000000322C0000000000006F000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000040002010100000004000000FFFFFFFF00000000000000000000000000000000000000000000000000000000000000000000000019000000AC80000000000000010043006F006D0070004F0062006A0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000012000201FFFFFFFFFFFFFFFFFFFFFFFF000000000000000000000000000000000000000000000000000000000000000000000000000000005F00000000000000000438000A1ED00D05000080D60000000F00FFFF5000000000000000D6000000007D0000E6930000175900001A9401001A4301000CCFFFFF7289FFFFDE805B10F195D011B0A000AA00BDCB5C000008003000000000020000030000003C006B0000000900000000000000D9E6B0E91C81D011AD5100A0C90F5739F43B7F847F61C74385352986E1D552F8A0327DB2D86295428D98273C25A2DA2D00002800430000000000000053444DD2011FD1118E63006097D2DF4834C9D2777977D811907000065B840D9C00002800430000000000000051444DD2011FD1118E63006097D2DF4834C9D2777977D811907000065B840D9C80000000382B000000FF01000170931500003800A50900000700008001000000AC020000008000000D0000805363684772696400A8480000E6FBFFFF436174616C6F6775654974656D07000000003400A50900000700008002000000A402000000800000090000805363684772696400A84800001E2D0000436174616C6F67756549746500008000A50900000700008003000000520000000180000058000080436F6E74726F6C00A74700004521000052656C6174696F6E736869702027464B5F436174616C6F6775655F4974656D735F446174615F436174616C6F67756527206265747765656E2027436174616C6F6775652720616E642027436174616C6F6775654974656D2700002800B50100000700008004000000310000007500000002800000436F6E74726F6C00ED490000E327000000003400A50900000700008005000000A402000000800000090000805363684772696400F4CFFFFF903300005461626C65496E666F49746500003400A50900000700008006000000A6020000008000000A0000805363684772696400FCD6FFFF6AFFFFFF436F6C756D6E496E666F746500007800A5090000070000800700000052000000018000004E000080436F6E74726F6C005BDFFFFF492A000052656C6174696F6E736869702027464B5F5461626C655F4974656D735F446174615F5461626C657327206265747765656E20275461626C65496E666F2720616E642027436F6C756D6E496E666F27000000002800B50100000700008008000000310000006700000002800000436F6E74726F6C00A1E1FFFF4E2F000000004000A5090000070000800D000000C20200000080000018000080536368477269640094110000A2E5FFFF436F6C756D6E496E666F5F436174616C6F6775654974656D00009400A509000007000080120000005A0000000180000069000080436F6E74726F6C0069FEFFFF51F6FFFF52656C6174696F6E736869702027464B5F436F6C756D6E496E666F5F436174616C6F6775654974656D5F436F6C756D6E496E666F27206265747765656E2027436F6C756D6E496E666F2720616E642027436F6C756D6E496E666F5F436174616C6F6775654974656D2700690000002800B50100000700008013000000310000007F00000002800000436F6E74726F6C00ABF6FFFFC5F5FFFF00009800A509000007000080160000006A000000018000006F000080436F6E74726F6C005B37000093F7FFFF52656C6174696F6E736869702027464B5F436F6C756D6E496E666F5F436174616C6F6775654974656D5F436174616C6F6775654974656D27206265747765656E2027436174616C6F6775654974656D2720616E642027436F6C756D6E496E666F5F436174616C6F6775654974656D270000002800B50100000700008017000000310000008500000002800000436F6E74726F6C005A29000075FAFFFF00003C00A50900000700008018000000B602000000800000120000805363684772696400FA7D00004A2E0000537570706F7274696E67446F63756D656E74756500008400A5090000070000801900000052000000018000005B000080436F6E74726F6C00F57000006331000052656C6174696F6E736869702027464B5F537570706F7274696E67446F63756D656E745F436174616C6F67756527206265747765656E2027436174616C6F6775652720616E642027537570706F7274696E67446F63756D656E74270000002800B5010000070000801A000000310000007100000002800000436F6E74726F6C004F6E0000A933000000004000A5090000070000801B000000BC020000008000001500008053636847726964007A0D0000E40C000045787472616374696F6E496E666F726D6174696F6E74656D0000A800A5090000070000802200000052000000018000007F000080436F6E74726F6C00391F000093F7FFFF52656C6174696F6E736869702027464B5F436F6C756D6E496E666F5F436174616C6F6775654974656D5F45787472616374696F6E496E666F726D6174696F6E27206265747765656E202745787472616374696F6E496E666F726D6174696F6E2720616E642027436F6C756D6E496E666F5F436174616C6F6775654974656D270000002800B50100000700008023000000310000009500000002800000436F6E74726F6C007F210000ED02000000003000A509000007000080240000009E02000000800000060000805363684772696400E2D2FFFF16DBFFFF4C6F6F6B7570640000007000A50900000700008027000000520000000180000045000080436F6E74726F6C0049E2FFFF49EDFFFF52656C6174696F6E736869702027464B5F4C6F6F6B75705F436F6C756D6E496E666F27206265747765656E2027436F6C756D6E496E666F2720616E6420274C6F6F6B757027D0135E00002800B50100000700008028000000310000005B00000002800000436F6E74726F6C008FE4FFFF0BF7FFFF00007000A50900000700008029000000520000000180000046000080436F6E74726F6C008FE7FFFF49EDFFFF52656C6174696F6E736869702027464B5F4C6F6F6B75705F436F6C756D6E496E666F3127206265747765656E2027436F6C756D6E496E666F2720616E6420274C6F6F6B757027135E00002800B5010000070000802A000000310000005D00000002800000436F6E74726F6C00D5E9FFFF0BF7FFFF00007000A5090000070000802B000000520000000180000046000080436F6E74726F6C005BDFFFFF49EDFFFF52656C6174696F6E736869702027464B5F4C6F6F6B75705F436F6C756D6E496E666F3227206265747765656E2027436F6C756D6E496E666F2720616E6420274C6F6F6B757027135E00002800B5010000070000802C000000310000005D00000002800000436F6E74726F6C00A1E1FFFF0BF7FFFF00003000A5090000070000802F000000A202000000800000080000805363684772696400A0ABFFFF12FDFFFF4A6F696E496E666F00007C00A50900000700008033000000520000000180000052000080436F6E74726F6C6F70D0FFFF95FFFFFF52656C6174696F6E736869702027464B5F4A6F696E496E666F5F436F6C756D6E496E666F5F4A6F696E4B65793127206265747765656E2027436F6C756D6E496E666F2720616E6420274A6F696E496E666F27654900002800B50100000700008034000000310000007100000002800000436F6E74726F6C6F9CD1FFFF25FFFFFF00007C00A50900000700008035000000520000000180000052000080436F6E74726F6C6F70D0FFFF4504000052656C6174696F6E736869702027464B5F4A6F696E496E666F5F436F6C756D6E496E666F5F4A6F696E4B65793227206265747765656E2027436F6C756D6E496E666F2720616E6420274A6F696E496E666F27654900002800B50100000700008036000000310000007100000002800000436F6E74726F6C6F9CD1FFFFD503000000003800A50900000700008038000000B20200000080000010000080536368477269646F12FDFFFFE835000045787472616374696F6E46696C74657200009800A5090000070000803900000052000000018000006F000080436F6E74726F6C6FEB1200001F2B000052656C6174696F6E736869702027464B5F45787472616374696F6E46696C7465725F45787472616374696F6E496E666F726D6174696F6E27206265747765656E202745787472616374696F6E496E666F726D6174696F6E2720616E64202745787472616374696F6E46696C746572270000002800B5010000070000803A000000310000008500000002800000436F6E74726F6C6F11FDFFFF3331000000004400A5090000070000803B000000C40200000080000019000080536368477269646F6AFFFFFFF654000045787472616374696F6E46696C746572506172616D657465720000000000A000A5090000070000803C000000520000000180000077000080436F6E74726F6C6F4D0B0000F64A000052656C6174696F6E736869702027464B5F45787472616374696F6E46696C746572506172616D657465725F45787472616374696F6E46696C74657227206265747765656E202745787472616374696F6E46696C7465722720616E64202745787472616374696F6E46696C746572506172616D65746572270000002800B5010000070000803D000000310000008D00000002800000436F6E74726F6C6F6DF3FFFFA550000000008C00A509000007000080400000005A0000000180000061000080436F6E74726F6C6F633100001F2B000052656C6174696F6E736869702027464B5F436174616C6F6775655F45787472616374696F6E496E666F726D6174696F6E27206265747765656E202745787472616374696F6E496E666F726D6174696F6E2720616E642027436174616C6F6775652700650000002800B50100000700008041000000310000007700000002800000436F6E74726F6C6F3E1E0000C76B000000004000A5090000070000804C000000C00200000080000017000080536368477269646F78D3FFFF22C0FFFF4C6F6F6B7570436F6D706F736974654A6F696E496E666F6D00008800A5090000070000805400000052000000018000005F000080436F6E74726F6C6F15DAFFFFC9D4FFFF52656C6174696F6E736869702027464B5F4C6F6F6B7570436F6D706F736974654A6F696E496E666F5F4C6F6F6B757027206265747765656E20274C6F6F6B75702720616E6420274C6F6F6B7570436F6D706F736974654A6F696E496E666F270000002800B50100000700008055000000310000007500000002800000436F6E74726F6C6F5BDCFFFFA1D8FFFF00009000A509000007000080560000006A0000000180000067000080436F6E74726F6C6F0FC8FFFF9DC8FFFF52656C6174696F6E736869702027464B5F4C6F6F6B7570436F6D706F736974654A6F696E496E666F5F436F6C756D6E496E666F27206265747765656E2027436F6C756D6E496E666F2720616E6420274C6F6F6B7570436F6D706F736974654A6F696E496E666F270000002800B50100000700008057000000310000007D00000002800000436F6E74726F6C6FEAC9FFFF9AE8FFFF00009400A509000007000080590000006A000000018000006A000080436F6E74726F6C6F57C1FFFF19C5FFFF52656C6174696F6E736869702027464B5F4C6F6F6B7570436F6D706F736974654A6F696E496E666F5F436F6C756D6E496E666F5F464B27206265747765656E2027436F6C756D6E496E666F2720616E6420274C6F6F6B7570436F6D706F736974654A6F696E496E666F276C7400002800B5010000070000805A000000310000008300000002800000436F6E74726F6C6F32C3FFFF08E5FFFF00003C00A5090000070000805C000000B60200000080000012000080536368477269646FDAAC0000D20F0000537570706F7274696E6753514C5461626C656E4900008400A5090000070000805D00000062000000018000005B000080436F6E74726F6C6FF5700000551F000052656C6174696F6E736869702027464B5F537570706F7274696E6753514C5461626C655F436174616C6F67756527206265747765656E2027436174616C6F6775652720616E642027537570706F7274696E6753514C5461626C65276E00002800B5010000070000805E000000310000007100000002800000436F6E74726F6C6F217200005131000000004000A5090000070000805F000000BE0200000080000016000080536368477269646FC201000086A7FFFF416767726567617465436F6E66696775726174696F6E6F6D00004000A50900000700008060000000C20200000080000018000080536368477269646F5C2B00006CA3FFFF41676772656761746546696C746572436F6E7461696E657200004400A50900000700008063000000C8020000008000001B000080536368477269646F7A580000FA9CFFFF41676772656761746546696C746572537562436F6E7461696E6572000000B400A5090000070000806400000052000000018000008B000080436F6E74726F6C6F6C4C000047A8FFFF52656C6174696F6E736869702027464B5F41676772656761746546696C746572537562436F6E7461696E65725F41676772656761746546696C746572436F6E7461696E657227206265747765656E202741676772656761746546696C746572436F6E7461696E65722720616E64202741676772656761746546696C746572537562436F6E7461696E6572270000002800B5010000070000806500000031000000A100000002800000436F6E74726F6C6FED4200008DAAFFFF0000B400A5090000070000806600000052000000018000008C000080436F6E74726F6C6F6C4C00006BA2FFFF52656C6174696F6E736869702027464B5F41676772656761746546696C746572537562436F6E7461696E65725F41676772656761746546696C746572436F6E7461696E65723127206265747765656E202741676772656761746546696C746572436F6E7461696E65722720616E64202741676772656761746546696C746572537562436F6E7461696E65722700002800B5010000070000806700000031000000A300000002800000436F6E74726F6C6F96420000FBA1FFFF00003800A5090000070000806E000000B0020000008000000F000080536368477269646F1059000050B0FFFF41676772656761746546696C7465727200009C00A5090000070000806F000000520000000180000073000080436F6E74726F6C6F6C4C00004FAFFFFF52656C6174696F6E736869702027464B5F41676772656761746546696C7465725F41676772656761746546696C746572436F6E7461696E657227206265747765656E202741676772656761746546696C746572436F6E7461696E65722720616E64202741676772656761746546696C746572277400002800B50100000700008070000000310000008900000002800000436F6E74726F6C6F55440000DFAEFFFF00003C00A50900000700008071000000B60200000080000012000080536368477269646FB42D000000B5FFFF41676772656761746544696D656E73696F6E74610000A000A50900000700008072000000520000000180000075000080436F6E74726F6C6FCC2200002BB5FFFF52656C6174696F6E736869702027464B5F41676772656761746544696D656E73696F6E5F416767726567617465436F6E66696775726174696F6E27206265747765656E2027416767726567617465436F6E66696775726174696F6E2720616E64202741676772656761746544696D656E73696F6E2772270000002800B50100000700008073000000310000008B00000002800000436F6E74726F6C6F351B000071B7FFFF0000AC00A50900000700008074000000520000000180000081000080436F6E74726F6C6FCC22000085A6FFFF52656C6174696F6E736869702027464B5F416767726567617465436F6E66696775726174696F6E5F41676772656761746546696C746572436F6E7461696E657227206265747765656E202741676772656761746546696C746572436F6E7461696E65722720616E642027416767726567617465436F6E66696775726174696F6E2700000000002800B50100000700008075000000310000009700000002800000436F6E74726F6C6FE818000015A6FFFF00004000A50900000700008076000000C20200000080000018000080536368477269646FD683000012B2FFFF41676772656761746546696C746572506172616D6574657200009C00A50900000700008077000000520000000180000073000080436F6E74726F6C6F1A7A000011B1FFFF52656C6174696F6E736869702027464B5F41676772656761746546696C746572506172616D657465725F41676772656761746546696C74657227206265747765656E202741676772656761746546696C7465722720616E64202741676772656761746546696C746572506172616D65746572276E00002800B50100000700008078000000310000008900000002800000436F6E74726F6C6FF072000057B3FFFF00003C00A5090000070000807C000000B60200000080000012000080536368477269646FF27600000CE5FFFF436174616C6F6775654974656D4973737565616D00008C00A509000007000080810000005A0000000180000063000080436F6E74726F6C6FD9600000F9F3FFFF52656C6174696F6E736869702027464B5F436174616C6F6775654974656D49737375655F436174616C6F6775654974656D27206265747765656E2027436174616C6F6775654974656D2720616E642027436174616C6F6775654974656D4973737565276E00002800B50100000700008082000000310000007900000002800000436F6E74726F6C6F8062000023F6FFFF00003800A50900000700008083000000B0020000008000000F000080536368477269646FF4B0000070CCFFFF497373756553797374656D557365727200009800A509000007000080840000005A000000018000006D000080436F6E74726F6C6F48A400004CDFFFFF52656C6174696F6E736869702027464B5F436174616C6F6775654974656D49737375655F4F776E65725F497373756553797374656D5573657227206265747765656E2027497373756553797374656D557365722720616E642027436174616C6F6775654974656D49737375652772270000002800B50100000700008085000000310000008900000002800000436F6E74726F6C6FF09700007CE3FFFF00009800A509000007000080860000005A0000000180000070000080436F6E74726F6C6F619A0000ABDAFFFF52656C6174696F6E736869702027464B5F436174616C6F6775654974656D49737375655F5265706F727465725F497373756553797374656D5573657227206265747765656E2027497373756553797374656D557365722720616E642027436174616C6F6775654974656D49737375652700002800B50100000700008087000000310000008F00000002800000436F6E74726F6C6F4B9200003BDAFFFF00003000A50900000700008088000000A20200000080000008000080536368477269646FC694FFFF56130000414E4F5461626C6500007400A5090000070000808B000000520000000180000049000080436F6E74726F6C65D6B5FFFF5512000052656C6174696F6E736869702027464B5F436F6C756D6E496E666F5F414E4F5461626C6527206265747765656E2027414E4F5461626C652720616E642027436F6C756D6E496E666F27496E6600002800B5010000070000808C000000310000005F00000002800000436F6E74726F6C65D8BFFFFF9B14000000004000A5090000070000808D000000BE0200000080000016000080536368477269646586A7FFFF302A00005072654C6F6164446973636172646564436F6C756D6E657200008C00A5090000070000808E000000520000000180000063000080436F6E74726F6C6596C8FFFF8F32000052656C6174696F6E736869702027464B5F5072654C6F6164446973636172646564436F6C756D6E5F5461626C65496E666F27206265747765656E20275461626C65496E666F2720616E6420275072654C6F6164446973636172646564436F6C756D6E276E00002800B5010000070000808F000000310000007900000002800000436F6E74726F6C6519C2FFFF1F32000000003C00A50900000700008091000000B80200000080000013000080536368477269646536D80000565E000050726F636573735461736B417267756D656E747500008C00A50900000700008094000000520000000180000061000080436F6E74726F6C6572D00000555D000052656C6174696F6E736869702027464B5F50726F636573735461736B417267756D656E745F50726F636573735461736B27206265747765656E202750726F636573735461736B2720616E64202750726F636573735461736B417267756D656E74276E276E00002800B50100000700008095000000310000007700000002800000436F6E74726F6C6544CA0000E55C000000003400A50900000700008090000000A8020000008000000B00008053636847726964659CAE0000D25A000050726F636573735461736B5500009C00A509000007000080960000005A0000000180000073000080436F6E74726F6C65F837000065C9FFFF52656C6174696F6E736869702027464B5F41676772656761746544696D656E73696F6E5F45787472616374696F6E496E666F726D6174696F6E27206265747765656E202745787472616374696F6E496E666F726D6174696F6E2720616E64202741676772656761746544696D656E73696F6E276E00002800B50100000700008097000000310000008900000002800000436F6E74726F6C658D4000008BF0FFFF00007800A5090000070000809A0000005A000000018000004D000080436F6E74726F6C65F5700000E87C000052656C6174696F6E736869702027464B5F50726F636573735461736B5F436174616C6F67756527206265747765656E2027436174616C6F6775652720616E64202750726F636573735461736B2727000000002800B5010000070000809B000000310000006300000002800000436F6E74726F6C65E99E00009592000000002400A5010000070000809C0000007100000002800000436F6E74726F6C65C47200005355000000003C00A5090000070000809D000000B60200000080000012000080536368477269646592B80000383100004C6F61644D6F64756C65417373656D626C79747500003400A5090000070000809E000000AA020000008000000C0000805363684772696465D8BD0000408300004C6F61645363686564756C6500004000A509000007000080A2000000BE0200000080000016000080536368477269646536F7FFFF0E6A000045787465726E616C4461746162617365536572766572657200008C00A509000007000080A30000005A0000000180000063000080436F6E74726F6C6539E1FFFF8E56000052656C6174696F6E736869702027464B5F5461626C65496E666F5F45787465726E616C446174616261736553657276657227206265747765656E202745787465726E616C44617461626173655365727665722720616E6420275461626C65496E666F276E00002800B501000007000080A4000000310000007900000002800000436F6E74726F6C6544CDFFFFAB6D000000008C00A509000007000080A60000005A0000000180000063000080436F6E74726F6C65B51B00006780000052656C6174696F6E736869702027464B5F436174616C6F6775655F45787465726E616C446174616261736553657276657227206265747765656E202745787465726E616C44617461626173655365727665722720616E642027436174616C6F677565276E00002800B501000007000080A7000000310000007900000002800000436F6E74726F6C65F50F0000B79D000000008C00A509000007000080A80000005A0000000180000064000080436F6E74726F6C65D91500006780000052656C6174696F6E736869702027464B5F436174616C6F6775655F45787465726E616C44617461626173655365727665723127206265747765656E202745787465726E616C44617461626173655365727665722720616E642027436174616C6F6775652700002800B501000007000080A9000000310000007B00000002800000436F6E74726F6C65B4090000FDA2000000008C00A509000007000080AA0000005A0000000180000061000080436F6E74726F6C65B3A3FFFF5723000052656C6174696F6E736869702027464B5F414E4F5461626C655F45787465726E616C446174616261736553657276657227206265747765656E202745787465726E616C44617461626173655365727665722720616E642027414E4F5461626C652775652700002800B501000007000080AB000000310000007700000002800000436F6E74726F6C65DDA5FFFF9268000000003800A509000007000080AC000000B2020000008000001000008053636847726964659E9D0000F69F00004C6F6164506572696F646963616C6C790000A000A509000007000080AF000000520000000180000075000080436F6E74726F6C65CC2200004DC0FFFF52656C6174696F6E736869702027464B5F416767726567617465436F6E66696775726174696F6E5F41676772656761746544696D656E73696F6E27206265747765656E202741676772656761746544696D656E73696F6E2720616E642027416767726567617465436F6E66696775726174696F6E2772270000002800B501000007000080B0000000310000008B00000002800000436F6E74726F6C655F1B0000DDBFFFFF00003400A509000007000080BD000000AA020000008000000C0000805363684772696465AA820000345300004C6F61644D6574616461746100007800A509000007000080BE00000052000000018000004F000080436F6E74726F6C65F57000005F53000052656C6174696F6E736869702027464B5F436174616C6F6775655F4C6F61644D6574616461746127206265747765656E20274C6F61644D657461646174612720616E642027436174616C6F677565270000002800B501000007000080BF000000310000006500000002800000436F6E74726F6C6566720000EF52000000008800A509000007000080C000000062000000018000005D000080436F6E74726F6C65FA9C0000CF85000052656C6174696F6E736869702027464B5F4C6F6164506572696F646963616C6C795F4C6F61644D6574616461746127206265747765656E20274C6F61644D657461646174612720616E6420274C6F6164506572696F646963616C6C79276F270000002800B501000007000080C1000000310000007300000002800000436F6E74726F6C65D98A00000695000000007C00A509000007000080C2000000520000000180000053000080436F6E74726F6C65659D0000A369000052656C6174696F6E736869702027464B5F50726F636573735461736B5F4C6F61644D6574616461746127206265747765656E20274C6F61644D657461646174612720616E64202750726F636573735461736B274900002800B501000007000080C3000000310000006900000002800000436F6E74726F6C65079E0000E96B000000008000A509000007000080C4000000620000000180000055000080436F6E74726F6C65659D0000517F000052656C6174696F6E736869702027464B5F4C6F61645363686564756C655F4C6F61644D6574616461746127206265747765656E20274C6F61644D657461646174612720616E6420274C6F61645363686564756C6527656D2700002800B501000007000080C5000000310000006B00000002800000436F6E74726F6C6581AA0000CF81000000004400A509000007000080C6000000C6020000008000001A000080536368477269646530750000CA9E0000444C4557696E646F777353657276696365457863657074696F6E720000009C00A509000007000080C7000000520000000180000071000080436F6E74726F6C65A98100005C86000052656C6174696F6E736869702027464B5F444C4557696E646F777353657276696365457863657074696F6E5F4C6F61644D6574616461746127206265747765656E20274C6F61644D657461646174612720616E642027444C4557696E646F777353657276696365457863657074696F6E27696F6E00002800B501000007000080C8000000310000008700000002800000436F6E74726F6C65FC6700004293000000008800A509000007000080C90000006A000000018000005E000080436F6E74726F6C65ED9700005C86000052656C6174696F6E736869702027464B5F4C6F6164506572696F646963616C6C795F4C6F61644D657461646174613127206265747765656E20274C6F61644D657461646174612720616E6420274C6F6164506572696F646963616C6C7927270000002800B501000007000080CA000000310000007500000002800000436F6E74726F6C65E7840000C995000000004400A509000007000080CB000000C8020000008000001B000080536368477269646562430000F4CFFFFF416767726567617465436F6E74696E756F75734461746541786973000000A800A509000007000080CC00000052000000018000007F000080436F6E74726F6C656142000069C9FFFF52656C6174696F6E736869702027464B5F416767726567617465436F6E74696E756F757344617465417869735F41676772656761746544696D656E73696F6E27206265747765656E202741676772656761746544696D656E73696F6E2720616E642027416767726567617465436F6E74696E756F75734461746541786973270000002800B501000007000080CD000000310000009500000002800000436F6E74726F6C65C22400005ECDFFFF00003C00A509000007000080D0000000B802000000800000130000805363684772696465B80B0000C8CEFFFF416767726567617465466F726365644A6F696E760000A000A509000007000080D10000005A0000000180000077000080436F6E74726F6C65DB04000016C9FFFF52656C6174696F6E736869702027464B5F416767726567617465466F726365644A6F696E5F416767726567617465436F6E66696775726174696F6E27206265747765656E2027416767726567617465436F6E66696775726174696F6E2720616E642027416767726567617465466F726365644A6F696E270000002800B501000007000080D2000000310000008D00000002800000436F6E74726F6C6564EAFFFFE9CFFFFF00008800A509000007000080D300000062000000018000005D000080436F6E74726F6C6582F8FFFF1FDDFFFF52656C6174696F6E736869702027464B5F416767726567617465466F726365644A6F696E5F5461626C65496E666F27206265747765656E20275461626C65496E666F2720616E642027416767726567617465466F726365644A6F696E2727270000002800B501000007000080D4000000310000007300000002800000436F6E74726F6C65360800002416000000008C00A509000007000080D50000007A0000000180000063000080436F6E74726F6C65AD21000012C9FFFF52656C6174696F6E736869702027464B5F416767726567617465436F6E66696775726174696F6E5F436174616C6F67756527206265747765656E2027436174616C6F6775652720616E642027416767726567617465436F6E66696775726174696F6E272700002800B501000007000080D6000000310000007900000002800000436F6E74726F6C656B3D00003DECFFFF0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000002143341208000000A72900001A28000078563412070000001401000043006100740061006C006F006700750065004900740065006D00000073000000000010400100000000000000000000000E000000050000001801000000000000000000000000000000000000F8000000000000000500000000000000000000000200000000000000009492400000000000000000000000000094924000000000000000400400000020000000300000000000000000000000008C924000000000000010400000000000000040000000000000004000000000000000000100000000000000050000000000000040000000010000000000000000000040000000000000104004000000200000002000000000000000000000000000000005000000540000002C0000002C0000002C000000340000000000000000000000A72900001A280000000000002D0100000D0000000C000000070000001C010000F70800005307000094020000390300003A020000DD040000DD040000EE020000DD04000036060000380400000000000001000000411700003F230000000000000C0000000C00000002000000020000001C010000E60A00000000000001000000F21300009408000000000000020000000200000002000000020000001C010000F70800000100000000000000F21300000804000000000000000000000000000002000000020000001C010000F7080000000000000000000055320000DD23000000000000000000000D00000004000000040000001C010000F70800009B0A00008106000078563412040000006400000001000000010000000B000000000000000100000002000000030000000400000005000000060000000700000008000000090000000A00000004000000640062006F0000000E00000043006100740061006C006F006700750065004900740065006D000000214334120800000079290000BC7E000078563412070000001401000043006100740061006C006F00670075006500000067007500650000000000870AC03D66020500000000000000020000000000000000000000000100000001000000000000484B75575034A75728307C57484B755763006F006D00700075007400650064005F0063006F006C0075006D006E007300200063006D0063006501006453C20F00E05A6B16109AA70A6F0062006A006500630074005F006900640020003D00200063006F006C002E006F0062006A006500630074005F0069006400200061006E006400200063006D0063002E0063006F006C0075006D006E005F006900640020003D00200063006F006C00000000000000000000000000000005000000540000002C0000002C0000002C00000034000000000000000000000079290000BC7E0000000000002D0100000D0000000C000000070000001C0100002F0D00005307000094020000390300003A020000DD040000DD040000EE020000DD04000036060000380400000000000001000000411700000341000000000000180000000C00000002000000020000001C010000D70A00000000000001000000F21300004E06000000000000010000000100000002000000020000001C010000F70800000100000000000000F21300000804000000000000000000000000000002000000020000001C010000F7080000000000000000000055320000DD23000000000000000000000D00000004000000040000001C010000F70800009B0A00008106000078563412040000005C00000001000000010000000B000000000000000100000002000000030000000400000005000000060000000700000008000000090000000A00000004000000640062006F0000000A00000043006100740061006C006F00670075006500000002000B003E4900001E2D00003E490000002400000000000002000000F0F0F00000000000000000000000000000000000010000000400000000000000ED490000E3270000BA1400005801000032000000010000020000BA14000058010000020000000000050000800800008001000000150001000000900144420100065461686F6D61210046004B005F0043006100740061006C006F006700750065005F004900740065006D0073005F0044006100740061005F0043006100740061006C006F006700750065002143341208000000BA290000B92500007856341207000000140100005400610062006C00650049006E0066006F00000073000000540020006E0061006D0065002C002000760061006C00750065002000460052004F004D0020007300790073002E0065007800740065006E006400650064005F00700072006F0070006500720074006900650073002000570048004500520045002000280063006C0061007300730020003D00200031002900200041004E004400200028006D0061006A006F0072005F006900640020003D0020004F0042004A004500430054005F004900440028004E0027005B00640062006F005D002E005B005400610062006C0065005F0031005D00270029002900000000000000000000000000000005000000540000002C0000002C0000002C000000340000000000000000000000BA290000B9250000000000002D010000080000000C000000070000001C010000F70800005307000094020000390300003A020000DD040000DD040000EE020000DD04000036060000380400000000000001000000AF1C0000A823000000000000060000000600000002000000020000001C010000F20D00000000000001000000F21300004E06000000000000010000000100000002000000020000001C010000F70800000100000000000000F21300000804000000000000000000000000000002000000020000001C010000F7080000000000000000000055320000DD23000000000000000000000D00000004000000040000001C010000F70800009B0A00008106000078563412040000005C00000001000000010000000B000000000000000100000002000000030000000400000005000000060000000700000008000000090000000A00000004000000640062006F0000000A0000005400610062006C00650049006E0066006F0000002143341208000000C42E00009A2D000078563412070000001401000043006F006C0075006D006E0049006E0066006F0000000000540020006E0061006D0065002C002000760061006C00750065002000460052004F004D0020007300790073002E0065007800740065006E006400650064005F00700072006F0070006500720074006900650073002000570048004500520045002000280063006C0061007300730020003D00200031002900200041004E004400200028006D0061006A006F0072005F006900640020003D0020004F0042004A004500430054005F004900440028004E0027005B00640062006F005D002E005B005400610062006C0065005F0031005D00270029002900000000000000000000000000000005000000540000002C0000002C0000002C000000340000000000000000000000C42E00009A2D0000000000002D010000080000000C000000070000001C010000F50A00005307000094020000390300003A020000DD040000DD040000EE020000DD04000036060000380400000000000001000000411700005D14000000000000060000000600000002000000020000001C010000E60A00000000000001000000F21300009408000000000000020000000200000002000000020000001C010000F70800000100000000000000F21300000804000000000000000000000000000002000000020000001C010000F7080000000000000000000055320000DD23000000000000000000000D00000004000000040000001C010000F70800009B0A00008106000078563412040000005E00000001000000010000000B000000000000000100000002000000030000000400000005000000060000000700000008000000090000000A00000004000000640062006F0000000B00000043006F006C0075006D006E0049006E0066006F00000002000B00F2E0FFFF90330000F2E0FFFF042D00000000000002000000F0F0F00000000000000000000000000000000000010000000800000000000000A1E1FFFF4E2F00001C10000058010000380000000100000200001C10000058010000020000000000050000800800008001000000150001000000900144420100065461686F6D611A0046004B005F005400610062006C0065005F004900740065006D0073005F0044006100740061005F005400610062006C00650073002143341208000000BA290000AC14000078563412070000001401000043006F006C0075006D006E0049006E0066006F005F0043006100740061006C006F006700750065004900740065006D0000000000050000001801000000000000000000000000000000000000F800000000000000050000000000000000000000020000000000000000D09D4000000000000000000000000000D09D400000000000000040040000002000000030000000000000000000000000C89D4000000000000010400000000000000040000000000000004000000000000000000100000000000000050000000000000040000000010000000000000000000040000000000000104004000000200000002000000000000000000000000000000005000000540000002C0000002C0000002C000000340000000000000000000000BA290000AC140000000000002D0100000D0000000C000000070000001C010000F70800005307000094020000390300003A020000DD040000DD040000EE020000DD04000036060000380400000000000001000000F21300000804000000000000000000000000000002000000020000001C010000F70800000000000001000000F21300000804000000000000000000000000000002000000020000001C010000F70800000100000000000000F21300000804000000000000000000000000000002000000020000001C010000F7080000000000000000000055320000DD23000000000000000000000D00000004000000040000001C010000F70800009B0A00008106000078563412040000007A00000001000000010000000B000000000000000100000002000000030000000400000005000000060000000700000008000000090000000A00000004000000640062006F0000001900000043006F006C0075006D006E0049006E0066006F005F0043006100740061006C006F006700750065004900740065006D00000003000B00000000006AFFFFFF00000000CCF7FFFF94110000CCF7FFFF0000000002000000F0F0F00000000000000000000000000000000000010000001300000000000000ABF6FFFFC5F5FFFFC51700005801000058000000010000020000C517000058010000020000000000050000800800008001000000150001000000900144420100065461686F6D61260046004B005F0043006F006C0075006D006E0049006E0066006F005F0043006100740061006C006F006700750065004900740065006D005F0043006F006C0075006D006E0049006E0066006F0005000B00A84800007CFCFFFFBF4000007CFCFFFFBF40000097FCFFFFD638000097FCFFFFD63800004EFAFFFF0000000002000000F0F0F000000000000000000000000000000000000100000017000000000000005A29000075FAFFFF92190000580100001D0000000100000200009219000058010000020000000000050000800800008001000000150001000000900144420100065461686F6D61290046004B005F0043006F006C0075006D006E0049006E0066006F005F0043006100740061006C006F006700750065004900740065006D005F0043006100740061006C006F006700750065004900740065006D0021433412080000003D2200005D1E000078563412070000001401000053007500700070006F007200740069006E00670044006F00630075006D0065006E0074000000000000000000D800000003000000010000000001000000000000446174615F5053004D756C7469706C7952616469616C4772616469656E7443656E74657265645F5053315F436F6E7374616E745461626C6500666C48616C66546578656C53697A654E6F726D616C697A656400AB000003000100010001000000000000007D00000098000000050000000100010001000100A80000004C000000B0000000050000000100010001000100C00000004D756C7469706C7952616469616C4772616469656E7443656E74000000000000000000000000000005000000540000002C0000002C0000002C0000003400000000000000000000003D2200005D1E0000000000002D0100000D0000000C000000070000001C01000026070000DC05000094020000390300003A020000DE030000DD040000EE020000DD04000036060000380400000000000001000000D91000000804000000000000000000000000000002000000020000001C010000260700000000000001000000D91000000804000000000000000000000000000002000000020000001C010000260700000100000000000000D91000000804000000000000000000000000000002000000020000001C0100002607000000000000000000003D2900004724000000000000000000000D00000004000000040000001C010000260700007F0800003705000078563412040000006E00000001000000010000000B000000000000000100000002000000030000000400000005000000060000000700000008000000090000000A00000004000000640062006F0000001300000053007500700070006F007200740069006E00670044006F00630075006D0065006E007400000002000B0021720000FA320000FA7D0000FA3200000000000002000000F0F0F00000000000000000000000000000000000010000001A000000000000004F6E0000A93300007C13000058010000320000000100000200007C13000058010000020000000000050000800800008001000000150001000000900144420100065461686F6D611F0046004B005F0053007500700070006F007200740069006E00670044006F00630075006D0065006E0074005F0043006100740061006C006F006700750065002143341208000000AA2B0000F2200000785634120700000014010000450078007400720061006300740069006F006E0049006E0066006F0072006D006100740069006F006E000000000000000E000000050000001801000000000000000000000000000000000000E800000000000000050000000000000000000000010000000000000000002A400000000000000000040000002000000020000000000000000000000000002E4000000000000000400000000000000040000000000000004000000000000000000100000000000000050000000000000040000000010000000000000000002E400000000000003640040000002000000020000000000000000000000000002A400000000000000000000000000000000005000000540000002C0000002C0000002C000000340000000000000000000000AA2B0000F2200000000000002D0100000D0000000C000000070000001C01000060090000DC05000094020000390300003A020000DE030000DD040000EE020000DD04000036060000380400000000000001000000D91000000804000000000000000000000000000002000000020000001C010000260700000000000001000000D91000000804000000000000000000000000000002000000020000001C010000260700000100000000000000D91000000804000000000000000000000000000002000000020000001C0100002607000000000000000000003D2900004724000000000000000000000D00000004000000040000001C010000260700007F0800003705000078563412040000007400000001000000010000000B000000000000000100000002000000030000000400000005000000060000000700000008000000090000000A00000004000000640062006F00000016000000450078007400720061006300740069006F006E0049006E0066006F0072006D006100740069006F006E00000002000B00D0200000E40C0000D02000004EFAFFFF0000000002000000F0F0F000000000000000000000000000000000000100000023000000000000007F210000ED020000671D00005801000032000000010000020000671D000058010000020000000000050000800800008001000000150001000000900144420100065461686F6D61310046004B005F0043006F006C0075006D006E0049006E0066006F005F0043006100740061006C006F006700750065004900740065006D005F00450078007400720061006300740069006F006E0049006E0066006F0072006D006100740069006F006E0021433412080000001C260000EE1400007856341207000000140100004C006F006F006B0075007000000000000001000010010000440000000200000001000000C800000000000000D800000003000000010000000001000000000000446174615F5053004D756C7469706C7952616469616C4772616469656E7443656E74657265645F5053315F436F6E7374616E745461626C6500666C48616C66546578656C53697A654E6F726D616C697A656400AB000003000100010001000000000000007D00000098000000050000000100010001000100A80000004C000000B0000000050000000100010001000100C00000004D756C7469706C7952616469616C4772616469656E7443656E74000000000000000000000000000005000000540000002C0000002C0000002C0000003400000000000000000000001C260000EE140000000000002D0100000D0000000C000000070000001C010000160800009F06000094020000390300003A02000065040000DD040000EE020000DD04000036060000380400000000000001000000661200000804000000000000000000000000000002000000020000001C010000160800000000000001000000661200000804000000000000000000000000000002000000020000001C010000160800000100000000000000661200000804000000000000000000000000000002000000020000001C010000160800000000000000000000E42D00001224000000000000000000000D00000004000000040000001C010000160800008D090000DC05000078563412040000005600000001000000010000000B000000000000000100000002000000030000000400000005000000060000000700000008000000090000000A00000004000000640062006F000000070000004C006F006F006B0075007000000002000B00E0E3FFFF6AFFFFFFE0E3FFFF04F0FFFF0000000002000000F0F0F000000000000000000000000000000000000100000028000000000000008FE4FFFF0BF7FFFFBB0C00005801000032000000010000020000BB0C000058010000020000000000050000800800008001000000150001000000900144420100065461686F6D61140046004B005F004C006F006F006B00750070005F0043006F006C0075006D006E0049006E0066006F0002000B0026E9FFFF6AFFFFFF26E9FFFF04F0FFFF0000000002000000F0F0F00000000000000000000000000000000000010000002A00000000000000D5E9FFFF0BF7FFFF680D00005801000032000000010000020000680D000058010000020000000000050000800800008001000000150001000000900144420100065461686F6D61150046004B005F004C006F006F006B00750070005F0043006F006C0075006D006E0049006E0066006F00310002000B00F2E0FFFF6AFFFFFFF2E0FFFF04F0FFFF0000000002000000F0F0F00000000000000000000000000000000000010000002C00000000000000A1E1FFFF0BF7FFFF680D00005801000032000000010000020000680D000058010000020000000000050000800800008001000000150001000000900144420100065461686F6D61150046004B005F004C006F006F006B00750070005F0043006F006C0075006D006E0049006E0066006F0032002143341208000000FC250000301500007856341207000000140100004A006F0069006E0049006E0066006F0000006E0064006F00770073002E0046006F0072006D0073002C002000560065007200730069006F006E003D0034002E0030002E0030002E0030002C002000430075006C0074007500720065003D006E00650075007400720061006C002C0020005000750062006C00690063004B006500790054006F006B0065006E003D00620037003700610035006300350036003100390033003400650030003800390000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000005000000540000002C0000002C0000002C000000340000000000000000000000FC25000030150000000000002D0100000D0000000C000000070000001C010000160800005406000094020000390300003A02000065040000DD040000EE020000DD04000036060000380400000000000001000000661200000804000000000000000000000000000002000000020000001C010000160800000000000001000000661200000804000000000000000000000000000002000000020000001C010000160800000100000000000000661200000804000000000000000000000000000002000000020000001C010000160800000000000000000000E42D00001224000000000000000000000D00000004000000040000001C010000160800008D090000DC05000078563412040000005A00000001000000010000000B000000000000000100000002000000030000000400000005000000060000000700000008000000090000000A00000004000000640062006F000000090000004A006F0069006E0049006E0066006F00000002000B00FCD6FFFF2C0100009CD1FFFF2C0100000000000002000000F0F0F000000000000000000000000000000000000100000034000000000000009CD1FFFF25FFFFFFD01200005801000064000000010000020000D012000058010000020000000000050000800800008001000000150001000000900144420100065461686F6D611F0046004B005F004A006F0069006E0049006E0066006F005F0043006F006C0075006D006E0049006E0066006F005F004A006F0069006E004B0065007900310002000B00FCD6FFFFDC0500009CD1FFFFDC0500000000000002000000F0F0F000000000000000000000000000000000000100000036000000000000009CD1FFFFD5030000D01200005801000064000000010000020000D012000058010000020000000000050000800800008001000000150001000000900144420100065461686F6D611F0046004B005F004A006F0069006E0049006E0066006F005F0043006F006C0075006D006E0049006E0066006F005F004A006F0069006E004B00650079003200214334120800000066230000C5170000785634120700000014010000450078007400720061006300740069006F006E00460069006C0074006500720000002E0030002E0030002E0030002C002000430075006C0074007500720065003D006E00650075007400720061006C002C0020005000750062006C00690063004B006500790054006F006B0065006E003D0062003700370061003500630035003600310039003300340065003000380039000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000005000000540000002C0000002C0000002C00000034000000000000000000000066230000C5170000000000002D0100000D0000000C000000070000001C010000070800009204000094020000390300003A02000029040000DD040000EE020000DD04000036060000380400000000000001000000D91000000804000000000000000000000000000002000000020000001C010000260700000000000001000000D91000000804000000000000000000000000000002000000020000001C010000260700000100000000000000D91000000804000000000000000000000000000002000000020000001C0100002607000000000000000000003D2900004724000000000000000000000D00000004000000040000001C010000260700007F0800003705000078563412040000006A00000001000000010000000B000000000000000100000002000000030000000400000005000000060000000700000008000000090000000A00000004000000640062006F00000011000000450078007400720061006300740069006F006E00460069006C00740065007200000002000B0082140000D62D000082140000E83500000000000002000000F0F0F00000000000000000000000000000000000010000003A0000000000000011FDFFFF33310000C21600005801000032000000010000020000C216000058010000020000000000050000800800008001000000150001000000900144420100065461686F6D61290046004B005F00450078007400720061006300740069006F006E00460069006C007400650072005F00450078007400720061006300740069006F006E0049006E0066006F0072006D006100740069006F006E002143341208000000EA1F00007A120000785634120700000014010000450078007400720061006300740069006F006E00460069006C0074006500720050006100720061006D006500740065007200000069006F006E003D0034002E0030002E0030002E0030002C002000430075006C0074007500720065003D006E00650075007400720061006C002C0020005000750062006C00690063004B006500790054006F006B0065006E003D00620037003700610035006300350036003100390033003400650030003800390000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000005000000540000002C0000002C0000002C000000340000000000000000000000EA1F00007A120000000000002D0100000D0000000C000000070000001C010000090600009204000094020000390300003A02000029040000DD040000EE020000DD04000036060000380400000000000001000000D91000000804000000000000000000000000000002000000020000001C010000260700000000000001000000D91000000804000000000000000000000000000002000000020000001C010000260700000100000000000000D91000000804000000000000000000000000000002000000020000001C0100002607000000000000000000003D2900004724000000000000000000000D00000004000000040000001C010000260700007F0800003705000078563412040000007C00000001000000010000000B000000000000000100000002000000030000000400000005000000060000000700000008000000090000000A00000004000000640062006F0000001A000000450078007400720061006300740069006F006E00460069006C0074006500720050006100720061006D006500740065007200000002000B00E40C0000AD4D0000E40C0000F65400000000000002000000F0F0F00000000000000000000000000000000000010000003D000000000000006DF3FFFFA5500000C81800005801000032000000010000020000C818000058010000020000000000050000800800008001000000150001000000900144420100065461686F6D612D0046004B005F00450078007400720061006300740069006F006E00460069006C0074006500720050006100720061006D0065007400650072005F00450078007400720061006300740069006F006E00460069006C0074006500720003000B00FA320000D62D0000FA320000F0870000A8480000F08700000000000002000000F0F0F000000000000000000000000000000000000100000041000000000000003E1E0000C76B00000D14000058010000370000000100000200000D14000058010000020000000000050000800800008001000000150001000000900144420100065461686F6D61220046004B005F0043006100740061006C006F006700750065005F00450078007400720061006300740069006F006E0049006E0066006F0072006D006100740069006F006E00214334120800000056250000621700007856341207000000140100004C006F006F006B007500700043006F006D0070006F0073006900740065004A006F0069006E0049006E0066006F000000680050006100720073006500720043006C00690065006E0074002C002000560065007200730069006F006E003D00310031002E0030002E0030002E0030002C002000430075006C0074007500720065003D006E00650075007400720061006C002C0020005000750062006C00690063004B006500790054006F006B0065006E003D00380039003800340035006400630064003800300038003000630063003900310000000000000000000000000000000000000000000000000000000000000000000000000000000000000005000000540000002C0000002C0000002C0000003400000000000000000000005625000062170000000000002D010000070000000C000000070000001C01000026070000DC05000094020000390300003A020000DE030000DD040000EE020000DD04000036060000380400000000000001000000042700004710000000000000040000000400000002000000020000001C010000CE1300000000000001000000D91000006806000000000000010000000100000002000000020000001C010000260700000100000000000000D91000000804000000000000000000000000000002000000020000001C0100002607000000000000000000003D2900004724000000000000000000000D00000004000000040000001C010000260700007F0800003705000078563412040000007800000001000000010000000B000000000000000100000002000000030000000400000005000000060000000700000008000000090000000A00000004000000640062006F000000180000004C006F006F006B007500700043006F006D0070006F0073006900740065004A006F0069006E0049006E0066006F00000002000B00ACDBFFFF16DBFFFFACDBFFFF84D7FFFF0000000002000000F0F0F000000000000000000000000000000000000100000055000000000000005BDCFFFFA1D8FFFF6414000058010000320000000100000200006414000058010000020000000000050000800800008001000000150001000000900144420100065461686F6D61210046004B005F004C006F006F006B007500700043006F006D0070006F0073006900740065004A006F0069006E0049006E0066006F005F004C006F006F006B007500700005000B006EDDFFFF6AFFFFFF6EDDFFFF07F9FFFF3BC9FFFF07F9FFFF3BC9FFFF18CAFFFF78D3FFFF18CAFFFF0000000002000000F0F0F00000000000000000000000000000000000010000005700000000000000EAC9FFFF9AE8FFFFC21600005801000032000000010000020000C216000058010000020000000000050000800800008001000000150001000000900144420100065461686F6D61250046004B005F004C006F006F006B007500700043006F006D0070006F0073006900740065004A006F0069006E0049006E0066006F005F0043006F006C0075006D006E0049006E0066006F0005000B0054D9FFFF6AFFFFFF54D9FFFFDDFBFFFF83C2FFFFDDFBFFFF83C2FFFF94C6FFFF78D3FFFF94C6FFFF0000000002000000F0F0F00000000000000000000000000000000000010000005A0000000000000032C3FFFF08E5FFFFC81800005801000032000000010000020000C818000058010000020000000000050000800800008001000000150001000000900144420100065461686F6D61280046004B005F004C006F006F006B007500700043006F006D0070006F0073006900740065004A006F0069006E0049006E0066006F005F0043006F006C0075006D006E0049006E0066006F005F0046004B0021433412080000003D2200005B1A000078563412070000001401000053007500700070006F007200740069006E006700530051004C005400610062006C00650000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000005000000540000002C0000002C0000002C0000003400000000000000000000003D2200005B1A0000000000002D0100000D0000000C000000070000001C01000026070000DC05000094020000390300003A020000DE030000DD040000EE020000DD04000036060000380400000000000001000000D91000000804000000000000000000000000000002000000020000001C010000260700000000000001000000D91000000804000000000000000000000000000002000000020000001C010000260700000100000000000000D91000000804000000000000000000000000000002000000020000001C0100002607000000000000000000003D2900004724000000000000000000000D00000004000000040000001C010000260700007F0800003705000078563412040000006E00000001000000010000000B000000000000000100000002000000030000000400000005000000060000000700000008000000090000000A00000004000000640062006F0000001300000053007500700070006F007200740069006E006700530051004C005400610062006C006500000004000B0021720000A2300000FB7A0000A2300000FB7A0000D0200000DAAC0000D02000000000000002000000F0F0F00000000000000000000000000000000000010000005E0000000000000021720000513100002613000058010000000000000100000200002613000058010000020000000000050000800800008001000000150001000000900144420100065461686F6D611F0046004B005F0053007500700070006F007200740069006E006700530051004C005400610062006C0065005F0043006100740061006C006F006700750065002143341208000000362200004724000078563412070000001401000041006700670072006500670061007400650043006F006E00660069006700750072006100740069006F006E000000000003000000010000000001000000000000446174615F5053004D756C7469706C7952616469616C4772616469656E7443656E74657265645F5053315F436F6E7374616E745461626C6500666C48616C66546578656C53697A654E6F726D616C697A656400AB000003000100010001000000000000007D00000098000000050000000100010001000100A80000004C000000B0000000050000000100010001000100C00000004D756C7469706C7952616469616C4772616469656E7443656E74000000000000000000000000000005000000540000002C0000002C0000002C0000003400000000000000000000003622000047240000000000002D0100000D0000000C000000070000001C01000026070000DC05000094020000390300003A020000DE030000DD040000EE020000DD04000036060000380400000000000001000000D91000000804000000000000000000000000000002000000020000001C010000260700000000000001000000D91000000804000000000000000000000000000002000000020000001C010000260700000100000000000000D91000000804000000000000000000000000000002000000020000001C0100002607000000000000000000003D2900004724000000000000000000000D00000004000000040000001C010000260700007F0800003705000078563412040000007600000001000000010000000B000000000000000100000002000000030000000400000005000000060000000700000008000000090000000A00000004000000640062006F0000001700000041006700670072006500670061007400650043006F006E00660069006700750072006100740069006F006E00000021433412080000003C2200005F0F0000785634120700000014010000410067006700720065006700610074006500460069006C0074006500720043006F006E007400610069006E0065007200000000000200000000001066000000010000200000005A66D11F3372CF63785FCA7F40C69EA7DCEDA7CC247BCFBB00C38E42D74AECAA000000000E800000000200002000000022DD4B17FD42971A525C6C9C486DCAF55C6C488516735862E36E352C2D42E2C460000000A7DE800C07B35B3F0D6263AC81AE6E31CA8A493A12274568746645EF462AD47FD8892C9460826B6F5F558A0E75CC4E4AC5561556A9455D20A14B4EE450E669AB1E892532D8D21B364B2F844052BF0F8ECA46A54A000000000000000000000000000005000000540000002C0000002C0000002C0000003400000000000000000000003C2200005F0F0000000000002D0100000D0000000C000000070000001C010000500A0000DC05000094020000390300003A020000DE030000DD040000EE020000DD04000036060000380400000000000001000000D91000000804000000000000000000000000000002000000020000001C010000260700000000000001000000D91000000804000000000000000000000000000002000000020000001C010000260700000100000000000000D91000000804000000000000000000000000000002000000020000001C0100002607000000000000000000003D2900004724000000000000000000000D00000004000000040000001C010000260700007F0800003705000078563412040000007A00000001000000010000000B000000000000000100000002000000030000000400000005000000060000000700000008000000090000000A00000004000000640062006F00000019000000410067006700720065006700610074006500460069006C0074006500720043006F006E007400610069006E0065007200000021433412080000003C220000CC100000785634120700000014010000410067006700720065006700610074006500460069006C0074006500720053007500620043006F006E007400610069006E006500720000006E003D0034002E0030002E0030002E0030002C002000430075006C0074007500720065003D006E00650075007400720061006C002C0020005000750062006C00690063004B006500790054006F006B0065006E003D00620037003700610035006300350036003100390033003400650030003800390000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000005000000540000002C0000002C0000002C0000003400000000000000000000003C220000CC100000000000002D0100000D0000000C000000070000001C010000300C0000DC05000094020000390300003A020000DE030000DD040000EE020000DD04000036060000380400000000000001000000D91000000804000000000000000000000000000002000000020000001C010000260700000000000001000000D91000000804000000000000000000000000000002000000020000001C010000260700000100000000000000D91000000804000000000000000000000000000002000000020000001C0100002607000000000000000000003D2900004724000000000000000000000D00000004000000040000001C010000260700007F0800003705000078563412040000008000000001000000010000000B000000000000000100000002000000030000000400000005000000060000000700000008000000090000000A00000004000000640062006F0000001C000000410067006700720065006700610074006500460069006C0074006500720053007500620043006F006E007400610069006E0065007200000002000B00984D0000DEA9FFFF7A580000DEA9FFFF0000000002000000F0F0F00000000000000000000000000000000000010000006500000000000000ED4200008DAAFFFF3820000058010000320000000100000200003820000058010000020000000000050000800800008001000000150001000000900144420100065461686F6D61370046004B005F00410067006700720065006700610074006500460069006C0074006500720053007500620043006F006E007400610069006E00650072005F00410067006700720065006700610074006500460069006C0074006500720043006F006E007400610069006E006500720002000B00984D000002A4FFFF7A58000002A4FFFF0000000002000000F0F0F0000000000000000000000000000000000001000000670000000000000096420000FBA1FFFFE52000005801000032000000010000020000E520000058010000020000000000050000800800008001000000150001000000900144420100065461686F6D61380046004B005F00410067006700720065006700610074006500460069006C0074006500720053007500620043006F006E007400610069006E00650072005F00410067006700720065006700610074006500460069006C0074006500720043006F006E007400610069006E00650072003100214334120800000036220000CF1C0000785634120700000014010000410067006700720065006700610074006500460069006C0074006500720000007600650072002E004200610074006300680050006100720073006500720043006C00690065006E0074002C002000560065007200730069006F006E003D00310031002E0030002E0030002E0030002C002000430075006C0074007500720065003D006E00650075007400720061006C002C0020005000750062006C00690063004B006500790054006F006B0065006E003D00380039003800340035006400630064003800300038003000630063003900310000000000000000000000000000000000000000000000000000000000000000000000000000000000000005000000540000002C0000002C0000002C00000034000000000000000000000036220000CF1C0000000000002D010000070000000C000000070000001C01000026070000DC05000094020000390300003A020000DE030000DD040000EE020000DD040000360600003804000000000000010000000B2400005015000000000000050000000500000002000000020000001C0100002A1200000000000001000000D9100000AF08000000000000020000000200000002000000020000001C010000260700000100000000000000D91000000804000000000000000000000000000002000000020000001C0100002607000000000000000000003D2900004724000000000000000000000D00000004000000040000001C010000260700007F0800003705000078563412040000006800000001000000010000000B000000000000000100000002000000030000000400000005000000060000000700000008000000090000000A00000004000000640062006F00000010000000410067006700720065006700610074006500460069006C00740065007200000002000B00984D0000E6B0FFFF10590000E6B0FFFF0000000002000000F0F0F0000000000000000000000000000000000001000000700000000000000055440000DFAEFFFFE51800005801000045000000010000020000E518000058010000020000000000050000800800008001000000150001000000900144420100065461686F6D612B0046004B005F00410067006700720065006700610074006500460069006C007400650072005F00410067006700720065006700610074006500460069006C0074006500720043006F006E007400610069006E006500720021433412080000007829000020170000785634120700000014010000410067006700720065006700610074006500440069006D0065006E00730069006F006E000000674797F9A80390A9CEBA040000000200000000001066000000010000200000005A66D11F3372CF63785FCA7F40C69EA7DCEDA7CC247BCFBB00C38E42D74AECAA000000000E800000000200002000000022DD4B17FD42971A525C6C9C486DCAF55C6C488516735862E36E352C2D42E2C460000000A7DE800C07B35B3F0D6263AC81AE6E31CA8A493A12274568746645EF462AD47FD8892C9460826B6F5F558A0E75CC4E4AC5561556A9455D20A14B4EE450E669AB1E892532D8D21B364B2F844052BF0F8ECA46A54A000000000000000000000000000005000000540000002C0000002C0000002C0000003400000000000000000000007829000020170000000000002D0100000D0000000C000000070000001C0100005F0A0000DC05000094020000390300003A020000DE030000DD040000EE020000DD04000036060000380400000000000001000000D91000000804000000000000000000000000000002000000020000001C010000260700000000000001000000D91000000804000000000000000000000000000002000000020000001C010000260700000100000000000000D91000000804000000000000000000000000000002000000020000001C0100002607000000000000000000003D2900004724000000000000000000000D00000004000000040000001C010000260700007F0800003705000078563412040000006E00000001000000010000000B000000000000000100000002000000030000000400000005000000060000000700000008000000090000000A00000004000000640062006F00000013000000410067006700720065006700610074006500440069006D0065006E00730069006F006E00000002000B00F8230000C2B6FFFFB42D0000C2B6FFFF0000000002000000F0F0F00000000000000000000000000000000000010000007300000000000000351B000071B7FFFFED1A00005801000033000000010000020000ED1A000058010000020000000000050000800800008001000000150001000000900144420100065461686F6D612C0046004B005F00410067006700720065006700610074006500440069006D0065006E00730069006F006E005F0041006700670072006500670061007400650043006F006E00660069006700750072006100740069006F006E0002000B005C2B00001CA8FFFFF82300001CA8FFFF0000000002000000F0F0F00000000000000000000000000000000000010000007500000000000000E818000015A6FFFF831D00005801000032000000010000020000831D000058010000020000000000050000800800008001000000150001000000900144420100065461686F6D61320046004B005F0041006700670072006500670061007400650043006F006E00660069006700750072006100740069006F006E005F00410067006700720065006700610074006500460069006C0074006500720043006F006E007400610069006E006500720021433412080000003C2200000E150000785634120700000014010000410067006700720065006700610074006500460069006C0074006500720050006100720061006D006500740065007200000000000200000000001066000000010000200000005A66D11F3372CF63785FCA7F40C69EA7DCEDA7CC247BCFBB00C38E42D74AECAA000000000E800000000200002000000022DD4B17FD42971A525C6C9C486DCAF55C6C488516735862E36E352C2D42E2C460000000A7DE800C07B35B3F0D6263AC81AE6E31CA8A493A12274568746645EF462AD47FD8892C9460826B6F5F558A0E75CC4E4AC5561556A9455D20A14B4EE450E669AB1E892532D8D21B364B2F844052BF0F8ECA46A54A000000000000000000000000000005000000540000002C0000002C0000002C0000003400000000000000000000003C2200000E150000000000002D0100000D0000000C000000070000001C01000026070000DC05000094020000390300003A020000DE030000DD040000EE020000DD04000036060000380400000000000001000000D91000000804000000000000000000000000000002000000020000001C010000260700000000000001000000D91000000804000000000000000000000000000002000000020000001C010000260700000100000000000000D91000000804000000000000000000000000000002000000020000001C0100002607000000000000000000003D2900004724000000000000000000000D00000004000000040000001C010000260700007F0800003705000078563412040000007A00000001000000010000000B000000000000000100000002000000030000000400000005000000060000000700000008000000090000000A00000004000000640062006F00000019000000410067006700720065006700610074006500460069006C0074006500720050006100720061006D006500740065007200000002000B00467B0000A8B2FFFFD6830000A8B2FFFF0000000002000000F0F0F00000000000000000000000000000000000010000007800000000000000F072000057B3FFFF3C19000058010000320000000100000200003C19000058010000020000000000050000800800008001000000150001000000900144420100065461686F6D612B0046004B005F00410067006700720065006700610074006500460069006C0074006500720050006100720061006D0065007400650072005F00410067006700720065006700610074006500460069006C007400650072002143341208000000822E0000641F000078563412070000001401000043006100740061006C006F006700750065004900740065006D004900730073007500650000002E0030002E0030002C002000430075006C0074007500720065003D006E00650075007400720061006C002C0020005000750062006C00690063004B006500790054006F006B0065006E003D0062003700370061003500630035003600310039003300340065003000380039000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000005000000540000002C0000002C0000002C000000340000000000000000000000822E0000641F0000000000002D010000090000000C000000070000001C0100006C0C0000DC05000094020000390300003A020000DE030000DD040000EE020000DD040000360600003804000000000000010000003E260000E717000000000000070000000700000002000000020000001C010000561300000000000001000000D91000006806000000000000010000000100000002000000020000001C010000260700000100000000000000D91000009E03000000000000000000000000000002000000020000001C0100002607000000000000000000003D2900004724000000000000000000000D00000004000000040000001C010000260700007F0800003705000078563412040000006E00000001000000010000000B000000000000000100000002000000030000000400000005000000060000000700000008000000090000000A00000004000000640062006F0000001300000043006100740061006C006F006700750065004900740065006D0049007300730075006500000003000B0070620000E6FBFFFF7062000074F5FFFFF276000074F5FFFF0000000002000000F0F0F000000000000000000000000000000000000100000082000000000000008062000023F6FFFFA11500005801000032000000010000020000A115000058010000020000000000050000800800008001000000150001000000900144420100065461686F6D61230046004B005F0043006100740061006C006F006700750065004900740065006D00490073007300750065005F0043006100740061006C006F006700750065004900740065006D0021433412080000003C2200009315000078563412070000001401000049007300730075006500530079007300740065006D005500730065007200000001000000C800000000000000D800000003000000010000000001000000000000446174615F5053004D756C7469706C7952616469616C4772616469656E7443656E74657265645F5053315F436F6E7374616E745461626C6500666C48616C66546578656C53697A654E6F726D616C697A656400AB000003000100010001000000000000007D00000098000000050000000100010001000100A80000004C000000B0000000050000000100010001000100C00000004D756C7469706C7952616469616C4772616469656E7443656E74000000000000000000000000000005000000540000002C0000002C0000002C0000003400000000000000000000003C22000093150000000000002D0100000D0000000C000000070000001C01000026070000DC05000094020000390300003A020000DE030000DD040000EE020000DD04000036060000380400000000000001000000D91000000804000000000000000000000000000002000000020000001C010000260700000000000001000000D91000000804000000000000000000000000000002000000020000001C010000260700000100000000000000D91000000804000000000000000000000000000002000000020000001C0100002607000000000000000000003D2900004724000000000000000000000D00000004000000040000001C010000260700007F0800003705000078563412040000006800000001000000010000000B000000000000000100000002000000030000000400000005000000060000000700000008000000090000000A00000004000000640062006F0000001000000049007300730075006500530079007300740065006D005500730065007200000003000B00E2B3000003E2FFFFE2B3000040EDFFFF74A5000040EDFFFF0000000002000000F0F0F00000000000000000000000000000000000010000008500000000000000F09700007CE3FFFF431B00005801000007000000010000020000431B000058010000020000000000050000800800008001000000150001000000900144420100065461686F6D612B0046004B005F0043006100740061006C006F006700750065004900740065006D00490073007300750065005F004F0077006E00650072005F0049007300730075006500530079007300740065006D00550073006500720003000B00F4B0000042DCFFFFDC9B000042DCFFFFDC9B00000CE5FFFF0000000002000000F0F0F000000000000000000000000000000000000100000087000000000000004B9200003BDAFFFF801C00005801000007000000010000020000801C000058010000020000000000050000800800008001000000150001000000900144420100065461686F6D612E0046004B005F0043006100740061006C006F006700750065004900740065006D00490073007300750065005F005200650070006F0072007400650072005F0049007300730075006500530079007300740065006D00550073006500720021433412080000003C220000BC12000078563412070000001401000041004E004F005400610062006C00650000006E0066006F00000000000200000001000000C800000000000000D800000003000000010000000001000000000000446174615F5053004D756C7469706C7952616469616C4772616469656E7443656E74657265645F5053315F436F6E7374616E745461626C6500666C48616C66546578656C53697A654E6F726D616C697A656400AB000003000100010001000000000000007D00000098000000050000000100010001000100A80000004C000000B0000000050000000100010001000100C00000004D756C7469706C7952616469616C4772616469656E7443656E74000000000000000000000000000005000000540000002C0000002C0000002C0000003400000000000000000000003C220000BC120000000000002D0100000D0000000C000000070000001C01000026070000DC05000094020000390300003A020000DE030000DD040000EE020000DD04000036060000380400000000000001000000D91000000804000000000000000000000000000002000000020000001C010000260700000000000001000000D91000000804000000000000000000000000000002000000020000001C010000260700000100000000000000D91000000804000000000000000000000000000002000000020000001C0100002607000000000000000000003D2900004724000000000000000000000D00000004000000040000001C010000260700007F0800003705000078563412040000005A00000001000000010000000B000000000000000100000002000000030000000400000005000000060000000700000008000000090000000A00000004000000640062006F0000000900000041004E004F005400610062006C006500000002000B0002B7FFFFEC130000FCD6FFFFEC1300000000000002000000F0F0F00000000000000000000000000000000000010000008C00000000000000D8BFFFFF9B1400004E0E000058010000320000000100000200004E0E000058010000020000000000050000800800008001000000150001000000900144420100065461686F6D61160046004B005F0043006F006C0075006D006E0049006E0066006F005F0041004E004F005400610062006C00650021433412080000003C2200007B1A00007856341207000000140100005000720065004C006F006100640044006900730063006100720064006500640043006F006C0075006D006E0000000000040000000200000000001066000000010000200000005A66D11F3372CF63785FCA7F40C69EA7DCEDA7CC247BCFBB00C38E42D74AECAA000000000E800000000200002000000022DD4B17FD42971A525C6C9C486DCAF55C6C488516735862E36E352C2D42E2C460000000A7DE800C07B35B3F0D6263AC81AE6E31CA8A493A12274568746645EF462AD47FD8892C9460826B6F5F558A0E75CC4E4AC5561556A9455D20A14B4EE450E669AB1E892532D8D21B364B2F844052BF0F8ECA46A54A000000000000000000000000000005000000540000002C0000002C0000002C0000003400000000000000000000003C2200007B1A0000000000002D0100000D0000000C000000070000001C01000052080000DC05000094020000390300003A020000DE030000DD040000EE020000DD04000036060000380400000000000001000000D91000000804000000000000000000000000000002000000020000001C010000260700000000000001000000D91000000804000000000000000000000000000002000000020000001C010000260700000100000000000000D91000000804000000000000000000000000000002000000020000001C0100002607000000000000000000003D2900004724000000000000000000000D00000004000000040000001C010000260700007F0800003705000078563412040000007600000001000000010000000B000000000000000100000002000000030000000400000005000000060000000700000008000000090000000A00000004000000640062006F000000170000005000720065004C006F006100640044006900730063006100720064006500640043006F006C0075006D006E00000002000B00F4CFFFFF26340000C2C9FFFF263400000000000002000000F0F0F00000000000000000000000000000000000010000008F0000000000000019C2FFFF1F3200008415000058010000320000000100000200008415000058010000020000000000050000800800008001000000150001000000900144420100065461686F6D61230046004B005F005000720065004C006F006100640044006900730063006100720064006500640043006F006C0075006D006E005F005400610062006C00650049006E0066006F0021433412080000009A290000A51B0000785634120700000014010000500072006F0063006500730073005400610073006B0041007200670075006D0065006E00740000002C002000560065007200730069006F006E003D0034002E0030002E0030002E0030002C002000430075006C0074007500720065003D006E00650075007400720061006C002C0020005000750062006C00690063004B006500790054006F006B0065006E003D00620037003700610035006300350036003100390033003400650030003800390000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000005000000540000002C0000002C0000002C0000003400000000000000000000009A290000A51B0000000000002D0100000D0000000C000000070000001C010000F70800005307000094020000390300003A020000DD040000DD040000EE020000DD04000036060000380400000000000001000000F21300000804000000000000000000000000000002000000020000001C010000F70800000000000001000000F21300000804000000000000000000000000000002000000020000001C010000F70800000100000000000000F21300000804000000000000000000000000000002000000020000001C010000F7080000000000000000000055320000DD23000000000000000000000D00000004000000040000001C010000F70800009B0A00008106000078563412040000007000000001000000010000000B000000000000000100000002000000030000000400000005000000060000000700000008000000090000000A00000004000000640062006F00000014000000500072006F0063006500730073005400610073006B0041007200670075006D0065006E007400000002000B009ED10000EC5E000036D80000EC5E00000000000002000000F0F0F0000000000000000000000000000000000001000000950000000000000044CA0000E55C00004B15000058010000320000000100000200004B15000058010000020000000000050000800800008001000000150001000000900144420100065461686F6D61220046004B005F00500072006F0063006500730073005400610073006B0041007200670075006D0065006E0074005F00500072006F0063006500730073005400610073006B00214334120800000002230000D1240000785634120700000014010000500072006F0063006500730073005400610073006B0000006700720061006D002000460069006C00650073002000280078003800360029002F004D006900630072006F0073006F00660074002000530051004C0020005300650072007600650072002F003100310030002F0054006F006F006C0073002F00420069006E006E002F004D0061006E006100670065006D0065006E007400530074007500640069006F002F004900440045002F00500072006900760061007400650041007300730065006D0062006C006900650073002F004F0062006A006500630074004500780070006C006F007200650072005200000000000000000000000000000005000000540000002C0000002C0000002C00000034000000000000000000000002230000D1240000000000002D0100000D0000000C000000070000001C010000450600002805000094020000390300003A02000066030000DD040000EE020000DD040000360600003804000000000000010000004C0F00000804000000000000000000000000000002000000020000001C0100004506000000000000010000004C0F00000804000000000000000000000000000002000000020000001C0100004506000001000000000000004C0F00000804000000000000000000000000000002000000020000001C010000450600000000000000000000CB2400007C24000000000000000000000D00000004000000040000001C01000045060000710700009204000078563412040000006000000001000000010000000B000000000000000100000002000000030000000400000005000000060000000700000008000000090000000A00000004000000640062006F0000000C000000500072006F0063006500730073005400610073006B00000003000B00243900007A0D0000DE3F00007A0D0000DE3F000020CCFFFF0000000002000000F0F0F000000000000000000000000000000000000100000097000000000000008D4000008BF0FFFFAF1900005801000032000000010000020000AF19000058010000020000000000050000800800008001000000150001000000900144420100065461686F6D612B0046004B005F00410067006700720065006700610074006500440069006D0065006E00730069006F006E005F00450078007400720061006300740069006F006E0049006E0066006F0072006D006100740069006F006E0003000B0021720000E691000092B80000E691000092B80000A37F00000000000002000000F0F0F00000000000000000000000000000000000010000009B00000000000000E99E000095920000DF0E00005801000032000000010000020000DF0E000058010000020000000000050000800800008001000000150001000000900144420100065461686F6D61180046004B005F00500072006F0063006500730073005400610073006B005F0043006100740061006C006F0067007500650000020000F90C0000160900000200640000000500008000000000000000003A00010000009001C0D40100085365676F652055491E004E0065007600650072002000430041005300430041004400450020000D000A00440045004C0045005400450020000D000A0048006500720065002100214334120800000036220000472400007856341207000000140100004C006F00610064004D006F00640075006C00650041007300730065006D0062006C0079000000000000000000D800000003000000010000000001000000000000446174615F5053004D756C7469706C7952616469616C4772616469656E7443656E74657265645F5053315F436F6E7374616E745461626C6500666C48616C66546578656C53697A654E6F726D616C697A656400AB000003000100010001000000000000007D00000098000000050000000100010001000100A80000004C000000B0000000050000000100010001000100C00000004D756C7469706C7952616469616C4772616469656E7443656E74000000000000000000000000000005000000540000002C0000002C0000002C0000003400000000000000000000003622000047240000000000002D0100000D0000000C000000070000001C01000026070000DC05000094020000390300003A020000DE030000DD040000EE020000DD04000036060000380400000000000001000000D91000000804000000000000000000000000000002000000020000001C010000260700000000000001000000D91000000804000000000000000000000000000002000000020000001C010000260700000100000000000000D91000000804000000000000000000000000000002000000020000001C0100002607000000000000000000003D2900004724000000000000000000000D00000004000000040000001C010000260700007F0800003705000078563412040000006E00000001000000010000000B000000000000000100000002000000030000000400000005000000060000000700000008000000090000000A00000004000000640062006F000000130000004C006F00610064004D006F00640075006C00650041007300730065006D0062006C007900000021433412080000009E390000252700007856341207000000140100004C006F00610064005300630068006500640075006C00650000005300650072007600650072002E004200610074006300680050006100720073006500720043006C00690065006E0074002C002000560065007200730069006F006E003D00310031002E0030002E0030002E0030002C002000430075006C0074007500720065003D006E00650075007400720061006C002C0020005000750062006C00690063004B006500790054006F006B0065006E003D00380039003800340035006400630064003800300038003000630063003900310000000000000000000000000000000000000000000000000000000000000000000000000000000000000005000000540000002C0000002C0000002C0000003400000000000000000000009E39000025270000000000002D0100000D0000000C000000070000001C010000C50D0000DC05000094020000390300003A020000DE030000DD040000EE020000DD04000036060000380400000000000001000000104F0000B1240000000000000B0000000B00000002000000020000001C0100007B2A00000000000001000000D9100000AF08000000000000020000000200000002000000020000001C010000260700000100000000000000D91000000804000000000000000000000000000002000000020000001C0100002607000000000000000000003D2900004724000000000000000000000D00000004000000040000001C010000260700007F0800003705000078563412040000006200000001000000010000000B000000000000000100000002000000030000000400000005000000060000000700000008000000090000000A00000004000000640062006F0000000D0000004C006F00610064005300630068006500640075006C006500000021433412080000007829000010190000785634120700000014010000450078007400650072006E0061006C0044006100740061006200610073006500530065007200760065007200000000007501000000000000140000000000000000000000010000000400000075010000430000004700000004000000000000000600000004000000750100001400000047000000010000000300000005000000E4FFFFFF750100001400000047000000000000000300000005000000E8FFFFFF7501000014000000470000000200000003000000050000000800000075010000530000005C0100000400000000000000060000000800000075010000530000005C01000001000000030000000500000000000000000000000000000005000000540000002C0000002C0000002C0000003400000000000000000000007829000010190000000000002D0100000D0000000C000000070000001C01000026070000DC05000094020000390300003A020000DE030000DD040000EE020000DD04000036060000380400000000000001000000D91000000804000000000000000000000000000002000000020000001C010000260700000000000001000000D91000000804000000000000000000000000000002000000020000001C010000260700000100000000000000D91000000804000000000000000000000000000002000000020000001C0100002607000000000000000000003D2900004724000000000000000000000D00000004000000040000001C010000260700007F0800003705000078563412040000007600000001000000010000000B000000000000000100000002000000030000000400000005000000060000000700000008000000090000000A00000004000000640062006F00000017000000450078007400650072006E0061006C0044006100740061006200610073006500530065007200760065007200000003000B0036F7FFFFFC6C0000B4E2FFFFFC6C0000B4E2FFFF495900000000000002000000F0F0F0000000000000000000000000000000000001000000A40000000000000044CDFFFFAB6D00002D15000058010000320000000100000200002D15000058010000020000000000050000800800008001000000150001000000900144420100065461686F6D61230046004B005F005400610062006C00650049006E0066006F005F00450078007400650072006E0061006C004400610074006100620061007300650053006500720076006500720003000B004C1D00001E8300004C1D0000089D0000A8480000089D00000000000002000000F0F0F0000000000000000000000000000000000001000000A700000000000000F50F0000B79D00008415000058010000320000000100000200008415000058010000020000000000FFFFFF000800008001000000150001000000900144420100065461686F6D61230046004B005F0043006100740061006C006F006700750065005F00450078007400650072006E0061006C004400610074006100620061007300650053006500720076006500720003000B00701700001E830000701700004EA20000A84800004EA200000000000002000000F0F0F0000000000000000000000000000000000001000000A900000000000000B4090000FDA200003116000058010000320000000100000200003116000058010000020000000000FFFFFF000800008001000000150001000000900144420100065461686F6D61240046004B005F0043006100740061006C006F006700750065005F00450078007400650072006E0061006C0044006100740061006200610073006500530065007200760065007200310003000B0036F7FFFF546F00002EA5FFFF546F00002EA5FFFF122600000000000002000000F0F0F0000000000000000000000000000000000001000000AB00000000000000DDA5FFFF926800006715000058010000380000000100000200006715000058010000020000000000FFFFFF000800008001000000150001000000900144420100065461686F6D61220046004B005F0041004E004F005400610062006C0065005F00450078007400650072006E0061006C00440061007400610062006100730065005300650072007600650072002143341208000000C61B0000E61700007856341207000000140100004C006F006100640050006500720069006F0064006900630061006C006C0079000000440026004400570044004F0052004A005800480003001000030026004400570044004F0052004A005800480042002700440057004400420027004C0044004A00550044005000000080400000004100001041000000410000C040000040400000C0400000E04000008040000040400000E040000080400000C0400000E0400000E04000008040000000410000C040000080400000C040000000410000C040000080400000C040000040400000E0400000E0400000E0400000C040000040400000A04000004040000000410000000000000000000000000000000005000000540000002C0000002C0000002C000000340000000000000000000000C61B0000E6170000000000002D0100000D0000000C000000070000001C010000890D0000DC05000094020000390300003A020000DE030000DD040000EE020000DD04000036060000380400000000000001000000D91000000804000000000000000000000000000002000000020000001C010000260700000000000001000000D91000000804000000000000000000000000000002000000020000001C010000260700000100000000000000D91000000804000000000000000000000000000002000000020000001C0100002607000000000000000000003D2900004724000000000000000000000D00000004000000040000001C010000260700007F0800003705000078563412040000006A00000001000000010000000B000000000000000100000002000000030000000400000005000000060000000700000008000000090000000A00000004000000640062006F000000110000004C006F006100640050006500720069006F0064006900630061006C006C007900000002000B00B42D0000E4C1FFFFF8230000E4C1FFFF0000000002000000F0F0F0000000000000000000000000000000000001000000B0000000000000005F1B0000DDBFFFFFED1A00005801000032000000010000020000ED1A000058010000020000000000FFFFFF000800008001000000150001000000900144420100065461686F6D612C0046004B005F0041006700670072006500670061007400650043006F006E00660069006700750072006100740069006F006E005F00410067006700720065006700610074006500440069006D0065006E00730069006F006E002143341208000000E71B0000DF3500007856341207000000140100004C006F00610064004D0065007400610064006100740061000000954000000000000010400100000000000000000000000E000000050000001801000000000000000000000000000000000000F800000000000000050000000000000000000000020000000000000000C0954000000000000000000000000000C095400000000000000040040000002000000030000000000000000000000000B8954000000000000010400000000000000040000000000000004000000000000000000100000000000000050000000000000040000000010000000000000000000040000000000000104004000000200000002000000000000000000000000100000005000000540000002C0000002C0000002C0000003400000000000000000000003622000047240000000000002D0100000D0000000C000000070000001C01000026070000DC05000094020000390300003A020000DE030000DD040000EE020000DD04000036060000380400000000000001000000E71B0000DF35000000000000140000000C00000002000000020000001C0100007A0D00000000000001000000D91000006806000000000000010000000100000002000000020000001C010000260700000100000000000000D91000000804000000000000000000000000000002000000020000001C0100002607000000000000000000003D2900004724000000000000000000000D00000004000000040000001C010000260700007F0800003705000078563412040000006200000001000000010000000B000000000000000100000002000000030000000400000005000000060000000700000008000000090000000A00000004000000640062006F0000000D0000004C006F00610064004D006500740061006400610074006100000002000B00AA820000F654000021720000F65400000000000002000000F0F0F0000000000000000000000000000000000001000000BF0000000000000066720000EF520000FF0F00005801000032000000010000020000FF0F000058010000020000000000FFFFFF000800008001000000150001000000900144420100065461686F6D61190046004B005F0043006100740061006C006F006700750065005F004C006F00610064004D00650074006100640061007400610004000B00919E000086880000919E000084940000CA9E000084940000CA9E0000F69F00000200000002000000F0F0F0000000000000000000000000000000000001000000C100000000000000D98A0000069500000913000058010000320000000100000200000913000058010000020000000000FFFFFF000800008001000000150001000000900144420100065461686F6D61200046004B005F004C006F006100640050006500720069006F0064006900630061006C006C0079005F004C006F00610064004D00650074006100640061007400610002000B00919E00003A6B00009CAE00003A6B00000000000002000000F0F0F0000000000000000000000000000000000001000000C300000000000000079E0000E96B00001F11000058010000320000000100000200001F11000058010000020000000000FFFFFF000800008001000000150001000000900144420100065461686F6D611B0046004B005F00500072006F0063006500730073005400610073006B005F004C006F00610064004D00650074006100640061007400610004000B00919E0000E88000008EA90000E88000008EA90000D6830000D8BD0000D68300000000000002000000F0F0F0000000000000000000000000000000000001000000C50000000000000081AA0000CF810000E91100005801000032000000010000020000E911000058010000020000000000FFFFFF000800008001000000150001000000900144420100065461686F6D611C0046004B005F004C006F00610064005300630068006500640075006C0065005F004C006F00610064004D0065007400610064006100740061002143341208000000362200003A1A000078563412070000001401000044004C004500570069006E0064006F0077007300530065007200760069006300650045007800630065007000740069006F006E000000720073006500720043006C00690065006E0074002C002000560065007200730069006F006E003D00310031002E0030002E0030002E0030002C002000430075006C0074007500720065003D006E00650075007400720061006C002C0020005000750062006C00690063004B006500790054006F006B0065006E003D00380039003800340035006400630064003800300038003000630063003900310000000000000000000000000000000000000000000000000000000000000000000000000000000000000005000000540000002C0000002C0000002C000000340000000000000000000000362200003A1A0000000000002D010000080000000C000000070000001C01000026070000DC05000094020000390300003A020000DE030000DD040000EE020000DD04000036060000380400000000000001000000621700007B1A000000000000060000000600000002000000020000001C010000E60A00000000000001000000D91000006806000000000000010000000100000002000000020000001C010000260700000100000000000000D91000000804000000000000000000000000000002000000020000001C0100002607000000000000000000003D2900004724000000000000000000000D00000004000000040000001C010000260700007F0800003705000078563412040000007E00000001000000010000000B000000000000000100000002000000030000000400000005000000060000000700000008000000090000000A00000004000000640062006F0000001B00000044004C004500570069006E0064006F0077007300530065007200760069006300650045007800630065007000740069006F006E00000002000B00408300001389000040830000CA9E00000000000002000000F0F0F0000000000000000000000000000000000001000000C800000000000000FC67000042930000951A00005801000032000000010000020000951A000058010000020000000000FFFFFF000800008001000000150001000000900144420100065461686F6D612A0046004B005F0044004C004500570069006E0064006F0077007300530065007200760069006300650045007800630065007000740069006F006E005F004C006F00610064004D00650074006100640061007400610005000B00849900001389000084990000849400004C990000849400004C9900008CA000009E9D00008CA000000000000002000000F0F0F0000000000000000000000000000000000001000000CA00000000000000E7840000C9950000B61300005801000033000000010000020000B613000058010000020000000000FFFFFF000800008001000000150001000000900144420100065461686F6D61210046004B005F004C006F006100640050006500720069006F0064006900630061006C006C0079005F004C006F00610064004D00650074006100640061007400610031002143341208000000112E00007116000078563412070000001401000041006700670072006500670061007400650043006F006E00740069006E0075006F0075007300440061007400650041007800690073000000010000002CF19F123CF19F1204000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000A891B10B000400000000000074F19F120400000000000000000000000000000000000000000000000000000080457D121884AC0B00000000AA550000E0F09F1210000000604578120C0000000500000094DF7B007CDF7B00C4F19F1200000000D5FFAA55001000000C00000000000000A891B10B0000000000000000000000000000000005000000540000002C0000002C0000002C000000340000000000000000000000112E000071160000000000002D0100000D0000000C000000070000001C010000F7080000DC05000094020000390300003A020000DE030000DD040000EE020000DD04000036060000380400000000000001000000D91000000804000000000000000000000000000002000000020000001C010000260700000000000001000000D91000000804000000000000000000000000000002000000020000001C010000260700000100000000000000D91000000804000000000000000000000000000002000000020000001C0100002607000000000000000000003D2900004724000000000000000000000D00000004000000040000001C010000260700007F0800003705000078563412040000008000000001000000010000000B000000000000000100000002000000030000000400000005000000060000000700000008000000090000000A00000004000000640062006F0000001C00000041006700670072006500670061007400650043006F006E00740069006E0075006F007500730044006100740065004100780069007300000002000B00F843000020CCFFFFF8430000F4CFFFFF0200000002000000F0F0F0000000000000000000000000000000000001000000CD00000000000000C22400005ECDFFFF871E00005801000032000000010000020000871E000058010000020000000000FFFFFF000800008001000000150001000000900144420100065461686F6D61310046004B005F0041006700670072006500670061007400650043006F006E00740069006E0075006F0075007300440061007400650041007800690073005F00410067006700720065006700610074006500440069006D0065006E00730069006F006E0021433412080000003D2200006910000078563412070000001401000041006700670072006500670061007400650046006F0072006300650064004A006F0069006E00000000000000D800000003000000010000000001000000000000446174615F5053004D756C7469706C7952616469616C4772616469656E7443656E74657265645F5053315F436F6E7374616E745461626C6500666C48616C66546578656C53697A654E6F726D616C697A656400AB000003000100010001000000000000007D00000098000000050000000100010001000100A80000004C000000B0000000050000000100010001000100C00000004D756C7469706C7952616469616C4772616469656E7443656E74000000000000000000000000000005000000540000002C0000002C0000002C0000003400000000000000000000003D22000069100000000000002D0100000D0000000C000000070000001C01000015090000DC05000094020000390300003A020000DE030000DD040000EE020000DD04000036060000380400000000000001000000D91000000804000000000000000000000000000002000000020000001C010000260700000000000001000000D91000000804000000000000000000000000000002000000020000001C010000260700000100000000000000D91000000804000000000000000000000000000002000000020000001C0100002607000000000000000000003D2900004724000000000000000000000D00000004000000040000001C010000260700007F0800003705000078563412040000007000000001000000010000000B000000000000000100000002000000030000000400000005000000060000000700000008000000090000000A00000004000000640062006F0000001400000041006700670072006500670061007400650046006F0072006300650064004A006F0069006E00000003000B0072060000CDCBFFFF720600005ECFFFFFB80B00005ECFFFFF0000000002000000F0F0F0000000000000000000000000000000000001000000D20000000000000064EAFFFFE9CFFFFF5F1B000058010000270000000100000200005F1B000058010000020000000000FFFFFF000800008001000000150001000000900144420100065461686F6D612D0046004B005F0041006700670072006500670061007400650046006F0072006300650064004A006F0069006E005F0041006700670072006500670061007400650043006F006E00660069006700750072006100740069006F006E0004000B00AEF9FFFF263400008707000026340000870700009ADEFFFFB80B00009ADEFFFF0000000002000000F0F0F0000000000000000000000000000000000001000000D40000000000000036080000241600007C130000580100002B0000000100000200007C13000058010000020000000000FFFFFF000800008001000000150001000000900144420100065461686F6D61200046004B005F0041006700670072006500670061007400650046006F0072006300650064004A006F0069006E005F005400610062006C00650049006E0066006F0007000B00A8480000B42D0000F2430000B42D0000F243000020280000BC3C000020280000BC3C00005DCDFFFF282300005DCDFFFF28230000CDCBFFFF0000000002000000F0F0F0000000000000000000000000000000000001000000D6000000000000006B3D00003DECFFFF4B15000058010000380000000100000200004B15000058010000020000000000FFFFFF000800008001000000150001000000900144420100065461686F6D61230046004B005F0041006700670072006500670061007400650043006F006E00660069006700750072006100740069006F006E005F0043006100740061006C006F006700750065000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001000000FEFFFFFFFEFFFFFF0400000005000000060000000700000008000000090000000A0000000B0000000C0000000D0000000E0000000F000000100000001100000012000000130000001400000015000000160000001700000018000000190000001A0000001B0000001C0000001D0000001E0000001F00000020000000210000002200000023000000240000002500000026000000270000002800000029000000FEFFFFFFFEFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF0100FEFF030A0000FFFFFFFF00000000000000000000000000000000170000004D6963726F736F66742044445320466F726D20322E300010000000456D626564646564204F626A6563740000000000F439B271000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000010003000000000000000C0000000B0000004E61BC00000000000000000000000000000000000000000000000000000000000000000000000000000000000000DBE6B0E91C81D011AD5100A0C90F573900000200D062F43BF38BD0010202000010484500000000000000000000000000000000007A0100004400610074006100200053006F0075007200630065003D006A0061006E00750073003B0049006E0069007400690061006C00200043006100740061006C006F0067003D0053007000720069006E00740046006F0075007200440061007400610043006100740061006C006F006700750065003B0049006E00740065006700720061007400650064002000530065006300750072006900740079003D0054007200750065003B004D0075006C007400690070006C00650041006300740069007600650052006500730075006C00740053006500740073003D00460061006C00730065003B005000610063006B00650074002000530069007A0065003D0034003000390036003B0041000300440064007300530074007200650061006D000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000160002000300000006000000FFFFFFFF0000000000000000000000000000000000000000000000000000000000000000000000005E0000007A6500000000000053006300680065006D00610020005500440056002000440065006600610075006C0074000000000000000000000000000000000000000000000000000000000026000200FFFFFFFFFFFFFFFFFFFFFFFF000000000000000000000000000000000000000000000000000000000000000000000000020000001600000000000000440053005200450046002D0053004300480045004D0041002D0043004F004E00540045004E0054005300000000000000000000000000000000000000000000002C0002010500000007000000FFFFFFFF00000000000000000000000000000000000000000000000000000000000000000000000003000000A40900000000000053006300680065006D00610020005500440056002000440065006600610075006C007400200050006F007300740020005600360000000000000000000000000036000200FFFFFFFFFFFFFFFFFFFFFFFF0000000000000000000000000000000000000000000000000000000000000000000000002A00000012000000000000008100000082000000830000008400000085000000860000008700000088000000890000008A0000008B0000008C0000008D0000008E0000008F00000090000000FEFFFFFF92000000930000009400000095000000FEFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF0C0000000CCFFFFF7289FFFF0100260000007300630068005F006C006100620065006C0073005F00760069007300690062006C0065000000010000000B0000001E000000000000000000000000000000000000006400000000000000000000000000000000000000000000000000010000000100000000000000000000000000000000000000D00200000600280000004100630074006900760065005400610062006C00650056006900650077004D006F006400650000000100000008000400000030000000200000005400610062006C00650056006900650077004D006F00640065003A00300000000100000008003A00000034002C0030002C003200380034002C0030002C0032003200390035002C0031002C0031003800370035002C0035002C0031003200340035000000200000005400610062006C00650056006900650077004D006F00640065003A00310000000100000008001E00000032002C0030002C003200380034002C0030002C0032003700390030000000200000005400610062006C00650056006900650077004D006F00640065003A00320000000100000008001E00000032002C0030002C003200380034002C0030002C0032003200390035000000200000005400610062006C00650056006900650077004D006F00640065003A00330000000100000008001E00000032002C0030002C003200380034002C0030002C0032003200390035000000200000005400610062006C00650056006900650077004D006F00640065003A00340000000100000008003E00000034002C0030002C003200380034002C0030002C0032003200390035002C00310032002C0032003700310035002C00310031002C0031003600360035000000020000000200000000000000000000000000000000000000D00200000600280000004100630074006900760065005400610062006C00650056006900650077004D006F006400650000000100000008000400000030000000200000005400610062006C00650056006900650077004D006F00640065003A00300000000100000008003A00000034002C0030002C003200380034002C0030002C0033003300370035002C0031002C0031003800370035002C0035002C0031003200340035000000200000005400610062006C00650056006900650077004D006F00640065003A00310000000100000008001E00000032002C0030002C003200380034002C0030002C0032003700370035000000200000005400610062006C00650056006900650077004D006F00640065003A00320000000100000008001E00000032002C0030002C003200380034002C0030002C0032003200390035000000200000005400610062006C00650056006900650077004D006F00640065003A00330000000100000008001E00000032002C0030002C003200380034002C0030002C0032003200390035000000200000005400610062006C00650056006900650077004D006F00640065003A00340000000100000008003E00000034002C0030002C003200380034002C0030002C0032003200390035002C00310032002C0032003700310035002C00310031002C0031003600360035000000030000000300000000000000540000000103737401000000640062006F00000046004B005F0043006100740061006C006F006700750065005F004900740065006D0073005F0044006100740061005F0043006100740061006C006F0067007500650000000000000000000000C40200000000040000000400000003000000080000000100650DC800650D0000000000000000AD070000000000050000000500000000000000000000000000000000000000D00200000600280000004100630074006900760065005400610062006C00650056006900650077004D006F006400650000000100000008000400000030000000200000005400610062006C00650056006900650077004D006F00640065003A00300000000100000008003A00000034002C0030002C003200380034002C0030002C0032003200390035002C0031002C0031003800370035002C0035002C0031003200340035000000200000005400610062006C00650056006900650077004D006F00640065003A00310000000100000008001E00000032002C0030002C003200380034002C0030002C0033003500370030000000200000005400610062006C00650056006900650077004D006F00640065003A00320000000100000008001E00000032002C0030002C003200380034002C0030002C0032003200390035000000200000005400610062006C00650056006900650077004D006F00640065003A00330000000100000008001E00000032002C0030002C003200380034002C0030002C0032003200390035000000200000005400610062006C00650056006900650077004D006F00640065003A00340000000100000008003E00000034002C0030002C003200380034002C0030002C0032003200390035002C00310032002C0032003700310035002C00310031002C0031003600360035000000060000000600000000000000000000000000000000000000D00200000600280000004100630074006900760065005400610062006C00650056006900650077004D006F006400650000000100000008000400000030000000200000005400610062006C00650056006900650077004D006F00640065003A00300000000100000008003A00000034002C0030002C003200380034002C0030002C0032003800300035002C0031002C0031003800370035002C0035002C0031003200340035000000200000005400610062006C00650056006900650077004D006F00640065003A00310000000100000008001E00000032002C0030002C003200380034002C0030002C0032003700390030000000200000005400610062006C00650056006900650077004D006F00640065003A00320000000100000008001E00000032002C0030002C003200380034002C0030002C0032003200390035000000200000005400610062006C00650056006900650077004D006F00640065003A00330000000100000008001E00000032002C0030002C003200380034002C0030002C0032003200390035000000200000005400610062006C00650056006900650077004D006F00640065003A00340000000100000008003E00000034002C0030002C003200380034002C0030002C0032003200390035002C00310032002C0032003700310035002C00310031002C00310036003600350000000700000007000000000000004600000001038A7601000000640062006F00000046004B005F005400610062006C0065005F004900740065006D0073005F0044006100740061005F005400610062006C006500730000000000000000000000C40200000000080000000800000007000000080000000100650D8800650D0000000000000000AD0700000000000D0000000D00000000000000000000000000000000000000D00200000600280000004100630074006900760065005400610062006C00650056006900650077004D006F006400650000000100000008000400000030000000200000005400610062006C00650056006900650077004D006F00640065003A00300000000100000008003A00000034002C0030002C003200380034002C0030002C0032003200390035002C0031002C0031003800370035002C0035002C0031003200340035000000200000005400610062006C00650056006900650077004D006F00640065003A00310000000100000008001E00000032002C0030002C003200380034002C0030002C0032003200390035000000200000005400610062006C00650056006900650077004D006F00640065003A00320000000100000008001E00000032002C0030002C003200380034002C0030002C0032003200390035000000200000005400610062006C00650056006900650077004D006F00640065003A00330000000100000008001E00000032002C0030002C003200380034002C0030002C0032003200390035000000200000005400610062006C00650056006900650077004D006F00640065003A00340000000100000008003E00000034002C0030002C003200380034002C0030002C0032003200390035002C00310032002C0032003700310035002C00310031002C00310036003600350000001200000012000000000000005E00000001FF5F5E01000000640062006F00000046004B005F0043006F006C0075006D006E0049006E0066006F005F0043006100740061006C006F006700750065004900740065006D005F0043006F006C0075006D006E0049006E0066006F0000000000000000000000C40200000000130000001300000012000000080000000100650D4800650D0000000000000000AD0700000000001600000016000000000000006400000001016F0001000000640062006F00000046004B005F0043006F006C0075006D006E0049006E0066006F005F0043006100740061006C006F006700750065004900740065006D005F0043006100740061006C006F006700750065004900740065006D0000000000000000000000C40200000000170000001700000016000000080000000100650D0800650D0000000000000000AD070000000000180000001800000000000000000000000000000000000000D00200000600280000004100630074006900760065005400610062006C00650056006900650077004D006F006400650000000100000008000400000030000000200000005400610062006C00650056006900650077004D006F00640065003A00300000000100000008003800000034002C0030002C003200380034002C0030002C0031003800330030002C0031002C0031003500300030002C0035002C003900390030000000200000005400610062006C00650056006900650077004D006F00640065003A00310000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00320000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00330000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00340000000100000008003E00000034002C0030002C003200380034002C0030002C0031003800330030002C00310032002C0032003100370035002C00310031002C00310033003300350000001900000019000000000000005000000001FF5F5E01000000640062006F00000046004B005F0053007500700070006F007200740069006E00670044006F00630075006D0065006E0074005F0043006100740061006C006F0067007500650000000000000000000000C402000000001A0000001A000000190000000800000001FF640DC8FF640D0000000000000000AD0700000000001B0000001B00000000000000000000000000000000000000D00200000600280000004100630074006900760065005400610062006C00650056006900650077004D006F006400650000000100000008000400000030000000200000005400610062006C00650056006900650077004D006F00640065003A00300000000100000008003800000034002C0030002C003200380034002C0030002C0032003400300030002C0031002C0031003500300030002C0035002C003900390030000000200000005400610062006C00650056006900650077004D006F00640065003A00310000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00320000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00330000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00340000000100000008003E00000034002C0030002C003200380034002C0030002C0031003800330030002C00310032002C0032003100370035002C00310031002C0031003300330035000000220000002200000000000000740000000102000001000000640062006F00000046004B005F0043006F006C0075006D006E0049006E0066006F005F0043006100740061006C006F006700750065004900740065006D005F00450078007400720061006300740069006F006E0049006E0066006F0072006D006100740069006F006E0000000000000000000000C402000000002300000023000000220000000800000001FF640D88FF640D0000000000000000AD070000000000240000002400000000000000000000000000000000000000D00200000600280000004100630074006900760065005400610062006C00650056006900650077004D006F006400650000000100000008000400000030000000200000005400610062006C00650056006900650077004D006F00640065003A00300000000100000008003A00000034002C0030002C003200380034002C0030002C0032003000370030002C0031002C0031003600390035002C0035002C0031003100320035000000200000005400610062006C00650056006900650077004D006F00640065003A00310000000100000008001E00000032002C0030002C003200380034002C0030002C0032003000370030000000200000005400610062006C00650056006900650077004D006F00640065003A00320000000100000008001E00000032002C0030002C003200380034002C0030002C0032003000370030000000200000005400610062006C00650056006900650077004D006F00640065003A00330000000100000008001E00000032002C0030002C003200380034002C0030002C0032003000370030000000200000005400610062006C00650056006900650077004D006F00640065003A00340000000100000008003E00000034002C0030002C003200380034002C0030002C0032003000370030002C00310032002C0032003400340035002C00310031002C00310035003000300000002700000027000000000000003A0000000106000001000000640062006F00000046004B005F004C006F006F006B00750070005F0043006F006C0075006D006E0049006E0066006F0000000000000000000000C402000000002800000028000000270000000800000001FF640D48FF640D0000000000000000AD0700000000002900000029000000000000003C0000000107000001000000640062006F00000046004B005F004C006F006F006B00750070005F0043006F006C0075006D006E0049006E0066006F00310000000000000000000000C402000000002A0000002A000000290000000800000001FF640D08FF640D0000000000000000AD0700000000002B0000002B000000000000003C0000000105000001000000640062006F00000046004B005F004C006F006F006B00750070005F0043006F006C0075006D006E0049006E0066006F00320000000000000000000000C402000000002C0000002C0000002B0000000800000001FE640DC8FE640D0000000000000000AD0700000000002F0000002F00000000000000000000000000000000000000D00200000600280000004100630074006900760065005400610062006C00650056006900650077004D006F006400650000000100000008000400000030000000200000005400610062006C00650056006900650077004D006F00640065003A00300000000100000008003A00000034002C0030002C003200380034002C0030002C0032003000370030002C0031002C0031003600320030002C0035002C0031003100320035000000200000005400610062006C00650056006900650077004D006F00640065003A00310000000100000008001E00000032002C0030002C003200380034002C0030002C0032003000370030000000200000005400610062006C00650056006900650077004D006F00640065003A00320000000100000008001E00000032002C0030002C003200380034002C0030002C0032003000370030000000200000005400610062006C00650056006900650077004D006F00640065003A00330000000100000008001E00000032002C0030002C003200380034002C0030002C0032003000370030000000200000005400610062006C00650056006900650077004D006F00640065003A00340000000100000008003E00000034002C0030002C003200380034002C0030002C0032003000370030002C00310032002C0032003400340035002C00310031002C00310035003000300000003300000033000000000000005000000001FF5F5E01000000640062006F00000046004B005F004A006F0069006E0049006E0066006F005F0043006F006C0075006D006E0049006E0066006F005F004A006F0069006E004B0065007900310000000000000000000000C402000000003400000034000000330000000800000001FE640D88FE640D0000000000000000AD0700000000003500000035000000000000005000000001FF5F5E01000000640062006F00000046004B005F004A006F0069006E0049006E0066006F005F0043006F006C0075006D006E0049006E0066006F005F004A006F0069006E004B0065007900320000000000000000000000C402000000003600000036000000350000000800000001FE640D48FE640D0000000000000000AD070000000000380000003800000000000000000000000000000000000000D00200000600280000004100630074006900760065005400610062006C00650056006900650077004D006F006400650000000100000008000400000030000000200000005400610062006C00650056006900650077004D006F00640065003A00300000000100000008003A00000034002C0030002C003200380034002C0030002C0032003000350035002C0031002C0031003100370030002C0035002C0031003000360035000000200000005400610062006C00650056006900650077004D006F00640065003A00310000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00320000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00330000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00340000000100000008003E00000034002C0030002C003200380034002C0030002C0031003800330030002C00310032002C0032003100370035002C00310031002C00310033003300350000003900000039000000000000006400000001006F0001000000640062006F00000046004B005F00450078007400720061006300740069006F006E00460069006C007400650072005F00450078007400720061006300740069006F006E0049006E0066006F0072006D006100740069006F006E0000000000000000000000C402000000003A0000003A000000390000000800000001FE640D08FE640D0000000000000000AD0700000000003B0000003B00000000000000000000000000000000000000D00200000600280000004100630074006900760065005400610062006C00650056006900650077004D006F006400650000000100000008000400000030000000200000005400610062006C00650056006900650077004D006F00640065003A00300000000100000008003A00000034002C0030002C003200380034002C0030002C0031003500340035002C0031002C0031003100370030002C0035002C0031003000360035000000200000005400610062006C00650056006900650077004D006F00640065003A00310000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00320000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00330000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00340000000100000008003E00000034002C0030002C003200380034002C0030002C0031003800330030002C00310032002C0032003100370035002C00310031002C00310033003300350000003C0000003C000000000000006C000000011A5F5E01000000640062006F00000046004B005F00450078007400720061006300740069006F006E00460069006C0074006500720050006100720061006D0065007400650072005F00450078007400720061006300740069006F006E00460069006C0074006500720000000000000000000000C402000000003D0000003D0000003C0000000800000001FD640DC8FD640D0000000000000000AD070000000000400000004000000000000000560000000103737401000000640062006F00000046004B005F0043006100740061006C006F006700750065005F00450078007400720061006300740069006F006E0049006E0066006F0072006D006100740069006F006E0000000000000000000000C402000000004100000041000000400000000800000001FD640D88FD640D0000000000000000AD0700000000004C0000004C00000000000000000000000000000000000000D00200000600280000004100630074006900760065005400610062006C00650056006900650077004D006F006400650000000100000008000400000030000000200000005400610062006C00650056006900650077004D006F00640065003A00300000000100000008003800000034002C0030002C003200380034002C0030002C0031003800330030002C0031002C0031003500300030002C0035002C003900390030000000200000005400610062006C00650056006900650077004D006F00640065003A00310000000100000008001E00000032002C0030002C003200380034002C0030002C0035003000370030000000200000005400610062006C00650056006900650077004D006F00640065003A00320000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00330000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00340000000100000008003E00000034002C0030002C003200380034002C0030002C0031003800330030002C00310032002C0032003100370035002C00310031002C0031003300330035000000540000005400000000000000540000000103737401000000640062006F00000046004B005F004C006F006F006B007500700043006F006D0070006F0073006900740065004A006F0069006E0049006E0066006F005F004C006F006F006B007500700000000000000000000000C402000000005500000055000000540000000800000001FD640D48FD640D0000000000000000AD0700000000005600000056000000000000005C00000001FF5F5E01000000640062006F00000046004B005F004C006F006F006B007500700043006F006D0070006F0073006900740065004A006F0069006E0049006E0066006F005F0043006F006C0075006D006E0049006E0066006F0000000000000000000000C402000000005700000057000000560000000800000001FD640D08FD640D0000000000000000AD0700000000005900000059000000000000006200000001006F0001000000640062006F00000046004B005F004C006F006F006B007500700043006F006D0070006F0073006900740065004A006F0069006E0049006E0066006F005F0043006F006C0075006D006E0049006E0066006F005F0046004B0000000000000000000000C402000000005A0000005A000000590000000800000001FC640DC8FC640D0000000000000000AD0700000000005C0000005C00000000000000000000000000000000000000D00200000600280000004100630074006900760065005400610062006C00650056006900650077004D006F006400650000000100000008000400000030000000200000005400610062006C00650056006900650077004D006F00640065003A00300000000100000008003800000034002C0030002C003200380034002C0030002C0031003800330030002C0031002C0031003500300030002C0035002C003900390030000000200000005400610062006C00650056006900650077004D006F00640065003A00310000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00320000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00330000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00340000000100000008003E00000034002C0030002C003200380034002C0030002C0031003800330030002C00310032002C0032003100370035002C00310031002C00310033003300350000005D0000005D000000000000005000000001FF5F5E01000000640062006F00000046004B005F0053007500700070006F007200740069006E006700530051004C005400610062006C0065005F0043006100740061006C006F0067007500650000000000000000000000C402000000005E0000005E0000005D0000000800000001FC640D88FC640D0000000000000000AD0700000000005F0000005F00000000000000000000000000000000000000D00200000600280000004100630074006900760065005400610062006C00650056006900650077004D006F006400650000000100000008000400000030000000200000005400610062006C00650056006900650077004D006F00640065003A00300000000100000008003800000034002C0030002C003200380034002C0030002C0031003800330030002C0031002C0031003500300030002C0035002C003900390030000000200000005400610062006C00650056006900650077004D006F00640065003A00310000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00320000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00330000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00340000000100000008003E00000034002C0030002C003200380034002C0030002C0031003800330030002C00310032002C0032003100370035002C00310031002C0031003300330035000000600000006000000000000000000000000000000000000000D00200000600280000004100630074006900760065005400610062006C00650056006900650077004D006F006400650000000100000008000400000030000000200000005400610062006C00650056006900650077004D006F00640065003A00300000000100000008003800000034002C0030002C003200380034002C0030002C0032003600340030002C0031002C0031003500300030002C0035002C003900390030000000200000005400610062006C00650056006900650077004D006F00640065003A00310000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00320000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00330000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00340000000100000008003E00000034002C0030002C003200380034002C0030002C0031003800330030002C00310032002C0032003100370035002C00310031002C0031003300330035000000630000006300000000000000000000000000000000000000D00200000600280000004100630074006900760065005400610062006C00650056006900650077004D006F006400650000000100000008000400000030000000200000005400610062006C00650056006900650077004D006F00640065003A00300000000100000008003800000034002C0030002C003200380034002C0030002C0033003100320030002C0031002C0031003500300030002C0035002C003900390030000000200000005400610062006C00650056006900650077004D006F00640065003A00310000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00320000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00330000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00340000000100000008003E00000034002C0030002C003200380034002C0030002C0031003800330030002C00310032002C0032003100370035002C00310031002C0031003300330035000000640000006400000000000000800000000106401001000000640062006F00000046004B005F00410067006700720065006700610074006500460069006C0074006500720053007500620043006F006E007400610069006E00650072005F00410067006700720065006700610074006500460069006C0074006500720043006F006E007400610069006E006500720000000000000000000000C402000000006500000065000000640000000800000001FC640D48FC640D0000000000000000AD0700000000006600000066000000000000008200000001FF690001000000640062006F00000046004B005F00410067006700720065006700610074006500460069006C0074006500720053007500620043006F006E007400610069006E00650072005F00410067006700720065006700610074006500460069006C0074006500720043006F006E007400610069006E0065007200310000000000000000000000C402000000006700000067000000660000000800000001F57115C8F571150000000000000000AD0700000000006E0000006E00000000000000000000000000000000000000D00200000600280000004100630074006900760065005400610062006C00650056006900650077004D006F006400650000000100000008000400000030000000200000005400610062006C00650056006900650077004D006F00640065003A00300000000100000008003800000034002C0030002C003200380034002C0030002C0031003800330030002C0031002C0031003500300030002C0035002C003900390030000000200000005400610062006C00650056006900650077004D006F00640065003A00310000000100000008001E00000032002C0030002C003200380034002C0030002C0034003600350030000000200000005400610062006C00650056006900650077004D006F00640065003A00320000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00330000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00340000000100000008003E00000034002C0030002C003200380034002C0030002C0031003800330030002C00310032002C0032003100370035002C00310031002C00310033003300350000006F0000006F000000000000006800000001006F0001000000640062006F00000046004B005F00410067006700720065006700610074006500460069006C007400650072005F00410067006700720065006700610074006500460069006C0074006500720043006F006E007400610069006E006500720000000000000000000000C4020000000070000000700000006F0000000800000001F5711588F571150000000000000000AD070000000000710000007100000000000000000000000000000000000000D00200000600280000004100630074006900760065005400610062006C00650056006900650077004D006F006400650000000100000008000400000030000000200000005400610062006C00650056006900650077004D006F00640065003A00300000000100000008003800000034002C0030002C003200380034002C0030002C0032003600350035002C0031002C0031003500300030002C0035002C003900390030000000200000005400610062006C00650056006900650077004D006F00640065003A00310000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00320000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00330000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00340000000100000008003E00000034002C0030002C003200380034002C0030002C0031003800330030002C00310032002C0032003100370035002C00310031002C00310033003300350000007200000072000000000000006A000000011A5F5E01000000640062006F00000046004B005F00410067006700720065006700610074006500440069006D0065006E00730069006F006E005F0041006700670072006500670061007400650043006F006E00660069006700750072006100740069006F006E0000000000000000000000C402000000007300000073000000720000000800000001F5711548F571150000000000000000AD070000000000740000007400000000000000760000000102000001000000640062006F00000046004B005F0041006700670072006500670061007400650043006F006E00660069006700750072006100740069006F006E005F00410067006700720065006700610074006500460069006C0074006500720043006F006E007400610069006E006500720000000000000000000000C402000000007500000075000000740000000800000001F5711508F571150000000000000000AD070000000000760000007600000000000000000000000000000000000000D00200000600280000004100630074006900760065005400610062006C00650056006900650077004D006F006400650000000100000008000400000030000000200000005400610062006C00650056006900650077004D006F00640065003A00300000000100000008003800000034002C0030002C003200380034002C0030002C0031003800330030002C0031002C0031003500300030002C0035002C003900390030000000200000005400610062006C00650056006900650077004D006F00640065003A00310000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00320000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00330000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00340000000100000008003E00000034002C0030002C003200380034002C0030002C0031003800330030002C00310032002C0032003100370035002C00310031002C00310033003300350000007700000077000000000000006800000001016F0001000000640062006F00000046004B005F00410067006700720065006700610074006500460069006C0074006500720050006100720061006D0065007400650072005F00410067006700720065006700610074006500460069006C0074006500720000000000000000000000C402000000007800000078000000770000000800000001F47115C8F471150000000000000000AD0700000000007C0000007C00000000000000000000000000000000000000D00200000600280000004100630074006900760065005400610062006C00650056006900650077004D006F006400650000000100000008000400000030000000200000005400610062006C00650056006900650077004D006F00640065003A00300000000100000008003800000034002C0030002C003200380034002C0030002C0033003100380030002C0031002C0031003500300030002C0035002C003900390030000000200000005400610062006C00650056006900650077004D006F00640065003A00310000000100000008001E00000032002C0030002C003200380034002C0030002C0034003900350030000000200000005400610062006C00650056006900650077004D006F00640065003A00320000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00330000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00340000000100000008003E00000034002C0030002C003200380034002C0030002C0031003800330030002C00310032002C0032003100370035002C00310031002C0031003300330035000000810000008100000000000000580000000103737401000000640062006F00000046004B005F0043006100740061006C006F006700750065004900740065006D00490073007300750065005F0043006100740061006C006F006700750065004900740065006D0000000000000000000000C402000000008200000082000000810000000800000001F4711588F471150000000000000000AD070000000000830000008300000000000000000000000000000000000000D00200000600280000004100630074006900760065005400610062006C00650056006900650077004D006F006400650000000100000008000400000030000000200000005400610062006C00650056006900650077004D006F00640065003A00300000000100000008003800000034002C0030002C003200380034002C0030002C0031003800330030002C0031002C0031003500300030002C0035002C003900390030000000200000005400610062006C00650056006900650077004D006F00640065003A00310000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00320000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00330000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00340000000100000008003E00000034002C0030002C003200380034002C0030002C0031003800330030002C00310032002C0032003100370035002C00310031002C00310033003300350000008400000084000000000000006800000001016F0001000000640062006F00000046004B005F0043006100740061006C006F006700750065004900740065006D00490073007300750065005F004F0077006E00650072005F0049007300730075006500530079007300740065006D00550073006500720000000000000000000000C402000000008500000085000000840000000800000001F4711548F471150000000000000000AD0700000000008600000086000000000000006E000000011A5F5E01000000640062006F00000046004B005F0043006100740061006C006F006700750065004900740065006D00490073007300750065005F005200650070006F0072007400650072005F0049007300730075006500530079007300740065006D00550073006500720000000000000000000000C402000000008700000087000000860000000800000001F4711508F471150000000000000000AD070000000000880000008800000000000000000000000000000000000000D00200000600280000004100630074006900760065005400610062006C00650056006900650077004D006F006400650000000100000008000400000030000000200000005400610062006C00650056006900650077004D006F00640065003A00300000000100000008003800000034002C0030002C003200380034002C0030002C0031003800330030002C0031002C0031003500300030002C0035002C003900390030000000200000005400610062006C00650056006900650077004D006F00640065003A00310000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00320000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00330000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00340000000100000008003E00000034002C0030002C003200380034002C0030002C0031003800330030002C00310032002C0032003100370035002C00310031002C00310033003300350000008B0000008B000000000000003E0000000105000001000000640062006F00000046004B005F0043006F006C0075006D006E0049006E0066006F005F0041004E004F005400610062006C00650000000000000000000000C402000000008C0000008C0000008B0000000800000001F37115C8F371150000000000000000AD0700000000008D0000008D00000000000000000000000000000000000000D00200000600280000004100630074006900760065005400610062006C00650056006900650077004D006F006400650000000100000008000400000030000000200000005400610062006C00650056006900650077004D006F00640065003A00300000000100000008003800000034002C0030002C003200380034002C0030002C0032003100330030002C0031002C0031003500300030002C0035002C003900390030000000200000005400610062006C00650056006900650077004D006F00640065003A00310000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00320000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00330000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00340000000100000008003E00000034002C0030002C003200380034002C0030002C0031003800330030002C00310032002C0032003100370035002C00310031002C00310033003300350000008E0000008E00000000000000580000000102737401000000640062006F00000046004B005F005000720065004C006F006100640044006900730063006100720064006500640043006F006C0075006D006E005F005400610062006C00650049006E0066006F0000000000000000000000C402000000008F0000008F0000008E0000000800000001F3711588F371150000000000000000AD070000000000900000009000000000000000000000000000000000000000D00200000600280000004100630074006900760065005400610062006C00650056006900650077004D006F006400650000000100000008000400000030000000200000005400610062006C00650056006900650077004D006F00640065003A00300000000100000008003800000034002C0030002C003200380034002C0030002C0031003600300035002C0031002C0031003300320030002C0035002C003800370030000000200000005400610062006C00650056006900650077004D006F00640065003A00310000000100000008001E00000032002C0030002C003200380034002C0030002C0031003600300035000000200000005400610062006C00650056006900650077004D006F00640065003A00320000000100000008001E00000032002C0030002C003200380034002C0030002C0031003600300035000000200000005400610062006C00650056006900650077004D006F00640065003A00330000000100000008001E00000032002C0030002C003200380034002C0030002C0031003600300035000000200000005400610062006C00650056006900650077004D006F00640065003A00340000000100000008003E00000034002C0030002C003200380034002C0030002C0031003600300035002C00310032002C0031003900300035002C00310031002C0031003100370030000000910000009100000000000000000000000000000000000000D00200000600280000004100630074006900760065005400610062006C00650056006900650077004D006F006400650000000100000008000400000030000000200000005400610062006C00650056006900650077004D006F00640065003A00300000000100000008003A00000034002C0030002C003200380034002C0030002C0032003200390035002C0031002C0031003800370035002C0035002C0031003200340035000000200000005400610062006C00650056006900650077004D006F00640065003A00310000000100000008001E00000032002C0030002C003200380034002C0030002C0032003200390035000000200000005400610062006C00650056006900650077004D006F00640065003A00320000000100000008001E00000032002C0030002C003200380034002C0030002C0032003200390035000000200000005400610062006C00650056006900650077004D006F00640065003A00330000000100000008001E00000032002C0030002C003200380034002C0030002C0032003200390035000000200000005400610062006C00650056006900650077004D006F00640065003A00340000000100000008003E00000034002C0030002C003200380034002C0030002C0032003200390035002C00310032002C0032003700310035002C00310031002C0031003600360035000000940000009400000000000000560000000101737401000000640062006F00000046004B005F00500072006F0063006500730073005400610073006B0041007200670075006D0065006E0074005F00500072006F0063006500730073005400610073006B0000000000000000000000C402000000009500000095000000940000000800000001F3711548F371150000000000000000AD0700000000009600000096000000000000006800000001016F0001000000640062006F00000046004B005F00410067006700720065006700610074006500440069006D0065006E00730069006F006E005F00450078007400720061006300740069006F006E0049006E0066006F0072006D006100740069006F006E0000000000000000000000C402000000009700000097000000960000000800000001F3711508F371150000000000000000AD0700000000009A0000009A000000000000004200000001038A7601000000640062006F00000046004B005F00500072006F0063006500730073005400610073006B005F0043006100740061006C006F0067007500650000000000000000000000C402000000009B0000009B0000009A0000000800000001F27115C8F271150000000000000000AD0700000000009C0000009C000000000000000800000001F2711588F271150000000000000000E40700000000009D0000009D00000000000000000000000000000000000000D00200000600280000004100630074006900760065005400610062006C00650056006900650077004D006F006400650000000100000008000400000030000000200000005400610062006C00650056006900650077004D006F00640065003A00300000000100000008003800000034002C0030002C003200380034002C0030002C0031003800330030002C0031002C0031003500300030002C0035002C003900390030000000200000005400610062006C00650056006900650077004D006F00640065003A00310000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00320000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00330000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00340000000100000008003E00000034002C0030002C003200380034002C0030002C0031003800330030002C00310032002C0032003100370035002C00310031002C00310033003300350000009E0000009E00000000000000000000000000000000000000D00200000600280000004100630074006900760065005400610062006C00650056006900650077004D006F006400650000000100000008000400000030000000200000005400610062006C00650056006900650077004D006F00640065003A00300000000100000008003800000034002C0030002C003200380034002C0030002C0033003500320035002C0031002C0031003500300030002C0035002C003900390030000000200000005400610062006C00650056006900650077004D006F00640065003A00310000000100000008002000000032002C0030002C003200380034002C0030002C00310030003800370035000000200000005400610062006C00650056006900650077004D006F00640065003A00320000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00330000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00340000000100000008003E00000034002C0030002C003200380034002C0030002C0031003800330030002C00310032002C0032003100370035002C00310031002C0031003300330035000000A2000000A200000000000000000000000000000000000000D00200000600280000004100630074006900760065005400610062006C00650056006900650077004D006F006400650000000100000008000400000030000000200000005400610062006C00650056006900650077004D006F00640065003A00300000000100000008003800000034002C0030002C003200380034002C0030002C0031003800330030002C0031002C0031003500300030002C0035002C003900390030000000200000005400610062006C00650056006900650077004D006F00640065003A00310000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00320000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00330000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00340000000100000008003E00000034002C0030002C003200380034002C0030002C0031003800330030002C00310032002C0032003100370035002C00310031002C0031003300330035000000A3000000A300000000000000580000000103737401000000640062006F00000046004B005F005400610062006C00650049006E0066006F005F00450078007400650072006E0061006C004400610074006100620061007300650053006500720076006500720000000000000000000000C40200000000A4000000A4000000A30000000800000001F2711548F271150000000000000000AD070000000000A6000000A600000000000000580000000103737401000000640062006F00000046004B005F0043006100740061006C006F006700750065005F00450078007400650072006E0061006C004400610074006100620061007300650053006500720076006500720000000000000000000000C40200000000A7000000A7000000A60000000800000001F2711508F271150000000000000000AD070000000000A8000000A8000000000000005A00000001075F5E01000000640062006F00000046004B005F0043006100740061006C006F006700750065005F00450078007400650072006E0061006C0044006100740061006200610073006500530065007200760065007200310000000000000000000000C40200000000A9000000A9000000A80000000800000001F17115C8F171150000000000000000AD070000000000AA000000AA00000000000000560000000103737401000000640062006F00000046004B005F0041004E004F005400610062006C0065005F00450078007400650072006E0061006C004400610074006100620061007300650053006500720076006500720000000000000000000000C40200000000AB000000AB000000AA0000000800000001F1711588F171150000000000000000AD070000000000AC000000AC00000000000000000000000000000000000000D00200000600280000004100630074006900760065005400610062006C00650056006900650077004D006F006400650000000100000008000400000030000000200000005400610062006C00650056006900650077004D006F00640065003A00300000000100000008003800000034002C0030002C003200380034002C0030002C0033003400360035002C0031002C0031003500300030002C0035002C003900390030000000200000005400610062006C00650056006900650077004D006F00640065003A00310000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00320000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00330000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00340000000100000008003E00000034002C0030002C003200380034002C0030002C0031003800330030002C00310032002C0032003100370035002C00310031002C0031003300330035000000AF000000AF000000000000006A000000011A5F5E01000000640062006F00000046004B005F0041006700670072006500670061007400650043006F006E00660069006700750072006100740069006F006E005F00410067006700720065006700610074006500440069006D0065006E00730069006F006E0000000000000000000000C40200000000B0000000B0000000AF0000000800000001F1711548F171150000000000000000AD070000000000BD000000BD00000000000000000000000000000000000000D00200000600280000004100630074006900760065005400610062006C00650056006900650077004D006F006400650000000100000008000400000031000000200000005400610062006C00650056006900650077004D006F00640065003A00300000000100000008003800000034002C0030002C003200380034002C0030002C0031003800330030002C0031002C0031003500300030002C0035002C003900390030000000200000005400610062006C00650056006900650077004D006F00640065003A00310000000100000008001E00000032002C0030002C003200380034002C0030002C0033003400350030000000200000005400610062006C00650056006900650077004D006F00640065003A00320000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00330000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00340000000100000008003E00000034002C0030002C003200380034002C0030002C0031003800330030002C00310032002C0032003100370035002C00310031002C0031003300330035000000BE000000BE000000000000004400000001038A7601000000640062006F00000046004B005F0043006100740061006C006F006700750065005F004C006F00610064004D00650074006100640061007400610000000000000000000000C40200000000BF000000BF000000BE0000000800000001F1711508F171150000000000000000AD070000000000C0000000C000000000000000520000000103737401000000640062006F00000046004B005F004C006F006100640050006500720069006F0064006900630061006C006C0079005F004C006F00610064004D00650074006100640061007400610000000000000000000000C40200000000C1000000C1000000C00000000800000001F07115C8F071150000000000000000AD070000000000C2000000C2000000000000004800000001038A7601000000640062006F00000046004B005F00500072006F0063006500730073005400610073006B005F004C006F00610064004D00650074006100640061007400610000000000000000000000C40200000000C3000000C3000000C20000000800000001F0711588F071150000000000000000AD070000000000C4000000C4000000000000004A00000001FF5F5E01000000640062006F00000046004B005F004C006F00610064005300630068006500640075006C0065005F004C006F00610064004D00650074006100640061007400610000000000000000000000C40200000000C5000000C5000000C40000000800000001F0711548F071150000000000000000AD070000000000C6000000C600000000000000000000000000000000000000D00200000600280000004100630074006900760065005400610062006C00650056006900650077004D006F006400650000000100000008000400000030000000200000005400610062006C00650056006900650077004D006F00640065003A00300000000100000008003800000034002C0030002C003200380034002C0030002C0031003800330030002C0031002C0031003500300030002C0035002C003900390030000000200000005400610062006C00650056006900650077004D006F00640065003A00310000000100000008001E00000032002C0030002C003200380034002C0030002C0032003700390030000000200000005400610062006C00650056006900650077004D006F00640065003A00320000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00330000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00340000000100000008003E00000034002C0030002C003200380034002C0030002C0031003800330030002C00310032002C0032003100370035002C00310031002C0031003300330035000000C7000000C7000000000000006600000001006F0001000000640062006F00000046004B005F0044004C004500570069006E0064006F0077007300530065007200760069006300650045007800630065007000740069006F006E005F004C006F00610064004D00650074006100640061007400610000000000000000000000C40200000000C8000000C8000000C70000000800000001F0711508F071150000000000000000AD070000000000C9000000C900000000000000540000000103737401000000640062006F00000046004B005F004C006F006100640050006500720069006F0064006900630061006C006C0079005F004C006F00610064004D006500740061006400610074006100310000000000000000000000C40200000000CA000000CA000000C90000000800000001EF7115C8EF71150000000000000000AD070000000000CB000000CB00000000000000000000000000000000000000D00200000600280000004100630074006900760065005400610062006C00650056006900650077004D006F006400650000000100000008000400000030000000200000005400610062006C00650056006900650077004D006F00640065003A00300000000100000008003800000034002C0030002C003200380034002C0030002C0032003200390035002C0031002C0031003500300030002C0035002C003900390030000000200000005400610062006C00650056006900650077004D006F00640065003A00310000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00320000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00330000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00340000000100000008003E00000034002C0030002C003200380034002C0030002C0031003800330030002C00310032002C0032003100370035002C00310031002C0031003300330035000000CC000000CC00000000000000740000000102000001000000640062006F00000046004B005F0041006700670072006500670061007400650043006F006E00740069006E0075006F0075007300440061007400650041007800690073005F00410067006700720065006700610074006500440069006D0065006E00730069006F006E0000000000000000000000C40200000000CD000000CD000000CC0000000800000001EF711588EF71150000000000000000AD070000000000D0000000D000000000000000000000000000000000000000D00200000600280000004100630074006900760065005400610062006C00650056006900650077004D006F006400650000000100000008000400000030000000200000005400610062006C00650056006900650077004D006F00640065003A00300000000100000008003800000034002C0030002C003200380034002C0030002C0032003300320035002C0031002C0031003500300030002C0035002C003900390030000000200000005400610062006C00650056006900650077004D006F00640065003A00310000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00320000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00330000000100000008001E00000032002C0030002C003200380034002C0030002C0031003800330030000000200000005400610062006C00650056006900650077004D006F00640065003A00340000000100000008003E00000034002C0030002C003200380034002C0030002C0031003800330030002C00310032002C0032003100370035002C00310031002C0031003300330035000000D1000000D1000000000000006C000000011A5F5E01000000640062006F00000046004B005F0041006700670072006500670061007400650046006F0072006300650064004A006F0069006E005F0041006700670072006500670061007400650043006F006E00660069006700750072006100740069006F006E0000000000000000000000C40200000000D2000000D2000000D10000000800000001EF711548EF71150000000000000000AD070000000000D3000000D300000000000000520000000100737401000000640062006F00000046004B005F0041006700670072006500670061007400650046006F0072006300650064004A006F0069006E005F005400610062006C00650049006E0066006F0000000000000000000000C40200000000D4000000D4000000D30000000800000001B5511558B551150000000000000000AD070000000000D5000000D500000000000000580000000103737401000000640062006F00000046004B005F0041006700670072006500670061007400650043006F006E00660069006700750072006100740069006F006E005F0043006100740061006C006F0067007500650000000000000000000000C40200000000D6000000D6000000D50000000800000001337715E03377150000000000000000AD0F0000010000EB00000081000000010000007C00000056000000D200000016000000010000000D0000008C00000085000000D5000000020000005F0000008C000000710000005D000000020000005C00000097000000AA0000001900000002000000180000009F0000008000000003000000020000000100000000000000010000009A0000000200000090000000E30100002100000007000000050000000600000038000000210000008E000000050000008D0000008C00000093000000D300000005000000D00000008D000000A600000012000000060000000D0000008A000000C80000002700000006000000240000002A000000390000002900000006000000240000003C0000004B0000002B0000000600000024000000200000002F00000033000000060000002F000000A20000008D00000035000000060000002F000000B20000009D00000056000000060000004C000000140000009C00000059000000060000004C0000000600000090000000960000001B00000071000000930000003D000000220000001B0000000D0000004000000033000000390000001B00000038000000170000004E000000400000001B000000020000007F000000C001000054000000240000004C0000001C0000001B0000003C000000380000003B000000350000002C000000720000005F00000071000000A500000090000000D10000005F000000D00000000F0000007200000074000000600000005F00000080000000730000006F000000600000006E0000009F000000720000006600000060000000630000007300000088000000640000006000000063000000870000009C000000770000006E000000760000007900000072000000AF000000710000005F000000B6000000CB000000CC00000071000000CB0000004B0000000000000084000000830000007C00000009000000B700000086000000830000007C000000A60000007C0000008B000000880000000600000073000000E2000000940000009000000091000000830000008C000000AA000000A2000000880000009C00000037000000A8000000A2000000020000006D0000001A020000A6000000A2000000020000008100000008020000A3000000A200000005000000940000003F000000C9000000BD000000AC0000004D0000005C000000BE000000BD000000020000006000000013010000C0000000BD000000AC0000001101000002000000C2000000BD00000090000000AD000000AC000000C4000000BD0000009E000000F7000000C2000000C7000000BD000000C6000000010000002E0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000700070006C00690063006100740069006F006E0020004E0061006D0065003D0022004D006900630072006F0073006F00660074002000530051004C00200053006500720076006500720020004D0061006E006100670065006D0065006E0074002000530074007500640069006F0022000000008005002E00000043006100740061006C006F006700750065005F0044006100740061005F004400690061006700720061006D000000000226002800000041006700670072006500670061007400650046006F0072006300650064004A006F0069006E00000008000000640062006F000000000226003800000041006700670072006500670061007400650043006F006E00740069006E0075006F007500730044006100740065004100780069007300000008000000640062006F000000000226003600000044004C004500570069006E0064006F0077007300530065007200760069006300650045007800630065007000740069006F006E00000008000000640062006F000000000226001A0000004C006F00610064004D006500740061006400610074006100000008000000640062006F00000000022600220000004C006F006100640050006500720069006F0064006900630061006C006C007900000008000000640062006F000000000226002E000000450078007400650072006E0061006C0044006100740061006200610073006500530065007200760065007200000008000000640062006F000000000226001A0000004C006F00610064005300630068006500640075006C006500000008000000640062006F00000000022600260000004C006F00610064004D006F00640075006C00650041007300730065006D0062006C007900000008000000640062006F0000000002260028000000500072006F0063006500730073005400610073006B0041007200670075006D0065006E007400000008000000640062006F0000000002260018000000500072006F0063006500730073005400610073006B00000008000000640062006F000000000226002E0000005000720065004C006F006100640044006900730063006100720064006500640043006F006C0075006D006E00000008000000640062006F000000000226001200000041004E004F005400610062006C006500000008000000640062006F000000000226002000000049007300730075006500530079007300740065006D005500730065007200000008000000640062006F000000000226002600000043006100740061006C006F006700750065004900740065006D0049007300730075006500000008000000640062006F0000000002260032000000410067006700720065006700610074006500460069006C0074006500720050006100720061006D006500740065007200000008000000640062006F0000000002260026000000410067006700720065006700610074006500440069006D0065006E00730069006F006E00000008000000640062006F0000000002260020000000410067006700720065006700610074006500460069006C00740065007200000008000000640062006F0000000002260038000000410067006700720065006700610074006500460069006C0074006500720053007500620043006F006E007400610069006E0065007200000008000000640062006F0000000002260032000000410067006700720065006700610074006500460069006C0074006500720043006F006E007400610069006E0065007200000008000000640062006F000000000226002E00000041006700670072006500670061007400650043006F006E00660069006700750072006100740069006F006E00000008000000640062006F000000000226002600000053007500700070006F007200740069006E006700530051004C005400610062006C006500000008000000640062006F00000000022600300000004C006F006F006B007500700043006F006D0070006F0073006900740065004A006F0069006E0049006E0066006F00000008000000640062006F0000000002260034000000450078007400720061006300740069006F006E00460069006C0074006500720050006100720061006D006500740065007200000008000000640062006F0000000002260022000000450078007400720061006300740069006F006E00460069006C00740065007200000008000000640062006F00000000022600120000004A006F0069006E0049006E0066006F00000008000000640062006F000000000226000E0000004C006F006F006B0075007000000008000000640062006F000000000226002C000000450078007400720061006300740069006F006E0049006E0066006F0072006D006100740069006F006E00000008000000640062006F000000000226002600000053007500700070006F007200740069006E00670044006F00630075006D0065006E007400000008000000640062006F000000000226003200000043006F006C0075006D006E0049006E0066006F005F0043006100740061006C006F006700750065004900740065006D00000008000000640062006F000000000226001600000043006F006C0075006D006E0049006E0066006F00000008000000640062006F00000000022600140000005400610062006C00650049006E0066006F00000008000000640062006F000000000226001400000043006100740061006C006F00670075006500000008000000640062006F000000000224001C00000043006100740061006C006F006700750065004900740065006D00000008000000640062006F00000001000000D68509B3BB6BF2459AB8371664F0327008004E0000007B00310036003300340043004400440037002D0030003800380038002D0034003200450033002D0039004600410032002D004200360044003300320035003600330042003900310044007D00000000000000000000000000000000000000000000000000000000000000010003000000000000000C0000000B00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000062885214)

GO