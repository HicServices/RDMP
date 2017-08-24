--Version: 1.3.0.0
--Description: Increases width of password field in ExternalDatabaseServer to allow for storing encrypted passwords instead of freetext (the software library patch will also affect [DataAccessCredentials] table)
--if the length of the field in ExternalDatabaseServer is 50
  if( select max_length from sys.columns where name = 'Password' and object_id = OBJECT_ID('ExternalDatabaseServer')) = 50
  begin
  
	--make it 500
    alter table ExternalDatabaseServer alter column Password varchar(500)
  end