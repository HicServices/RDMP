----Version: 9.3.0
----Description: Allow a catalogue to be linked to multiple projects

IF NOT EXISTS (SELECT 1 FROM sys.change_tracking_tables WHERE object_id = OBJECT_ID('ExtractableDataSetProject')) 
	ALTER TABLE ExtractableDataSetProject								   ENABLE CHANGE_TRACKING