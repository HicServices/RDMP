﻿----Version: 8.4.0
----Description: Add Order to Aggregate Filters

if not exists (select 1 from sys.columns where name = 'Order' and OBJECT_NAME(object_id) = 'AggregateFilter')
BEGIN
ALTER TABLE [dbo].[AggregateFilter]
ADD [Order] [int] NOT NULL DEFAULT 0 WITH VALUES
END
if not exists (select 1 from sys.columns where name = 'Order' and OBJECT_NAME(object_id) = 'ExtractionFilter')
BEGIN
ALTER TABLE [dbo].[ExtractionFilter]
ADD [Order] [int] NOT NULL DEFAULT 0 WITH VALUES
END