--Version: 8.2.1
--Description: Add lookup table of release status names for a ticketing configuration


if not exists (select 1 from sys.tables where name = 'TicketingSystemReleaseStatus')
BEGIN
CREATE TABLE [dbo].[TicketingSystemReleaseStatus](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Status] [nvarchar](250) NOT NULL,
	[TicketingSystemConfigurationID] [int] NOT NULL,
	    FOREIGN KEY (TicketingSystemConfigurationID) REFERENCES TicketingSystemConfiguration(ID),

CONSTRAINT [PK_TicketingSystemReleaseStatus] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO

-- add cascade delete when we delete a ticketing system