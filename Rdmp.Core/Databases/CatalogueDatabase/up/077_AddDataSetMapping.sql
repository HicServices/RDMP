--Version:8.1.1
--Description: Adds tables to model external datasets
 GO
-- Create Dataset table
if not exists(select 1 from sys.columns where object_id = OBJECT_ID('Dataset'))
BEGIN
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
END
GO

IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'Dataset_ID'
          AND Object_ID = Object_ID('ColumnInfo'))
BEGIN
ALTER TABLE [dbo].[ColumnInfo] ADD Dataset_ID [int] NULL
--ALTER TABLE [dbo].[ColumnInfo] ADD CONSTRAINT [FK_Column_Info_Dataset] FOREIGN KEY([Dataset_ID]) REFERENCES [dbo].[Dataset] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
END
