--Version:1.2.0.0
--Description: Allows you to set a Pivot dimension on data quality engine reports 

if not exists (select * from sys.columns where name = 'PivotCategory')
begin

	ALTER TABLE dbo.ColumnState ADD	PivotCategory varchar(50) NULL
	ALTER TABLE dbo.DQEGraphAnnotation ADD PivotCategory varchar(50) NULL
	ALTER TABLE dbo.PeriodicityState ADD PivotCategory varchar(50) NULL
	ALTER TABLE dbo.RowState ADD PivotCategory varchar(50) NULL
end
GO

if exists(select * from sys.columns where name = 'PivotCategory' and is_nullable = 1)

--Recreate primary key in ColumnState
begin
update ColumnState set PivotCategory = 'ALL' 
ALTER TABLE dbo.ColumnState alter column PivotCategory varchar(50) NOT NULL 
ALTER TABLE dbo.ColumnState DROP CONSTRAINT PK_ColumnState
ALTER TABLE dbo.ColumnState ADD CONSTRAINT PK_ColumnState PRIMARY KEY CLUSTERED 
	(
	Evaluation_ID,
	TargetProperty,
	DataLoadRunID,
	PivotCategory
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]


--Make PivotCategory not null in DQEGraphAnnotation
	update DQEGraphAnnotation set PivotCategory = 'ALL' 
ALTER TABLE dbo.DQEGraphAnnotation alter column PivotCategory varchar(50) NOT NULL 

--Recreate primary key in PeriodicityState
	update PeriodicityState set PivotCategory = 'ALL' 
ALTER TABLE dbo.PeriodicityState alter column PivotCategory varchar(50) NOT NULL 
ALTER TABLE dbo.PeriodicityState DROP CONSTRAINT PK_PeriodicityState
ALTER TABLE dbo.PeriodicityState ADD CONSTRAINT PK_PeriodicityState PRIMARY KEY CLUSTERED 
	(
	[Evaluation_ID] ASC,
	[Year] ASC,
	[Month] ASC,
	[RowEvaluation] ASC,
	PivotCategory
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

--Recreate primary key in RowState
update RowState set PivotCategory = 'ALL' 
ALTER TABLE dbo.RowState alter column PivotCategory varchar(50) NOT NULL 
ALTER TABLE dbo.RowState DROP CONSTRAINT PK_RowState
ALTER TABLE dbo.RowState ADD CONSTRAINT PK_RowState PRIMARY KEY CLUSTERED 
	(
	[Evaluation_ID] ASC,
	[DataLoadRunID] ASC,
	PivotCategory
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]


end