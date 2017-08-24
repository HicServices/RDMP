--Version: 1.2.0.0
--Description: Allows the user to specify explicit usernames and password that override integrated security when accessing data through a TableInfo under a specific context (e.g. DataLoading, QualityReportGeneration, DataExtraction etc).  IMPORTANT: this table contains sensitive passwords
--Create the table credentials
if not exists (select 1 from sys.tables where name = 'DataAccessCredentials')
begin
CREATE TABLE [dbo].[DataAccessCredentials](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Username] [varchar](500) NULL,
	[Password] [varchar](500) NULL,
	[Name] [varchar](100) NOT NULL,
 CONSTRAINT [PK_DataAccessCredentials] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
end


--create the link table
if not exists (select 1 from sys.tables where name = 'DataAccessCredentials_TableInfo')
begin
CREATE TABLE [dbo].[DataAccessCredentials_TableInfo](
	[TableInfo_ID] [int] NOT NULL,
	[DataAccessCredentials_ID] [int] NOT NULL,
	[Context] [varchar](30) NOT NULL,
 CONSTRAINT [PK_DataAccessCredentials_TableInfo] PRIMARY KEY CLUSTERED 
(
	[TableInfo_ID] ASC, --There can only be 1 credentials to use to access a given table under a given context
	[Context] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
end


if not exists( select 1 from sys.indexes where name = 'ix_NamesMustBeUnique' and object_id = object_id('DataAccessCredentials'))
begin
CREATE UNIQUE NONCLUSTERED INDEX [ix_NamesMustBeUnique] ON [dbo].DataAccessCredentials
(
	[Name] ASC
)
end 

if not exists( select 1 from sys.all_objects where name = 'FK_DataAccessCredentials_TableInfo_DataAccessCredentials' and type_desc = 'FOREIGN_KEY_CONSTRAINT')
begin
ALTER TABLE [dbo].[DataAccessCredentials_TableInfo]  WITH CHECK ADD  CONSTRAINT [FK_DataAccessCredentials_TableInfo_DataAccessCredentials] FOREIGN KEY([DataAccessCredentials_ID])
REFERENCES [dbo].[DataAccessCredentials] ([ID])
end
ALTER TABLE [dbo].[DataAccessCredentials_TableInfo] CHECK CONSTRAINT [FK_DataAccessCredentials_TableInfo_DataAccessCredentials]

if not exists( select 1 from sys.all_objects where name = 'FK_DataAccessCredentials_TableInfo_TableInfo' and type_desc = 'FOREIGN_KEY_CONSTRAINT')
begin
ALTER TABLE [dbo].[DataAccessCredentials_TableInfo]  WITH CHECK ADD  CONSTRAINT [FK_DataAccessCredentials_TableInfo_TableInfo] FOREIGN KEY([TableInfo_ID])
REFERENCES [dbo].[TableInfo] ([ID])
ON DELETE CASCADE --Allow the deletion of TableInfos to delete the RELATIONSHIP to Credentials - do not allow the other way around
end

ALTER TABLE [dbo].[DataAccessCredentials_TableInfo] CHECK CONSTRAINT [FK_DataAccessCredentials_TableInfo_TableInfo]