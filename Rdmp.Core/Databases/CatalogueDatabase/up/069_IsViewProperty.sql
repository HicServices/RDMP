--Version:3.1.0
--Description: Adds the IsView property to TableInfo table
if not exists (select * from sys.all_columns where name = 'IsView')
	ALTER TABLE TableInfo
		ADD IsView bit NOT NULL 
		CONSTRAINT DF_IsView DEFAULT 0
		WITH VALUES;

