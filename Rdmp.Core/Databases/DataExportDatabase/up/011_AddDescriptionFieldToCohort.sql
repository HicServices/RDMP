--Version:1.48.0.1
--Description: Adds a Description field onto ExtractableCohort
if not exists (select 1 from sys.columns where name = 'AuditLog' and OBJECT_NAME(object_id) = 'ExtractableCohort')
  begin
	alter table ExtractableCohort add AuditLog varchar(max) null
  end