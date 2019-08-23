--Version:3.2.0
--Description: Changes all varchar(x) columns that involve user provided values into nvarchar(x)

alter table [CachedAggregateConfigurationResults] alter column [TableName] nvarchar(500)NOT NULL