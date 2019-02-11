--Version:2.12.0.1
--Description: Database constraints Name properties are never null
update ExternalCohortTable set Name = 'NoName' where Name is null
alter table ExternalCohortTable alter column Name varchar(1000) not null

update Project set Name = 'NoName' where Name is null
alter table Project alter column Name varchar(1000) not null