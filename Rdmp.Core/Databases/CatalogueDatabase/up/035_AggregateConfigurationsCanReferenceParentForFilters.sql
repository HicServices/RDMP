--Version:1.29.0.0
--Description: Adds new field OverrideFiltersByUsingParentAggregateConfigurationInstead_ID to AggregateConfiguration, this lets you clone an aggregate but link it to its parent creating a 'single version of the truth' for Filter logic and hence record selection but having different dimensions/group bys.  This lets you have a master e.g. for cohort identification and many children which are effectively views onto that record selection which can be used to generate graphs/tables that provide evidence to support the core aggregate

if not exists ( select 1 from sys.columns where name = 'OverrideFiltersByUsingParentAggregateConfigurationInstead_ID')
begin
alter table [AggregateConfiguration] add [OverrideFiltersByUsingParentAggregateConfigurationInstead_ID] [int] NULL
end

if not exists (select 1 from sys.foreign_keys where name = 'FK_OverrideRootFilterContainerToUseParents') 
begin
ALTER TABLE [dbo].[AggregateConfiguration]  WITH CHECK ADD  CONSTRAINT [FK_OverrideRootFilterContainerToUseParents] FOREIGN KEY([OverrideFiltersByUsingParentAggregateConfigurationInstead_ID])
REFERENCES [dbo].[AggregateConfiguration] ([ID])
end

