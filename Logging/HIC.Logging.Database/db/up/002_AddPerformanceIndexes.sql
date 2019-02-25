--Version:2.12.0.1
--Description:Adds indexes in foreign key columns to allow for very large logging databases


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'ixFatalError_DataLoadRun')
BEGIN

CREATE NONCLUSTERED INDEX ixFatalError_DataLoadRun ON FatalError
(
	[dataLoadRunID] ASC
)

END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'ixProgressLog_DataLoadRun')
BEGIN

CREATE NONCLUSTERED INDEX ixProgressLog_DataLoadRun ON ProgressLog
(
	[dataLoadRunID] ASC
)

END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'ixTableLoadRun_DataLoadRun')
BEGIN

CREATE NONCLUSTERED INDEX ixTableLoadRun_DataLoadRun ON TableLoadRun
(
	[dataLoadRunID] ASC
)

END
