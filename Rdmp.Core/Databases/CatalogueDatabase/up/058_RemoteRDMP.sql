--Version:2.5.0.1
--Description: Creates the RemoteRDMP Table and Adds a Name to the AutomationServiceSlot

/****** Object:  Table [dbo].[RemoteRDMP]    Script Date: 09/11/2017 10:27:33 ******/
if not exists(select 1 from sys.tables where name ='RemoteRDMP')
BEGIN
    CREATE TABLE [dbo].[RemoteRDMP](
	    [ID] [int] IDENTITY(1,1) NOT NULL,
	    [URL] [varchar](1024) NOT NULL,
	    [Name] [varchar](100) NOT NULL,
	    [Username] [varchar](500) NULL,
	    [Password] [varchar](max) NULL,
     CONSTRAINT [PK_RemoteRDMP] PRIMARY KEY CLUSTERED 
    (
	    [ID] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END

/****** Object:  Index [IX_RemoteRDMP_NameMustBeUnique]    Script Date: 09/11/2017 10:31:36 ******/
if not exists (select 1 from sys.indexes where name ='IX_RemoteRDMP_NameMustBeUnique')
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_RemoteRDMP_NameMustBeUnique] ON [dbo].[RemoteRDMP]
    (
	    [Name] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END

if not exists (select 1 from sys.columns where name = 'Name' and OBJECT_NAME(object_id) = 'AutomationServiceSlot')
  begin
	alter table AutomationServiceSlot add Name varchar(1000) null
  end
  GO

update  AutomationServiceSlot set Name = 'Unnamed Slot' where Name is null;

--now make it not null

if exists (select 1 from sys.columns where name = 'Name' and OBJECT_NAME(object_id) = 'AutomationServiceSlot' and is_nullable = 1)
  begin
    alter table AutomationServiceSlot alter column Name varchar(1000) not null
  end

