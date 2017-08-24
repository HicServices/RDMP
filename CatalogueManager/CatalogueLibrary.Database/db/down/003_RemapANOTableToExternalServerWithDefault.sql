alter table ANOTable drop column Server
GO

alter table ANOTable add Server_ID int
GO

ALTER TABLE [dbo].[ANOTable]  WITH CHECK ADD CONSTRAINT [FK_ANOTable_ExternalDatabaseServer] FOREIGN KEY(Server_ID)
REFERENCES [dbo].[ExternalDatabaseServer] ([ID])
GO

--todo update defaults

alter table ANOTable alter column Server_ID int not null
GO