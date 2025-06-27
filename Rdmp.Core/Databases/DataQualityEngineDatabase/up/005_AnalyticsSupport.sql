--Version:9.0.0
--Description: Add support for Analytics engine
create table [DataLoadReport] (
	[ID] [int] identity(1,1) NOT NULL,
	[dataLoad_ID] [int] not null,
	[ok] [int] default 0,
	[warn] [int] default 0,
	[error] [int] default 0,
)
create table [DataLoadReplacementReport] (
	[ID] [int] identity(1,1) NOT NULL,
	[dataLoad_ID] [int] not null,
	[ok] [int] default 0,
	[warn] [int] default 0,
	[error] [int] default 0,
)
create table [CatalogueAnalyticConfiguration](
[ID] [int] identity(1,1) NOT NULL,
[catalogue_ID] [int] NOT NULL,
[configuration_XML] varchar(max) NOT NULL,
PRIMARY KEY(catalogue_ID)
)