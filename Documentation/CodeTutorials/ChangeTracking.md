# Change Tracking

When RDMP [Platform Databases] get very large (e.g. 1,000+ [Catalogue], 1,000+ [ExtractionConfiguration], 1,000 cohort builder queries etc) the GUI Client refresh process (updating the UI after saving a change) can slow down.  This problem can be avoided by granting users the `VIEW CHANGE TRACKING` permission (this ensures that only new/changed objects are fetched back after saving).

## Enabling Change Tracking

Any user with the `dbo` role on the Catalogue or data export databases automatically benefits from this feature.  For regular users (e.g. db_reader / db_writer) you need to grant `VIEW CHANGE TRACKING`.  The easiest way to do this is with a `ROLE` in each database which users can then be assigned to.

For example if your Catalogue platform database is called `RDMP_Catalogue` and your data export database is called `RDMP_DataExport` then run the following:


```sql
USE RDMP_Catalogue;

CREATE ROLE rdmp_user

GRANT VIEW CHANGE TRACKING ON dbo.CohortAggregateContainer						  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.CohortAggregateSubContainer					  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.CohortIdentificationConfiguration				  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.CohortAggregateContainer_AggregateConfiguration to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.GovernanceDocument							  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.GovernancePeriod								  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.GovernancePeriod_Catalogue					  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.StandardRegex									  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.AnyTableSqlParameter							  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.PasswordEncryptionKeyLocation					  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.Plugin										  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.ANOTable										  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.AggregateConfiguration						  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.AggregateContinuousDateAxis					  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.AggregateDimension							  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.AggregateFilter								  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.AggregateFilterContainer						  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.AggregateFilterParameter						  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.AggregateForcedJoin							  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.Catalogue										  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.CatalogueItem									  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.CatalogueItemIssue							  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.ColumnInfo									  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.JoinableCohortAggregateConfiguration			  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.JoinableCohortAggregateConfigurationUse		  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.ExternalDatabaseServer						  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.ExtractionFilter								  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.ExtractionFilterParameter						  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.ExtractionInformation							  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.IssueSystemUser								  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.JoinInfo										  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.ExtractionFilterParameterSet					  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.LoadMetadata									  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.ExtractionFilterParameterSetValue				  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.LoadModuleAssembly							  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.LoadProgress									  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.Favourite										  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.Pipeline										  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.Lookup										  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.AggregateTopX									  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.PipelineComponent								  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.LookupCompositeJoinInfo						  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.PipelineComponentArgument						  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.PreLoadDiscardedColumn						  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.ProcessTask									  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.DashboardLayout								  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.ProcessTaskArgument							  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.ServerDefaults								  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.DashboardControl								  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.DataAccessCredentials							  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.SupportingDocument							  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.DataAccessCredentials_TableInfo				  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.DashboardObjectUse							  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.SupportingSQLTable							  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.TableInfo										  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.RemoteRDMP									  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.ObjectImport									  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.ObjectExport									  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.CacheProgress									  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.ConnectionStringKeyword						  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.WindowLayout									  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.PermissionWindow								  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.TicketingSystemConfiguration					  to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.CacheFetchFailure								  to rdmp_user;
	
USE RDMP_DataExport;

CREATE ROLE rdmp_user;

GRANT VIEW CHANGE TRACKING ON dbo.SupplementalExtractionResults							to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.ConfigurationProperties								to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.CumulativeExtractionResults							to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.DataUser												to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.DeployedExtractionFilter								to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.DeployedExtractionFilterParameter						to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.ExternalCohortTable									to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.ExtractableCohort										to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.ExtractableColumn										to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.ExtractableDataSet									to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.ExtractionConfiguration								to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.FilterContainer										to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.GlobalExtractionFilterParameter						to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.Project												to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.Project_DataUser										to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.ReleaseLog											to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.SelectedDataSets										to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.ExtractableDataSetPackage								to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.ExtractableDataSetPackage_ExtractableDataSet			to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.ProjectCohortIdentificationConfigurationAssociation	to rdmp_user;
GRANT VIEW CHANGE TRACKING ON dbo.SelectedDataSetsForcedJoin							to rdmp_user;
```

[Catalogue]: ./Glossary.md#Catalogue
[Platform Databases]: ./Glossary.md#Platform-Databases
[ExtractionConfiguration]: ./Glossary.md#ExtractionConfiguration
