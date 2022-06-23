--Version:1.8.0.0
--Description: Changes [FK_Project_DataUser_Project] from 'NO ACTION' to 'CASCADE' on delete (allows you to delete a project and have its users m-m relationships deleted)
if exists (select * from sys.foreign_keys where name='FK_Project_DataUser_Project' and delete_referential_action = 0 /*NO_ACTION*/)
begin

--to start with we know it is delete_referential_action = 2 so we want it to be 0 (the default) so drop it
ALTER TABLE Project_DataUser drop constraint FK_Project_DataUser_Project

--and recreate it with CASCADE
ALTER TABLE [dbo].[Project_DataUser]  WITH CHECK ADD  CONSTRAINT [FK_Project_DataUser_Project] FOREIGN KEY([Project_ID])
REFERENCES [dbo].[Project] ([ID])
ON DELETE CASCADE

end


   