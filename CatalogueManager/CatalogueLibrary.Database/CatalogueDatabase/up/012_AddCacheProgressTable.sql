--Version:1.10.0.0
--Description:Adds CacheProgress table, migrates relevant data from LoadSchedule table and removes redundant columns from LoadSchedule
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='CacheProgress')
BEGIN
CREATE TABLE [CacheProgress](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[LoadSchedule_ID] [int] NOT NULL,
	[PermissionWindow_ID] [int] NULL,
	[CacheFillProgress] [datetime] NULL,
	[CacheLagPeriod] [varchar](10) NULL,
	[AlternativeCacheLocation] [varchar](3000) NULL,
	[RetrieverType] [varchar](3000) NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_CacheProgress] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END

-- ADD NEW FOREIGN KEY COLUMN to LoadSchedule
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='LoadSchedule' AND COLUMN_NAME='CacheProgress_ID')
BEGIN
ALTER TABLE LoadSchedule ADD CacheProgress_ID int NULL
END

-- Only do the migration if we haven't previously executed this script. We can check that by looking for 'AlternativeCacheLocation', the last column to be removed. If it is still there, we haven't successfully executed this script.
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='LoadSchedule' AND COLUMN_NAME='AlternativeCacheLocation')
BEGIN

-- MIGRATE Load Schedule data to CacheProgress
-- first copy relevant data from LoadSchedule table into CacheProgress
-- This isn't 'dynamic' but is in an EXEC to support idempotency of this script. If the columns aren't in LoadSchedule (i.e. this script has been executed before) then the SQL fails, complaining that the columns don't exist.
EXEC('INSERT INTO [CacheProgress]
(LoadSchedule_ID, CacheFillProgress, CacheLagPeriod, AlternativeCacheLocation, SoftwareVersion)
SELECT ID, CacheProgress, CacheLagPeriod, AlternativeCacheLocation, dbo.GetSoftwareVersion() FROM [LoadSchedule]')

-- UPDATE FOREIGN KEYS in LoadSchedule to point to the newly inserted records in CacheProgress
UPDATE ls
SET ls.CacheProgress_ID = cp.ID
FROM [LoadSchedule] ls
INNER JOIN [CacheProgress] cp
	ON ls.ID = cp.LoadSchedule_ID
END 

-- And finally, DROP relevant columns in LoadSchedule
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='LoadSchedule' AND COLUMN_NAME='CacheProgress')
BEGIN
	ALTER TABLE LoadSchedule DROP COLUMN CacheProgress
END

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='LoadSchedule' AND COLUMN_NAME='CacheLagPeriod')
BEGIN
	ALTER TABLE LoadSchedule DROP COLUMN CacheLagPeriod
END

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='LoadSchedule' AND COLUMN_NAME='AlternativeCacheLocation')
BEGIN
	ALTER TABLE LoadSchedule DROP COLUMN AlternativeCacheLocation
END
