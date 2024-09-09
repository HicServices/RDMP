--Version: 8.2.4
--Description: Add configuration for regex redaction


if not exists (select 1 from sys.tables where name = 'RegexRedactionConfiguration')
BEGIN
CREATE TABLE [dbo].RegexRedactionConfiguration(
	[ID] [int] IDENTITY(1,1) NOT NULL,
	Name [nvarchar](250) NOT NULL,
	Description [nvarchar](250),
	RegexPattern [nvarchar](250) NOT NULL,
	RedactionString [nvarchar](250) NOT NULL,
	Folder [nvarchar](max) NOT NULL DEFAULT '\'
CONSTRAINT [PK_RegexRedactionConfiguration] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO


if not exists (select 1 from sys.tables where name = 'RegexRedaction')
BEGIN
CREATE TABLE [dbo].RegexRedaction(
	[ID] [int] IDENTITY(1,1) NOT NULL,
	RedactionConfiguration_ID [int] NOT NULL,
	ColumnInfo_ID [int] NOT NULL,
	StartingIndex [int] NOT NULL,
	RedactedValue [nvarchar](250),
	ReplacementValue [nvarchar](250) NOT NULL,
	CONSTRAINT FK_Redaction_RedactionConfiguration_ID FOREIGN KEY (RedactionConfiguration_ID) REFERENCES RegexRedactionConfiguration(ID),
    CONSTRAINT FK_Redaction_ColumnInfo_ID FOREIGN KEY (ColumnInfo_ID) REFERENCES ColumnInfo(ID) ON DELETE CASCADE,
CONSTRAINT [PK_RegexRedaction] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO

if not exists (select 1 from sys.tables where name = 'RegexRedactionKey')
BEGIN
CREATE TABLE [dbo].RegexRedactionKey(
	[ID] [int] IDENTITY(1,1) NOT NULL,
	RegexRedaction_ID [int] NOT NULL,
	ColumnInfo_ID [int] NOT NULL,
	Value [nvarchar](max),
	CONSTRAINT FK_RedactionKey_Redaction_ID FOREIGN KEY (RegexRedaction_ID) REFERENCES RegexRedaction(ID)  ON DELETE CASCADE,
	CONSTRAINT FK_RedactionKey_ColumnInfo_ID FOREIGN KEY (ColumnInfo_ID) REFERENCES ColumnInfo(ID),
CONSTRAINT [PK_RegexRedactionKey] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO

