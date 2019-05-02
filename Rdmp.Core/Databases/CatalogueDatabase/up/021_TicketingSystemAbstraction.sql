--Version:1.16.0.0
--Description: Removes the systems dependency on the HIC specific JIRA server as the sole source of ticketing (and releasability) and lets you configure a custom JIRA server (or any other plugin ticketing system with support for ITicketingSystem)
CREATE TABLE [dbo].[TicketingSystemConfiguration](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](1000) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[Url] [varchar](5000) NULL,
	[Type] [varchar](500) NULL,
	[DataAccessCredentials_ID] [int] NULL,
 CONSTRAINT [PK_TicketingSystemConfiguration] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)
)

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[TicketingSystemConfiguration]  WITH CHECK ADD  CONSTRAINT [FK_TicketingSystemConfiguration_DataAccessCredentials] FOREIGN KEY([DataAccessCredentials_ID])
REFERENCES [dbo].[DataAccessCredentials] ([ID])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[TicketingSystemConfiguration] CHECK CONSTRAINT [FK_TicketingSystemConfiguration_DataAccessCredentials]
GO

CREATE UNIQUE NONCLUSTERED INDEX [idx_CanOnlyBeOneActiveTicketingSystemConfiguration] ON [dbo].[TicketingSystemConfiguration]
(
	[IsActive] ASC
)
GO
 
 --rename all the JIRATicket fields to ticket
 if exists(select  1 from sys.columns where name = 'JIRATicket' and OBJECT_NAME(object_id) ='Catalogue')
begin
	exec sp_rename 'Catalogue.JIRATicket','Ticket','COLUMN'
end

 if exists(select  1 from sys.columns where name = 'JIRATicket' and OBJECT_NAME(object_id) ='CatalogueItemIssue')
begin
	exec sp_rename 'CatalogueItemIssue.JIRATicket','Ticket','COLUMN'
end

 if exists(select  1 from sys.columns where name = 'JIRATicket' and OBJECT_NAME(object_id) ='SupportingDocument')
begin
	exec sp_rename 'SupportingDocument.JIRATicket','Ticket','COLUMN'
end

 if exists(select  1 from sys.columns where name = 'JIRATicket' and OBJECT_NAME(object_id) ='SupportingSQLTable')
begin
	exec sp_rename 'SupportingSQLTable.JIRATicket','Ticket','COLUMN'
end


