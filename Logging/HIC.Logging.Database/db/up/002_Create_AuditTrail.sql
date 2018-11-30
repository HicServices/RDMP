--Version:2.10.0.1
--Description:Creates new AuditTrail TABLE

/****** Object:  Table [dbo].[AuditTrail]    Script Date: 28/11/2018 16:05:16 ******/
if not exists(select 1 from sys.tables where name ='AuditTrail')
BEGIN
    CREATE TABLE [dbo].[AuditTrail](
	    [ID] [int] IDENTITY(1,1) NOT NULL,
	    [Message] [varchar](max) NULL,
	    [User] [varchar](50) NULL,
     CONSTRAINT [PK_AuditTrail] PRIMARY KEY CLUSTERED 
    (
	    [ID] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
