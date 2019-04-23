--Version:1.3.0.0
--Description: Fixes a bug where [PeriodicityState] table did not have a CASCADE from Evaluation table where every other table did
alter table PeriodicityState drop constraint FK_PeriodicityState_Evaluation
GO
ALTER TABLE [dbo].[PeriodicityState]  WITH CHECK ADD  CONSTRAINT [FK_PeriodicityState_Evaluation] FOREIGN KEY([Evaluation_ID])
REFERENCES [dbo].[Evaluation] ([ID])
ON DELETE CASCADE
GO