if not exists (select 1 from sys.tables where name = 'DatasetProviderConfiguration')
BEGIN
CREATE TABLE [dbo].[DatasetProviderConfiguration](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	Type [varchar(256)] NOT NULL,
	Url [varchar(500)] NOT NULL,
	Name [varchar(256)] NOT NULL,
	DataAccessCredentials_ID [int] NOT NULL,
	Organisation_ID [varchar(256)] NOT NULL
	CONSTRAINT FK_DatasetProviderConfiguration_DataAccessCredentials FOREIGN KEY (DataAccessCredentials_ID) REFERENCES DataAccessCredentials(ID)  ON DELETE CASCADE,
CONSTRAINT [PK_RegexRedactionKey] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
