alter table Catalogue DROP constraint df_LiveLoggingServer_ID
alter table Catalogue DROP constraint df_TestLoggingServer_ID
alter table TableInfo DROP constraint df_IdentifierDumpServer_ID
GO

EXEC sp_rename 'Catalogue.LiveLoggingServer_ID', 'NewLiveLoggingServer_ID', 'COLUMN';
EXEC sp_rename 'Catalogue.TestLoggingServer_ID', 'NewTestLoggingServer_ID', 'COLUMN';
EXEC sp_rename 'Catalogue.LoggingDataTask', 'NewLogging_DataTask', 'COLUMN';

DROP FUNCTION GetDefaultExternalServerIDFor
DROP TABLE [dbo].[ServerDefaults]