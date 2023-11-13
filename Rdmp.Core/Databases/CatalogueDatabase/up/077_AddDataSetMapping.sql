--Version:8.1.1
--Description: Adds tables to model external datasets
 GO
-- Create Dataset table
if not exists(select 1 from sys.columns where object_id = OBJECT_ID('Dataset'))
CREATE TABLE [dbo].Dataset(
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](256) NOT NULL,
	[DigitalObjectIdentifier] [varchar](256) NULL, CONSTRAINT [PK_Dataset] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

if not exists (select 1 from sys.columns where name = 'Dataset' AND object_id = OBJECT_ID('ColumnInfo'))
begin
ALTER TABLE [dbo].[ColumnInfo] ADD Dataset_ID [int] NULL
end
begin
ALTER TABLE [dbo].[ColumnInfo]  WITH CHECK ADD  CONSTRAINT [FK_Column_Info_Dataset] FOREIGN KEY([Dataset_ID])
REFERENCES [dbo].[Dataset] ([ID])
end


--ALTER TABLE [dbo].[ANOTable]  WITH CHECK ADD  CONSTRAINT [FK_ANOTable_ExternalDatabaseServer] FOREIGN KEY([Server_ID])
--REFERENCES [dbo].[ExternalDatabaseServer] ([ID])