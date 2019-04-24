/****** Object:  Table [dbo].[SMR01_MIGRATION_TEST_EXPECTED]    Script Date: 22/09/2014 11:40:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[SMR01_MIGRATION_TEST_EXPECTED](
	[CHI] [int] NOT NULL,
	[Description] [varchar](max) NULL,
	[EPISODE_RECORD_KEY] [varchar](max) NULL,
	[SENDING_LOCATION] [varchar](max) NULL,
	[hic_validFrom] [datetime] NULL,
	[hic_sourceID] [int] NULL,
	[hic_dataLoadRunID] [int] NULL
 CONSTRAINT [PK_SMR01_MIGRATION_TEST_EXPECTED] PRIMARY KEY CLUSTERED 
(
	[CHI] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
INSERT [dbo].[SMR01_MIGRATION_TEST_EXPECTED] ([CHI], [Description], [EPISODE_RECORD_KEY], [SENDING_LOCATION], [hic_validFrom], [hic_sourceID], [hic_dataLoadRunID]) VALUES (1, N'New description', N'ER1', N'SL1', GETDATE(), 16, 2)
GO
INSERT [dbo].[SMR01_MIGRATION_TEST_EXPECTED] ([CHI], [Description], [EPISODE_RECORD_KEY], [SENDING_LOCATION], [hic_validFrom], [hic_sourceID], [hic_dataLoadRunID]) VALUES (2, N'Original description', N'ER2', N'SL1', CAST(0x0000A3AE00BE9CE0 AS DateTime), 16, 1)
GO
INSERT [dbo].[SMR01_MIGRATION_TEST_EXPECTED] ([CHI], [Description], [EPISODE_RECORD_KEY], [SENDING_LOCATION], [hic_validFrom], [hic_sourceID], [hic_dataLoadRunID]) VALUES (3, N'Original description', N'ER3', N'SL1', GETDATE(), 16, 2)
GO
INSERT [dbo].[SMR01_MIGRATION_TEST_EXPECTED] ([CHI], [Description], [EPISODE_RECORD_KEY], [SENDING_LOCATION], [hic_validFrom], [hic_sourceID], [hic_dataLoadRunID]) VALUES (4, N'Original description', N'ER4', N'SL1', GETDATE(), 16, 2)
GO
INSERT [dbo].[SMR01_MIGRATION_TEST_EXPECTED] ([CHI], [Description], [EPISODE_RECORD_KEY], [SENDING_LOCATION], [hic_validFrom], [hic_sourceID], [hic_dataLoadRunID]) VALUES (5, N'Original description', N'ER5', N'SL1', GETDATE(), 16, 2)
GO