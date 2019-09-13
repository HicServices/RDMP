--Version:3.2.0
--Description: Changes all varchar(x) columns that involve user provided values into nvarchar(x)

alter table [DQEGraphAnnotation] alter column [Text] nvarchar(500)NOT NULL
alter table [DQEGraphAnnotation] alter column [Username] nvarchar(500)NOT NULL
alter table [DQEGraphAnnotation] alter column [PivotCategory] nvarchar(500)NOT NULL -- increased this from 50 too 500 since pivot values come from source data and could well be longer than 50