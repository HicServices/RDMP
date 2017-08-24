--Version:1.6.0.0
--Description: Increases the allowed length of [ConfigurationProperties].[Value] to max which allows for big long paragraphs of text which is a requirement for storing agency specific release document header
if(select max_length from sys.columns where name = 'Value' and object_id = OBJECT_ID('ConfigurationProperties')) = 500 
begin
alter table ConfigurationProperties alter column Value varchar(max)
end


   