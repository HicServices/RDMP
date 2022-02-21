--Version:7.0.0
--Description: Adds Retry column to ExtractionProgress table.

  if(not exists (select * from sys.all_columns where name ='Retry' AND OBJECT_NAME(object_id) = 'ExtractionProgress'))
  begin
  	ALTER TABLE ExtractionProgress ADD Retry varchar(100) 
  end
  GO 

  UPDATE ExtractionProgress set Retry = 'NoRetry' where Retry is null
  ALTER TABLE ExtractionProgress ALTER COLUMN Retry varchar(100) not null