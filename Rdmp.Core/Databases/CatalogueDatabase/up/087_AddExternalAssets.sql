--Version: 8.4.0
--Description: Add External Assets Table

-- todo add the ticketing config as an FK

if not exists (select 1 from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME='ExternalAsset')
BEGIN
CREATE TABLE [dbo].[ExternalAsset](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](250) NOT NULL,
	[ExternalAssetID] int NOT NULL,
	[TicketingConfigurationID] int NOT NULL,
	[ObjectID] int NOT NULL,
	[ObjectType] [varchar](500) NOT NULL,
 CONSTRAINT [PK_ExternalAssetsKey] PRIMARY KEY CLUSTERED
(
	[TicketingConfigurationID] ASC,
	[ExternalAssetID] ASC,
	[ObjectId] ASC,
	[ObjectType] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
