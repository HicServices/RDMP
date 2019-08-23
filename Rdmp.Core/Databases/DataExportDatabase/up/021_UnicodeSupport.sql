--Version:3.2.0
--Description: Changes all varchar(x) columns that involve user provided values into nvarchar(x)
ALTER TABLE [dbo].[ExtractionConfiguration] DROP CONSTRAINT [DF_ExtractionConfiguration_Separator]
GO


alter table [SupplementalExtractionResults] alter column [DestinationDescription] nvarchar(max)NULL
alter table [SupplementalExtractionResults] alter column [Exception] nvarchar(max)NULL
alter table [SupplementalExtractionResults] alter column [SQLExecuted] nvarchar(max)NULL
alter table [SupplementalExtractionResults] alter column [ExtractedName] nvarchar(max)NULL
alter table [SupplementalExtractionResults] alter column [ReferencedObjectType] nvarchar(max)NULL
alter table [SupplementalExtractionResults] alter column [ReferencedObjectRepositoryType] nvarchar(max)NULL
alter table [SupplementalExtractionResults] alter column [DestinationType] nvarchar(500)NULL
alter table [ConfigurationProperties] alter column [Value] nvarchar(max)NULL
alter table [ConfigurationProperties] alter column [Description] nvarchar(max)NULL
alter table [CumulativeExtractionResults] alter column [DestinationDescription] nvarchar(max)NULL
alter table [CumulativeExtractionResults] alter column [FiltersUsed] nvarchar(max)NULL
alter table [CumulativeExtractionResults] alter column [Exception] nvarchar(max)NULL
alter table [CumulativeExtractionResults] alter column [SQLExecuted] nvarchar(max)NULL
alter table [CumulativeExtractionResults] alter column [DestinationType] nvarchar(500)NULL
alter table [DataUser] alter column [Forename] nvarchar(50)NOT NULL
alter table [DataUser] alter column [Surname] nvarchar(50)NOT NULL
alter table [DataUser] alter column [Email] nvarchar(100)NULL
alter table [DeployedExtractionFilter] alter column [WhereSQL] nvarchar(max)NULL
alter table [DeployedExtractionFilter] alter column [Description] nvarchar(max)NULL
alter table [DeployedExtractionFilter] alter column [Name] nvarchar(100)NOT NULL
alter table [DeployedExtractionFilterParameter] alter column [ParameterSQL] nvarchar(500)NULL
alter table [DeployedExtractionFilterParameter] alter column [Value] nvarchar(max)NULL
alter table [DeployedExtractionFilterParameter] alter column [Comment] nvarchar(500)NULL
alter table [ExternalCohortTable] alter column [Name] nvarchar(1000)NOT NULL
alter table [ExternalCohortTable] alter column [Server] nvarchar(50)NULL
alter table [ExternalCohortTable] alter column [Database] nvarchar(250)NULL
alter table [ExternalCohortTable] alter column [TableName] nvarchar(500)NULL
alter table [ExternalCohortTable] alter column [DefinitionTableName] nvarchar(500)NULL
alter table [ExternalCohortTable] alter column [PrivateIdentifierField] nvarchar(1000)NULL
alter table [ExternalCohortTable] alter column [ReleaseIdentifierField] nvarchar(1000)NULL
alter table [ExternalCohortTable] alter column [DefinitionTableForeignKeyField] nvarchar(1000)NULL
alter table [ExternalCohortTable] alter column [Username] nvarchar(500)NULL
alter table [ExternalCohortTable] alter column [Password] nvarchar(max)NULL
alter table [ExternalCohortTable] alter column [DatabaseType] nvarchar(100)NOT NULL
alter table [ExtractableCohort] alter column [OverrideReleaseIdentifierSQL] nvarchar(500)NULL
alter table [ExtractableCohort] alter column [AuditLog] nvarchar(max)NULL
alter table [ExtractableColumn] alter column [SelectSQL] nvarchar(500)NULL
alter table [ExtractableColumn] alter column [Alias] nvarchar(100)NULL
alter table [ExtractionConfiguration] alter column [Username] nvarchar(50)NULL
alter table [ExtractionConfiguration] alter column [RequestTicket] nvarchar(10)NULL
alter table [ExtractionConfiguration] alter column [ReleaseTicket] nvarchar(10)NULL
alter table [ExtractionConfiguration] alter column [Separator] nvarchar(3)NOT NULL
alter table [ExtractionConfiguration] alter column [Description] nvarchar(max)NULL
alter table [ExtractionConfiguration] alter column [Name] nvarchar(1000)NOT NULL
alter table [FilterContainer] alter column [Operation] nvarchar(10)NULL
alter table [GlobalExtractionFilterParameter] alter column [Value] nvarchar(max)NULL
alter table [GlobalExtractionFilterParameter] alter column [ParameterSQL] nvarchar(500)NULL
alter table [GlobalExtractionFilterParameter] alter column [Comment] nvarchar(500)NULL
alter table [Project] alter column [Name] nvarchar(1000)NOT NULL
alter table [Project] alter column [MasterTicket] nvarchar(10)NULL
alter table [Project] alter column [ExtractionDirectory] nvarchar(300)NULL
alter table [ReleaseLog] alter column [Username] nvarchar(50)NOT NULL
alter table [ReleaseLog] alter column [MD5OfDatasetFile] nvarchar(256)NOT NULL
alter table [ReleaseLog] alter column [DatasetState] nvarchar(100)NOT NULL
alter table [ReleaseLog] alter column [EnvironmentState] nvarchar(500)NOT NULL
alter table [ReleaseLog] alter column [ReleaseFolder] nvarchar(max)NOT NULL
alter table [ExtractableDataSetPackage] alter column [Name] nvarchar(1000)NOT NULL
alter table [ExtractableDataSetPackage] alter column [Creator] nvarchar(500)NOT NULL

GO

ALTER TABLE [dbo].[ExtractionConfiguration] ADD  CONSTRAINT [DF_ExtractionConfiguration_Separator]  DEFAULT (',') FOR [Separator]