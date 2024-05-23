--Version: 8.2.0
--Description: Adds unified instance settings
if not exists (select 1 from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME='CohortConfigurationVersion')
BEGIN
CREATE TABLE [dbo].CohortConfigurationVersion(
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](450) NOT NULL,
	[CohortIdentificationConfigurationId] [int] NOT NULL
	)
END
