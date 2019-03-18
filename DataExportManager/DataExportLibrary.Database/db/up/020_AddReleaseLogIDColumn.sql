--Version:2.13.0.1
--Description: Adds a column called ID to ReleaseLog allowing it to be created/deleted more easily.

  if( exists (select * from sys.key_constraints where type ='PK' AND OBJECT_NAME(parent_object_id) = 'ReleaseLog'))
	ALTER TABLE ReleaseLog DROP CONSTRAINT PK_ReleaseLog;  

  if(not exists (select * from sys.all_columns where name ='ID' AND OBJECT_NAME(object_id) = 'ReleaseLog'))
  	ALTER TABLE ReleaseLog ADD ID INT IDENTITY(1,1)

  if(not exists (select * from sys.key_constraints where type ='PK' AND OBJECT_NAME(parent_object_id) = 'ReleaseLog'))
	ALTER TABLE ReleaseLog ADD CONSTRAINT PK_ReleaseLog PRIMARY KEY (ID);
  