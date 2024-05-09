--Version: 8.2.0
--Description: Adds unified instance settings
if not exists (select 1 from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME='Setting')
BEGIN
CREATE TABLE [dbo].[Setting](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Key] [varchar](max) NOT NULL,
	[Value] [varchar](max) NOT NULL,
 CONSTRAINT [PK_SettingKey] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
