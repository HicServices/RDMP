--Version:5.0.1
--Description: Changes ExtractableColumn SelectSQL to nvarchar(max) to match the size in Catalogue database


alter table [ExtractableColumn] alter column [SelectSQL] nvarchar(max) NULL