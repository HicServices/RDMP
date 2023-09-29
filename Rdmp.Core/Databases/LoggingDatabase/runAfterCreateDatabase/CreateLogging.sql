--Version:1.0.0.0
--Description:Initial Creation Script
/****** Object:  Table [dbo].[DataLoadRun]    Script Date: 06/02/2015 18:39:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[DataLoadRun](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[description] [varchar](max) NOT NULL,
	[startTime] [datetime] NOT NULL,
	[endTime] [datetime] NULL,
	[dataLoadTaskID] [int] NOT NULL,
	[isTest] [bit] NOT NULL,
	[packageName] [varchar](100) NOT NULL,
	[userAccount] [varchar](50) NOT NULL,
	[suggestedRollbackCommand] [varchar](max) NULL,
 CONSTRAINT [PK_DataLoad] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[DataLoadTask]    Script Date: 06/02/2015 18:39:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[DataLoadTask](
	[ID] [int] NOT NULL,
	[description] [varchar](max) NOT NULL,
	[name] [varchar](255) NOT NULL,
	[createTime] [datetime] NOT NULL,
	[userAccount] [varchar](50) NOT NULL,
	[statusID] [int] NOT NULL,
	[isTest] [bit] NOT NULL,
	[dataSetID] [varchar](16) NOT NULL,
 CONSTRAINT [PK_DataTask] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[DataSet]    Script Date: 06/02/2015 18:39:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[DataSet](
	[dataSetID] [varchar](16) NOT NULL,
	[name] [varchar](64) NULL,
	[description] [varchar](128) NULL,
	[time_period] [varchar](64) NULL,
	[SLA_required] [varchar](3) NULL,
	[supplier_name] [varchar](32) NULL,
	[supplier_tel_no] [varchar](32) NULL,
	[supplier_email] [varchar](64) NULL,
	[contact_name] [varchar](64) NULL,
	[contact_position] [varchar](64) NULL,
	[currentContactInstitutions] [varchar](64) NULL,
	[contact_tel_no] [varchar](32) NULL,
	[contact_email] [varchar](64) NULL,
	[frequency] [varchar](32) NULL,
	[method] [varchar](16) NULL,
 CONSTRAINT [PK_DataSet] PRIMARY KEY CLUSTERED 
(
	[dataSetID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[DataSource]    Script Date: 06/02/2015 18:39:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[DataSource](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[source] [varchar](max) NOT NULL,
	[tableLoadRunID] [int] NULL,
	[archive] [varchar](max) NULL,
	[originDate] [date] NULL,
	[MD5] [binary](128) NULL,
 CONSTRAINT [PK_DataSource] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[FatalError]    Script Date: 06/02/2015 18:39:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[FatalError](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[time] [datetime] NOT NULL,
	[source] [varchar](50) NULL,
	[description] [varchar](max) NOT NULL,
	[explanation] [varchar](max) NULL,
	[dataLoadRunID] [int] NULL,
	[statusID] [int] NULL,
	[interestingToOthers] [bit] NULL,
 CONSTRAINT [PK_FatalError] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ProgressLog]    Script Date: 06/02/2015 18:39:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ProgressLog](
	[dataLoadRunID] [int] NOT NULL,
	[eventType] [varchar](50) NULL,
	[description] [varchar](8000) NULL,
	[source] [varchar](100) NULL,
	[time] [datetime] NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL,
 CONSTRAINT [PK_ProgressLog] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[RowError]    Script Date: 06/02/2015 18:39:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[RowError](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[tableLoadRunID] [int] NOT NULL,
	[rowErrorTypeID] [int] NULL,
	[description] [varchar](max) NOT NULL,
	[locationOfRow] [varchar](max) NOT NULL,
	[requiresReloading] [bit] NOT NULL,
	[columnName] [varchar](50) NULL,
 CONSTRAINT [PK_RowErrors] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[TableLoadRun]    Script Date: 06/02/2015 18:39:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[TableLoadRun](
	[startTime] [datetime] NOT NULL,
	[endTime] [datetime] NULL,
	[dataLoadRunID] [int] NOT NULL,
	[targetTable] [varchar](200) NOT NULL,
	[expectedInserts] [bigint] NULL,
	[inserts] [bigint] NULL,
	[updates] [bigint] NULL,
	[deletes] [bigint] NULL,
	[errorRows] [bigint] NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[duplicates] [bigint] NULL,
	[notes] [varchar](8000) NULL,
	[suggestedRollbackCommand] [varchar](max) NULL,
 CONSTRAINT [PK_TableLoadRun] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[z_DataLoadTaskStatus]    Script Date: 06/02/2015 18:39:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[z_DataLoadTaskStatus](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[status] [varchar](50) NULL,
	[description] [varchar](max) NULL,
 CONSTRAINT [PK_z_DataLoadTaskStatus] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[z_FatalErrorStatus]    Script Date: 06/02/2015 18:39:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[z_FatalErrorStatus](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[status] [varchar](20) NOT NULL,
 CONSTRAINT [PK_z_FatalErrorsStatus] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[z_RowErrorType]    Script Date: 06/02/2015 18:39:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[z_RowErrorType](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[type] [varchar](20) NOT NULL,
 CONSTRAINT [PK_z_RowErrorsType] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[DataLoadRun]  WITH CHECK ADD  CONSTRAINT [FK_DataLoadRun_DataLoadTask] FOREIGN KEY([dataLoadTaskID])
REFERENCES [dbo].[DataLoadTask] ([ID])
ON UPDATE CASCADE
GO
ALTER TABLE [dbo].[DataLoadRun] CHECK CONSTRAINT [FK_DataLoadRun_DataLoadTask]
GO
ALTER TABLE [dbo].[DataLoadTask]  WITH CHECK ADD  CONSTRAINT [FK_DataLoadTask_DataLoadTask] FOREIGN KEY([dataSetID])
REFERENCES [dbo].[DataSet] ([dataSetID])
GO
ALTER TABLE [dbo].[DataLoadTask] CHECK CONSTRAINT [FK_DataLoadTask_DataLoadTask]
GO
ALTER TABLE [dbo].[DataLoadTask]  WITH CHECK ADD  CONSTRAINT [FK_DataLoadTask_z_DataLoadTaskStatus] FOREIGN KEY([statusID])
REFERENCES [dbo].[z_DataLoadTaskStatus] ([ID])
GO
ALTER TABLE [dbo].[DataLoadTask] CHECK CONSTRAINT [FK_DataLoadTask_z_DataLoadTaskStatus]
GO
ALTER TABLE [dbo].[DataSource]  WITH CHECK ADD  CONSTRAINT [FK_DataSource_TableLoadRun] FOREIGN KEY([tableLoadRunID])
REFERENCES [dbo].[TableLoadRun] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DataSource] CHECK CONSTRAINT [FK_DataSource_TableLoadRun]
GO
ALTER TABLE [dbo].[FatalError]  WITH CHECK ADD  CONSTRAINT [FK_FatalError_DataLoadRun] FOREIGN KEY([dataLoadRunID])
REFERENCES [dbo].[DataLoadRun] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[FatalError] CHECK CONSTRAINT [FK_FatalError_DataLoadRun]
GO
ALTER TABLE [dbo].[FatalError]  WITH CHECK ADD  CONSTRAINT [FK_FatalErrors_z_FatalErrorsStatus] FOREIGN KEY([statusID])
REFERENCES [dbo].[z_FatalErrorStatus] ([ID])
GO
ALTER TABLE [dbo].[FatalError] CHECK CONSTRAINT [FK_FatalErrors_z_FatalErrorsStatus]
GO
ALTER TABLE [dbo].[ProgressLog]  WITH CHECK ADD  CONSTRAINT [FK_ProgressLog_DataLoadRun] FOREIGN KEY([dataLoadRunID])
REFERENCES [dbo].[DataLoadRun] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ProgressLog] CHECK CONSTRAINT [FK_ProgressLog_DataLoadRun]
GO
ALTER TABLE [dbo].[RowError]  WITH CHECK ADD  CONSTRAINT [FK_RowErrors_TableLoadRun] FOREIGN KEY([tableLoadRunID])
REFERENCES [dbo].[TableLoadRun] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[RowError] CHECK CONSTRAINT [FK_RowErrors_TableLoadRun]
GO
ALTER TABLE [dbo].[RowError]  WITH CHECK ADD  CONSTRAINT [FK_RowErrors_z_RowErrorsType] FOREIGN KEY([rowErrorTypeID])
REFERENCES [dbo].[z_RowErrorType] ([ID])
GO
ALTER TABLE [dbo].[RowError] CHECK CONSTRAINT [FK_RowErrors_z_RowErrorsType]
GO
ALTER TABLE [dbo].[TableLoadRun]  WITH CHECK ADD  CONSTRAINT [FK_TableLoadRun_DataLoadRun] FOREIGN KEY([dataLoadRunID])
REFERENCES [dbo].[DataLoadRun] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TableLoadRun] CHECK CONSTRAINT [FK_TableLoadRun_DataLoadRun]
GO


--create statuses
SET IDENTITY_INSERT [dbo].[z_DataLoadTaskStatus] ON 

GO
INSERT [dbo].[z_DataLoadTaskStatus] ([ID], [status], [description]) VALUES (1, N'Open', NULL)
GO
INSERT [dbo].[z_DataLoadTaskStatus] ([ID], [status], [description]) VALUES (2, N'Ready', NULL)
GO
INSERT [dbo].[z_DataLoadTaskStatus] ([ID], [status], [description]) VALUES (3, N'Committed', NULL)
GO
SET IDENTITY_INSERT [dbo].[z_DataLoadTaskStatus] OFF
GO
SET IDENTITY_INSERT [dbo].[z_FatalErrorStatus] ON 

GO
INSERT [dbo].[z_FatalErrorStatus] ([ID], [status]) VALUES (1, N'Outstanding')
GO
INSERT [dbo].[z_FatalErrorStatus] ([ID], [status]) VALUES (2, N'Resolved')
GO
INSERT [dbo].[z_FatalErrorStatus] ([ID], [status]) VALUES (3, N'Blocked')
GO
SET IDENTITY_INSERT [dbo].[z_FatalErrorStatus] OFF
GO
SET IDENTITY_INSERT [dbo].[z_RowErrorType] ON 

GO
INSERT [dbo].[z_RowErrorType] ([ID], [type]) VALUES (1, N'LoadRow')
GO
INSERT [dbo].[z_RowErrorType] ([ID], [type]) VALUES (2, N'Duplication')
GO
INSERT [dbo].[z_RowErrorType] ([ID], [type]) VALUES (3, N'Validation')
GO
INSERT [dbo].[z_RowErrorType] ([ID], [type]) VALUES (4, N'DatabaseOperation')
GO
INSERT [dbo].[z_RowErrorType] ([ID], [type]) VALUES (5, N'Unknown')
GO
SET IDENTITY_INSERT [dbo].[z_RowErrorType] OFF
GO

--create datasets
INSERT [dbo].[DataSet] ([dataSetID], [name], [description], [time_period], [SLA_required], [supplier_name], [supplier_tel_no], [supplier_email], [contact_name], [contact_position], [currentContactInstitutions], [contact_tel_no], [contact_email], [frequency], [method]) VALUES (N'DataExtraction', N'DataExtraction', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[DataSet] ([dataSetID], [name], [description], [time_period], [SLA_required], [supplier_name], [supplier_tel_no], [supplier_email], [contact_name], [contact_position], [currentContactInstitutions], [contact_tel_no], [contact_email], [frequency], [method]) VALUES (N'Internal', N'Internal', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
GO

--create tasks
INSERT [dbo].[DataLoadTask] ([ID], [description], [name], [createTime], [userAccount], [statusID], [isTest], [dataSetID]) VALUES (1, N'Internal', N'Internal', GETDATE(), N'Thomas', 1, 0, N'Internal')
GO
INSERT [dbo].[DataLoadTask] ([ID], [description], [name], [createTime], [userAccount], [statusID], [isTest], [dataSetID]) VALUES (2, N'DataExtraction', N'DataExtraction',  GETDATE(), N'Thomas', 1, 0, N'DataExtraction')
GO

CREATE FUNCTION [dbo].[GetSoftwareVersion]()
RETURNS nvarchar(50)
AS
BEGIN
	-- Return the result of the function
	RETURN (SELECT top 1 version from RoundhousE.Version order by version desc)
END