--Version: 9.3.0
--Description: Add new metadata fields for catalogues
IF NOT EXISTS (SELECT 1 FROM sys.change_tracking_tables WHERE object_id = OBJECT_ID('Dataset')) 
	ALTER TABLE Dataset								    ENABLE CHANGE_TRACKING
IF NOT EXISTS (SELECT 1 FROM sys.change_tracking_tables WHERE object_id = OBJECT_ID('Favourite')) 
	ALTER TABLE Favourite								   ENABLE CHANGE_TRACKING
IF NOT EXISTS (SELECT 1 FROM sys.change_tracking_tables WHERE object_id = OBJECT_ID('ExtendedProperty')) 
	ALTER TABLE ExtendedProperty						ENABLE CHANGE_TRACKING
IF NOT EXISTS (SELECT 1 FROM sys.change_tracking_tables WHERE object_id = OBJECT_ID('Commit')) 
	ALTER TABLE [Commit]								ENABLE CHANGE_TRACKING
IF NOT EXISTS (SELECT 1 FROM sys.change_tracking_tables WHERE object_id = OBJECT_ID('RegexRedactionConfiguration')) 
	ALTER TABLE RegexRedactionConfiguration				ENABLE CHANGE_TRACKING
IF NOT EXISTS (SELECT 1 FROM sys.change_tracking_tables WHERE object_id = OBJECT_ID('RegexRedaction')) 
	ALTER TABLE RegexRedaction							ENABLE CHANGE_TRACKING
IF NOT EXISTS (SELECT 1 FROM sys.change_tracking_tables WHERE object_id = OBJECT_ID('RegexRedactionKey')) 
	ALTER TABLE RegexRedactionKey							ENABLE CHANGE_TRACKING