--Version:1.42.0.1
--Description: Fixes length of password column in ExternalCohortTable to accommodate long encrypted passwords (e.g. 1500 characters).  Also an index to prevent adding same column twice to a configuration
if exists (select 1 from sys.columns where name = 'Password' and max_length = 500)
begin
	alter table ExternalCohortTable alter column Password varchar(max)
end