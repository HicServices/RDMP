--Version:1.0.0.1
--Description:Adds the ability to prevent loads too close to the current date, e.g. don't load any data until it is at least 6 months old (useful for a volatile datasource)
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='LoadSchedule' AND COLUMN_NAME='CacheLagPeriod')
BEGIN
	ALTER TABLE LoadSchedule ADD CacheLagPeriod varchar(10) NULL
END