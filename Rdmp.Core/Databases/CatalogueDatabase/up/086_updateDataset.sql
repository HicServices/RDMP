--Version: 8.3.1
--Description: Add new fields to the Dataset object


if not exists (select 1 from sys.tables where name = 'DatasetProviderConfiguration')
BEGIN
CREATE TABLE [dbo].[DatasetProviderConfiguration](
[ID] [int] IDENTITY(1,1) NOT NULL,
[Type] [varchar](256) NOT NULL,
[Url] [varchar](500),
[Name] [varchar](256) NOT NULL,
[DataAccessCredentials_ID] [int],
Organisation_ID [varchar](250),
PRIMARY KEY(ID),
  CONSTRAINT FK_DataAccessCredentials_ID FOREIGN KEY (DataAccessCredentials_ID)
    REFERENCES DataAccessCredentials(ID)
)
END
GO


if exists(select 1 from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME='Dataset' and COLUMN_NAME='Url')
BEGIN
ALTER TABLE [dbo].[Dataset]
ADD [Provider] [int] NULL;
ALTER TABLE [dbo].[Dataset]
ADD [Url] [varchar](500) NULL;
ALTER TABLE [dbo].[Dataset]
ADD [Type] [varchar](500) NULL;
ALTER TABLE [dbo].[Dataset]
ADD [Provider_ID] [int] NULL;
ALTER TABLE [dbo].[Dataset]
ADD FOREIGN KEY (Provider_ID) REFERENCES DatasetProviderConfiguration(ID);
END
GO