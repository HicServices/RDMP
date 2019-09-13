--Version:3.2.0
--Description: Changes all varchar(x) columns that involve user provided values into nvarchar(x)

if exists (select 1 from sys.foreign_keys where name = 'FK_DataLoadTask_DataSet')
	ALTER TABLE [dbo].[DataLoadTask] DROP CONSTRAINT [FK_DataLoadTask_DataSet]

if exists (select 1 from sys.foreign_keys where name = 'FK_DataLoadRun_DataLoadTask')
	ALTER TABLE [dbo].[DataLoadRun] DROP CONSTRAINT [FK_DataLoadRun_DataLoadTask]

if exists (select 1 from sys.key_constraints where name = 'PK_DataTask')
	ALTER TABLE [dbo].[DataLoadTask] DROP CONSTRAINT [PK_DataTask]

if exists (select 1 from sys.key_constraints where name = 'PK_DataSet')
	ALTER TABLE DataSet DROP CONSTRAINT PK_DataSet;

GO

alter table [DataLoadRun] alter column [description] nvarchar(max)NOT NULL
alter table [DataLoadRun] alter column [packageName] nvarchar(750)NOT NULL
alter table [DataLoadRun] alter column [userAccount] nvarchar(500)NOT NULL
alter table [DataLoadRun] alter column [suggestedRollbackCommand] nvarchar(max)NULL
alter table [DataLoadTask] alter column [description] nvarchar(max)NOT NULL
alter table [DataLoadTask] alter column [name] nvarchar(1000)NOT NULL
alter table [DataLoadTask] alter column [userAccount] nvarchar(500)NOT NULL
alter table [DataLoadTask] alter column [dataSetID] nvarchar(450)NOT NULL
alter table [DataSet] alter column [dataSetID] nvarchar(450)NOT NULL
alter table [DataSet] alter column [name] nvarchar(2000)NULL
alter table [DataSet] alter column [description] nvarchar(max)NULL
alter table [DataSet] alter column [time_period] nvarchar(64)NULL
alter table [DataSet] alter column [SLA_required] nvarchar(3)NULL
alter table [DataSet] alter column [supplier_name] nvarchar(32)NULL
alter table [DataSet] alter column [supplier_tel_no] nvarchar(32)NULL
alter table [DataSet] alter column [supplier_email] nvarchar(64)NULL
alter table [DataSet] alter column [contact_name] nvarchar(64)NULL
alter table [DataSet] alter column [contact_position] nvarchar(64)NULL
alter table [DataSet] alter column [currentContactInstitutions] nvarchar(64)NULL
alter table [DataSet] alter column [contact_tel_no] nvarchar(32)NULL
alter table [DataSet] alter column [contact_email] nvarchar(64)NULL
alter table [DataSet] alter column [frequency] nvarchar(32)NULL
alter table [DataSet] alter column [method] nvarchar(16)NULL
alter table [DataSource] alter column [source] nvarchar(max)NOT NULL
alter table [DataSource] alter column [archive] nvarchar(max)NULL
alter table [FatalError] alter column [source] nvarchar(max)NULL
alter table [FatalError] alter column [description] nvarchar(max)NOT NULL
alter table [FatalError] alter column [explanation] nvarchar(max)NULL
alter table [ProgressLog] alter column [eventType] nvarchar(50)NULL
alter table [ProgressLog] alter column [description] nvarchar(max)NULL
alter table [ProgressLog] alter column [source] nvarchar(max)NULL
alter table [RowError] alter column [description] nvarchar(max)NOT NULL
alter table [RowError] alter column [locationOfRow] nvarchar(max)NOT NULL
alter table [RowError] alter column [columnName] nvarchar(max)NULL

GO

if not exists (select 1 from sys.key_constraints where name = 'PK_DataTask')
	ALTER TABLE DataSet ADD CONSTRAINT PK_DataSet PRIMARY KEY (dataSetID); 

if not exists (select 1 from sys.foreign_keys where name = 'FK_DataLoadTask_DataSet')
	ALTER TABLE [dbo].[DataLoadTask]  WITH CHECK ADD  CONSTRAINT [FK_DataLoadTask_DataSet] FOREIGN KEY([dataSetID])
	REFERENCES [dbo].[DataSet] ([dataSetID])

if not exists (select 1 from sys.key_constraints where name = 'PK_DataTask')
	ALTER TABLE [dbo].[DataLoadTask] ADD  CONSTRAINT [PK_DataTask] PRIMARY KEY CLUSTERED 
	(
		[ID] ASC
	)

if not exists (select 1 from sys.foreign_keys where name = 'FK_DataLoadRun_DataLoadTask')
	ALTER TABLE [dbo].[DataLoadRun]  WITH CHECK ADD  CONSTRAINT [FK_DataLoadRun_DataLoadTask] FOREIGN KEY([dataLoadTaskID])
	REFERENCES [dbo].[DataLoadTask] ([ID])
	ON UPDATE CASCADE

