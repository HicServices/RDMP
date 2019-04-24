--Version:1.39.0.0
--Description: Removes limitation on SQL Parameter values being 500 characters, they can now be as long as you like
if exists (select 1 from sys.columns where name = 'Value' and max_length = 500)
begin
	alter table GlobalExtractionFilterParameter alter column Value varchar(max)
end