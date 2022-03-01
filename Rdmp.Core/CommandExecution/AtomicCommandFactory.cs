// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution.AtomicCommands.Alter;
using Rdmp.Core.CommandExecution.AtomicCommands.CatalogueCreationCommands;
using Rdmp.Core.CommandExecution.AtomicCommands.CohortCreationCommands;
using Rdmp.Core.CommandExecution.AtomicCommands.Sharing;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.Curation.Data.Referencing;
using Rdmp.Core.Curation.FilterImporting.Construction;
using Rdmp.Core.Databases;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataViewing;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Logging;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.Providers.Nodes.CohortNodes;
using Rdmp.Core.Providers.Nodes.LoadMetadataNodes;
using Rdmp.Core.Providers.Nodes.PipelineNodes;
using Rdmp.Core.Providers.Nodes.ProjectCohortNodes;
using Rdmp.Core.Providers.Nodes.SharingNodes;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Rdmp.Core.CommandExecution
{
    /// <summary>
    /// Builds lists of <see cref="IAtomicCommand"/> for any given RDMP object
    /// </summary>
    public class AtomicCommandFactory : CommandFactoryBase
    {
        IBasicActivateItems _activator;
        GoToCommandFactory _goto;
        public const string Add = "Add";
        public const string New = "New";
        public const string GoTo = "Go To";
        public const string Extraction = "Extractability";
        public const string Metadata = "Metadata";
        public const string Alter = "Alter";
        public const string SetUsageContext = "Set Context";
        public const string SetContainerOperation = "Set Operation";
        public const string Dimensions = "Dimensions";
        public const string Advanced = "Advanced";

        public AtomicCommandFactory(IBasicActivateItems activator)
        {
            _activator = activator;
            _goto = new GoToCommandFactory(_activator);
        }

        /// <summary>
        /// Returns all commands that could be run involving <paramref name="o"/> in order of most useful to least useful
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public IEnumerable<IAtomicCommand> CreateCommands(object o)
        {
            foreach(var cmd in _goto.GetCommands(o))
            {
                cmd.SuggestedCategory = GoTo;
                yield return cmd;
            }

            if(_activator.CanActivate(o))
                yield return new ExecuteCommandActivate(_activator,o);

            if(Is(o, out ILoggedActivityRootObject root)){
                yield return new ExecuteCommandViewLogs(_activator,root);
            }

            if(Is(o,out Catalogue c))
            {
                bool isApiCall = c.IsApiCall();

                if (!isApiCall)
                {
                    yield return new ExecuteCommandViewCatalogueData(_activator, c, -1);
                }
                

                yield return new ExecuteCommandAddNewSupportingSqlTable(_activator, c) { SuggestedCategory = Add };
                yield return new ExecuteCommandAddNewSupportingDocument(_activator, c) { SuggestedCategory = Add };

                if (!isApiCall)
                {
                    yield return new ExecuteCommandAddNewAggregateGraph(_activator, c) { SuggestedCategory = Add };
                }

                yield return new ExecuteCommandAddNewCatalogueItem(_activator, c) { SuggestedCategory = Add };


                if (!isApiCall)
                {
                    yield return new ExecuteCommandChangeExtractability(_activator, c) { SuggestedCategory = Extraction };
                    yield return new ExecuteCommandMakeCatalogueProjectSpecific(_activator, c, null) { SuggestedCategory = Extraction };
                    yield return new ExecuteCommandMakeProjectSpecificCatalogueNormalAgain(_activator, c) { SuggestedCategory = Extraction };
                    yield return new ExecuteCommandSetExtractionIdentifier(_activator, c, null, null) { SuggestedCategory = Extraction };
                }

                yield return new ExecuteCommandExportObjectsToFile(_activator, new[] {c}) { SuggestedCategory = Metadata };
                yield return new ExecuteCommandExtractMetadata(_activator, new []{ c},null,null,null,false,null) { SuggestedCategory = Metadata };
            }

            if(Is(o,out CatalogueFolder cf))
            {
                yield return new ExecuteCommandCreateNewCatalogueByImportingExistingDataTable(_activator)
                {
                    TargetFolder = cf
                };
                yield return new ExecuteCommandCreateNewEmptyCatalogue(_activator){
                    TargetFolder = cf
                };
            }

            if(Is(o, out ExtractionInformation ei))
            {
                yield return new ExecuteCommandCreateNewFilter(_activator, new ExtractionFilterFactory(ei));
                yield return new ExecuteCommandCreateNewCohortFromCatalogue(_activator, ei);
                yield return new ExecuteCommandChangeExtractionCategory(_activator, ei);

                yield return new ExecuteCommandViewData(_activator, ViewType.TOP_100, ei) { SuggestedCategory = "View Data" };
                yield return new ExecuteCommandViewData(_activator, ViewType.Aggregate, ei) { SuggestedCategory = "View Data" };
                yield return new ExecuteCommandViewData(_activator, ViewType.Distribution, ei) { SuggestedCategory = "View Data" };
            }

            if(Is(o,out ExtractionFilter cataFilter))
            {
                yield return new ExecuteCommandAddNewExtractionFilterParameterSet(_activator, cataFilter);
            }

            if (Is(o,out ExtractionFilterParameterSet efps))
                yield return new ExecuteCommandAddMissingParameters(_activator, efps);

            if (Is(o, out ISqlParameter p) && p is IMapsDirectlyToDatabaseTable m)
            {
                yield return new ExecuteCommandSet(_activator, m, p.GetType().GetProperty(nameof(ISqlParameter.Value)));

                if(p is not ExtractionFilterParameterSetValue)
                    yield return new ExecuteCommandSet(_activator, m, p.GetType().GetProperty(nameof(ISqlParameter.ParameterSQL)));
            }

            if(Is(o,out  CatalogueItem ci))
            {
                yield return new ExecuteCommandLinkCatalogueItemToColumnInfo(_activator, ci);
                yield return new ExecuteCommandMakeCatalogueItemExtractable(_activator, ci);
                yield return new ExecuteCommandChangeExtractionCategory(_activator, ci.ExtractionInformation);
                yield return new ExecuteCommandImportCatalogueItemDescription(_activator, ci){SuggestedShortcut= "I",Ctrl=true };

                var ciExtractionInfo = ci.ExtractionInformation;
                if(ciExtractionInfo != null)
                {
                    yield return new ExecuteCommandViewData(_activator, ViewType.TOP_100, ciExtractionInfo) { SuggestedCategory = "View Data" };
                    yield return new ExecuteCommandViewData(_activator, ViewType.Aggregate, ciExtractionInfo) { SuggestedCategory = "View Data" };
                    yield return new ExecuteCommandViewData(_activator, ViewType.Distribution, ciExtractionInfo) { SuggestedCategory = "View Data" };
                }
            }

            if(Is(o, out SupportingSQLTable sqlTable))
            {
                yield return new ExecuteCommandRunSupportingSql(_activator,sqlTable);
            }

            if(Is(o,out  AggregateConfiguration ac) && !ac.Catalogue.IsApiCall())
            {
                yield return new ExecuteCommandViewSample(_activator, ac);
                yield return new ExecuteCommandAddNewFilterContainer(_activator,ac);
                yield return new ExecuteCommandImportFilterContainerTree(_activator,ac);
                yield return new ExecuteCommandCreateNewFilter(_activator,ac);

                yield return new ExecuteCommandAddParameter(_activator, ac, null,null,null);

                // graph options
                yield return new ExecuteCommandAddDimension(_activator, ac) { SuggestedCategory = Dimensions };
                yield return new ExecuteCommandSetPivot(_activator, ac) { SuggestedCategory = Dimensions };
                yield return new ExecuteCommandSetPivot(_activator, ac, null) { OverrideCommandName = "Clear Pivot", SuggestedCategory = Dimensions };
                yield return new ExecuteCommandSetAxis(_activator, ac) { SuggestedCategory = Dimensions };
                yield return new ExecuteCommandSetAxis(_activator, ac, null) { OverrideCommandName = "Clear Axis", SuggestedCategory = Dimensions };


                yield return new ExecuteCommandCreateNewFilterFromCatalogue(_activator,ac);
                
                if(ac.OverrideFiltersByUsingParentAggregateConfigurationInstead_ID != null)
                {
                    yield return new ExecuteCommandSetFilterTreeShortcut(_activator, ac, null) { OverrideCommandName = "Clear Filter Tree Shortcut" };
                }
                else
                {
                    yield return new ExecuteCommandSetFilterTreeShortcut(_activator, ac);
                }
                
                

                //only allow them to execute graph if it is normal aggregate graph
                if (!ac.IsCohortIdentificationAggregate)
                    yield return new ExecuteCommandExecuteAggregateGraph(_activator, ac);

                yield return new ExecuteCommandCreateNewCatalogueByExecutingAnAggregateConfiguration(_activator,ac);
            }
            
            if(Is(o,out  IContainer container))
            {
                string targetOperation = container.Operation == FilterContainerOperation.AND ? "OR" : "AND";

                yield return new ExecuteCommandSet(_activator,container,nameof(IContainer.Operation),targetOperation){OverrideCommandName = $"Set Operation to {targetOperation}" };
                
                yield return new ExecuteCommandCreateNewFilter(_activator,container.GetFilterFactory(),container);
                yield return new ExecuteCommandCreateNewFilterFromCatalogue(_activator, container);
                yield return new ExecuteCommandAddNewFilterContainer(_activator,container){OverrideCommandName = "Add SubContainer" };
               
                yield return new ExecuteCommandViewFilterMatchData(_activator, container, ViewType.TOP_100);
                yield return new ExecuteCommandViewFilterMatchData(_activator, container, ViewType.Aggregate);
            }
            
            if(Is(o,out AggregatesNode an))
                yield return new ExecuteCommandAddNewAggregateGraph(_activator, an.Catalogue);

            if(Is(o,out AllANOTablesNode _))
            {
                yield return new ExecuteCommandCreateNewANOTable(_activator);
            
                yield return new ExecuteCommandCreateNewExternalDatabaseServer(_activator,
                new ANOStorePatcher(), PermissableDefaults.ANOStore) 
                { OverrideCommandName = "Create ANOStore Database" };

                yield return new ExecuteCommandExportObjectsToFile(_activator,_activator.CoreChildProvider.AllANOTables);
            }

            if(Is(o,out AllCataloguesUsedByLoadMetadataNode aculmd))
            {
                yield return new ExecuteCommandAssociateCatalogueWithLoadMetadata(_activator, aculmd.LoadMetadata);
            }

            if(Is(o,out AllDataAccessCredentialsNode _))
            {
                yield return new ExecuteCommandNewObject(_activator,
                    ()=>new DataAccessCredentials(_activator.RepositoryLocator.CatalogueRepository, "New Blank Credentials " + Guid.NewGuid()))
                    {
                        OverrideCommandName= "Add New Credentials"
                    };
            }

            if(Is(o,out DataAccessCredentialUsageNode usage))
            {
                var existingUsages = _activator.RepositoryLocator.CatalogueRepository.TableInfoCredentialsManager.GetCredentialsIfExistsFor(usage.TableInfo);

                foreach (DataAccessContext context in Enum.GetValues(typeof(DataAccessContext)))
                    yield return new ExecuteCommandSetDataAccessContextForCredentials(_activator, usage, context, existingUsages){
                        SuggestedCategory = SetUsageContext 
                    };
            }

            if(Is(o,out AllConnectionStringKeywordsNode _))
            {
                yield return new ExecuteCommandNewObject(_activator,
                    ()=>new ConnectionStringKeyword(_activator.RepositoryLocator.CatalogueRepository,DatabaseType.MicrosoftSQLServer,"NewKeyword", "v"))
                    {
                        OverrideCommandName= "Add New Connection String Keyword"
                    };
            }

            if(Is(o,out AllExternalServersNode _))
            {
                yield return new ExecuteCommandCreateNewExternalDatabaseServer(_activator, null,PermissableDefaults.None);
                
                var assemblyDictionary = new Dictionary<PermissableDefaults, IPatcher>()
                {
                    {PermissableDefaults.DQE, new DataQualityEnginePatcher() },
                    {PermissableDefaults.WebServiceQueryCachingServer_ID, new QueryCachingPatcher()},
                    {PermissableDefaults.LiveLoggingServer_ID, new LoggingDatabasePatcher()},
                    {PermissableDefaults.IdentifierDumpServer_ID, new IdentifierDumpDatabasePatcher()},
                    {PermissableDefaults.ANOStore, new ANOStorePatcher()},
                    {PermissableDefaults.CohortIdentificationQueryCachingServer_ID, new QueryCachingPatcher()}
                };

                foreach (var kvp in assemblyDictionary)
                    yield return new ExecuteCommandCreateNewExternalDatabaseServer(_activator, kvp.Value, kvp.Key);
            }

            if(Is(o, out ExternalDatabaseServer eds))
            {
                if(eds.WasCreatedBy(new LoggingDatabasePatcher()))
                {
                    yield return new ExecuteCommandViewLogs(_activator,eds,new LogViewerFilter(LoggingTables.DataLoadRun));
                    yield return new ExecuteCommandViewLogs(_activator,eds,new LogViewerFilter(LoggingTables.FatalError));
                    yield return new ExecuteCommandViewLogs(_activator,eds,new LogViewerFilter(LoggingTables.ProgressLog));
                    yield return new ExecuteCommandViewLogs(_activator,eds,new LogViewerFilter(LoggingTables.TableLoadRun));
                }

                yield return new ExecuteCommandQueryPlatformDatabase(_activator, eds) { OverrideCommandName = "Query Database"};
            }

            if(Is(o, out QueryCacheUsedByCohortIdentificationNode cicQueryCache))
            {
                yield return new ExecuteCommandClearQueryCache(_activator, cicQueryCache.User);
            }

            if(Is(o,out AllFreeCohortIdentificationConfigurationsNode _) || Is(o,out AllProjectCohortIdentificationConfigurationsNode _))
                yield return new ExecuteCommandCreateNewCohortIdentificationConfiguration(_activator);


            CohortIdentificationConfiguration cic = null;
            if (Is(o, out ProjectCohortIdentificationConfigurationAssociation pcic) || Is(o,out cic))
            {
                if (pcic != null)
                {
                    cic = pcic.CohortIdentificationConfiguration;
                }

                yield return new ExecuteCommandViewCohortIdentificationConfiguration(_activator, cic, true);
                yield return new ExecuteCommandViewCohortIdentificationConfiguration(_activator, cic, false);

                var commit = new ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(_activator, null).SetTarget(cic);
                if (pcic != null)
                {
                    commit.SetTarget((DatabaseEntity)pcic.Project);
                }

                yield return commit;

                //associate with project
                yield return new ExecuteCommandAssociateCohortIdentificationConfigurationWithProject(_activator).SetTarget(cic);
                
                var clone = new ExecuteCommandCloneCohortIdentificationConfiguration(_activator).SetTarget(cic);
                if(pcic != null)
                {
                    clone.SetTarget((DatabaseEntity) pcic.Project);
                }

                yield return clone;

                yield return new ExecuteCommandFreezeCohortIdentificationConfiguration(_activator, cic, !cic.Frozen);

                yield return new ExecuteCommandCreateNewCohortIdentificationConfiguration(_activator);

                yield return new ExecuteCommandSetQueryCachingDatabase(_activator, cic);
                yield return new ExecuteCommandCreateNewExternalDatabaseServer(_activator, new QueryCachingPatcher(), PermissableDefaults.WebServiceQueryCachingServer_ID);
            }

            if(Is(o,out AllGovernanceNode _))
                yield return new ExecuteCommandCreateNewGovernancePeriod(_activator);

            if(Is(o,out AllLoadMetadatasNode _))
            {
                yield return new ExecuteCommandCreateNewLoadMetadata(_activator);
                yield return new ExecuteCommandImportShareDefinitionList(_activator){OverrideCommandName = "Import Load"};
            }

            if(Is(o,out LoadMetadata lmd))
            {


                yield return new ExecuteCommandExportObjectsToFile(_activator, new IMapsDirectlyToDatabaseTable[] { lmd });

                yield return new ExecuteCommandOverrideRawServer(_activator, lmd);
                yield return new ExecuteCommandCreateNewLoadMetadata(_activator);


                yield return new ExecuteCommandSetGlobalDleIgnorePattern(_activator) { SuggestedCategory = Advanced };
                yield return new ExecuteCommandSetIgnoredColumns(_activator, lmd) { SuggestedCategory = Advanced };
                yield return new ExecuteCommandSetIgnoredColumns(_activator, lmd, null) { OverrideCommandName = "Clear Ignored Columns", SuggestedCategory = Advanced };

                yield return new ExecuteCommandSet(_activator, lmd, typeof(LoadMetadata).GetProperty(nameof(LoadMetadata.IgnoreTrigger)))
                {
                    OverrideCommandName = $"Ignore Trigger (Current value:{lmd.IgnoreTrigger})",
                    SuggestedCategory = Advanced
                };
            }


            if (Is(o, out LoadMetadataScheduleNode scheduleNode))
            {
                yield return new ExecuteCommandCreateNewLoadProgress(_activator, scheduleNode.LoadMetadata);
            }

            if(Is(o, out LoadProgress loadProgress))
            {
                yield return new ExecuteCommandCreateNewCacheProgress(_activator, loadProgress);
            }

            if (Is(o,out LoadStageNode lsn))
            {
                yield return new ExecuteCommandCreateNewClassBasedProcessTask(_activator,lsn.LoadMetadata,lsn.LoadStage,null);
                yield return new ExecuteCommandCreateNewFileBasedProcessTask(_activator,ProcessTaskType.SQLFile,lsn.LoadMetadata,lsn.LoadStage);
                yield return new ExecuteCommandCreateNewFileBasedProcessTask(_activator,ProcessTaskType.Executable,lsn.LoadMetadata,lsn.LoadStage);
            }

            if(Is(o, out LoadDirectoryNode ldn))
            {
                yield return new ExecuteCommandSet(_activator,ldn.LoadMetadata,typeof(LoadMetadata).GetProperty(nameof(LoadMetadata.LocationOfFlatFiles)));
                yield return new ExecuteCommandCreateNewDataLoadDirectory(_activator, ldn.LoadMetadata, null);
            }

            if(Is(o,out AllObjectImportsNode _))
                yield return new ExecuteCommandImportShareDefinitionList(_activator);

            if(Is(o,out AllPermissionWindowsNode _))
                yield return new ExecuteCommandCreateNewPermissionWindow(_activator);

            if(Is(o,out AllPluginsNode _))
            {
                yield return new ExecuteCommandAddPlugins(_activator);
                yield return new ExecuteCommandPrunePlugin(_activator);
                yield return new ExecuteCommandExportPlugins(_activator);
            }

            if(Is(o,out AllRDMPRemotesNode _))
                yield return new ExecuteCommandCreateNewRemoteRDMP(_activator);

            if(Is(o,out AllServersNode _))
            {
                yield return new ExecuteCommandImportTableInfo(_activator,null,false);
                yield return new ExecuteCommandBulkImportTableInfos(_activator);
            }

            if(Is(o, out IFilter filter))
            {
                yield return new ExecuteCommandViewFilterMatchData(_activator, filter, ViewType.TOP_100);
                yield return new ExecuteCommandViewFilterMatchData(_activator, filter, ViewType.Aggregate);
            }


            if(Is(o,out TableInfo ti))
            {
                yield return new ExecuteCommandViewData(_activator, ti);
                
                yield return new ExecuteCommandImportTableInfo(_activator, null, false) { SuggestedCategory = New };
                yield return new ExecuteCommandCreateNewCatalogueFromTableInfo(_activator, ti) { SuggestedCategory = New };
                                    
                yield return new ExecuteCommandUseCredentialsToAccessTableInfoData(_activator,null,ti);
            
                yield return new ExecuteCommandScriptTable(_activator, ti);

                IAtomicCommand[] alterCommands = null;
                try
                {
                    alterCommands = new IAtomicCommand[]
                    {
                        new ExecuteCommandAlterTableName(_activator,ti){SuggestedCategory = Alter },
                        new ExecuteCommandAlterTableCreatePrimaryKey(_activator,ti){SuggestedCategory = Alter },
                        new ExecuteCommandAlterTableAddArchiveTrigger(_activator,ti){SuggestedCategory = Alter },
                        new ExecuteCommandAlterTableMakeDistinct(_activator,ti){SuggestedCategory = Alter }
                    };
                }
                catch(Exception ex)
                {
                    _activator.GlobalErrorCheckNotifier.OnCheckPerformed(new CheckEventArgs("Failed to build Alter commands",CheckResult.Fail,ex));
                }
                
                if(alterCommands  != null)
                    foreach (var item in alterCommands )
                        yield return item;
            
                yield return new ExecuteCommandSyncTableInfo(_activator,ti,false,false);
                yield return new ExecuteCommandSyncTableInfo(_activator,ti,true,false);
                yield return new ExecuteCommandNewObject(_activator,()=>new ColumnInfo(_activator.RepositoryLocator.CatalogueRepository, Guid.NewGuid().ToString(), "fish", ti)){OverrideCommandName = "Add New ColumnInfo" };
            }
                
            if(Is(o,out ColumnInfo colInfo))
            {
                yield return new ExecuteCommandViewData(_activator, ViewType.TOP_100, colInfo) { SuggestedCategory = "View Data" };
                yield return new ExecuteCommandViewData(_activator, ViewType.Aggregate, colInfo) { SuggestedCategory = "View Data" };
                yield return new ExecuteCommandViewData(_activator, ViewType.Distribution, colInfo) { SuggestedCategory = "View Data" };

                yield return new ExecuteCommandAlterColumnType(_activator, colInfo) { SuggestedCategory = Alter };

                yield return new ExecuteCommandSet(_activator, colInfo, typeof(ColumnInfo).GetProperty(nameof(ColumnInfo.IgnoreInLoads))) { OverrideCommandName = $"Ignore In Loads ({colInfo.IgnoreInLoads})" };
            }

            if(Is(o, out AllStandardRegexesNode _))
                yield return new ExecuteCommandCreateNewStandardRegex(_activator);

            if(Is(o, out ArbitraryFolderNode f))
                if(f.CommandGetter != null)
                    foreach(IAtomicCommand cmd in f.CommandGetter())
                        yield return cmd;

            if(Is(o, out CacheProgress cp))
                yield return new ExecuteCommandSetPermissionWindow(_activator,cp);

            if(Is(o, out SelectedDataSets sds))
            {
                yield return new ExecuteCommandAddNewFilterContainer(_activator,sds);
                yield return new ExecuteCommandImportFilterContainerTree(_activator,sds);
                yield return new ExecuteCommandCreateNewFilter(_activator,sds);
                yield return new ExecuteCommandCreateNewFilterFromCatalogue(_activator,sds);
                yield return new ExecuteCommandViewExtractionSql(_activator,sds);
                yield return new ExecuteCommandSetExtractionIdentifier(_activator, sds.GetCatalogue(), sds.ExtractionConfiguration,null);
                yield return new ExecuteCommandAddExtractionProgress(_activator,sds);
                yield return new ExecuteCommandResetExtractionProgress(_activator, sds);
            }

            if(Is(o,out ExtractionProgress progress))
            {
                yield return new ExecuteCommandResetExtractionProgress(_activator, progress);
            }
            
            if(Is(o, out ExtractionConfiguration ec))
            {

                ///////////////////Change Cohorts//////////////

                yield return new ExecuteCommandChooseCohort(_activator, ec);

                yield return new ExecuteCommandAddParameter(_activator, ec, null,null,null);

                /////////////////Add Datasets/////////////
                yield return new ExecuteCommandAddDatasetsToConfiguration(_activator, ec);

                yield return new ExecuteCommandAddPackageToConfiguration(_activator, ec);

                yield return new ExecuteCommandGenerateReleaseDocument(_activator, ec);

                if (ec.IsReleased)
                    yield return new ExecuteCommandUnfreezeExtractionConfiguration(_activator, ec);
                else
                    yield return new ExecuteCommandFreezeExtractionConfiguration(_activator, ec);

                yield return new ExecuteCommandCloneExtractionConfiguration(_activator, ec);

                yield return new ExecuteCommandResetExtractionProgress(_activator, ec, null);
            }

            if(Is(o, out ProjectCataloguesNode pcn))
            {
                yield return new ExecuteCommandMakeCatalogueProjectSpecific(_activator).SetTarget(pcn.Project);
                yield return new ExecuteCommandCreateNewCatalogueByImportingFile(_activator).SetTarget(pcn.Project);
                yield return new ExecuteCommandCreateNewCatalogueByImportingExistingDataTable(_activator).SetTarget(pcn.Project);
            }

            if(Is(o, out ProjectCohortIdentificationConfigurationAssociationsNode pccan))
            {
                yield return new ExecuteCommandAssociateCohortIdentificationConfigurationWithProject(_activator).SetTarget(pccan.Project);
                yield return new ExecuteCommandCreateNewCohortIdentificationConfiguration(_activator).SetTarget(pccan.Project);
            }

            if(Is(o, out ProjectSavedCohortsNode savedCohortsNode ))
            {
                yield return new ExecuteCommandCreateNewCohortFromFile(_activator,null).SetTarget(savedCohortsNode.Project);
                yield return new ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(_activator,null).SetTarget(savedCohortsNode.Project);
                yield return new ExecuteCommandCreateNewCohortFromCatalogue(_activator).SetTarget(savedCohortsNode.Project);
                yield return new ExecuteCommandCreateNewCohortFromTable(_activator,null).SetTarget(savedCohortsNode.Project);
                yield return new ExecuteCommandImportAlreadyExistingCohort(_activator,null,savedCohortsNode.Project);
            }

            if(Is(o,out ExternalCohortTable ect))
            {
                yield return new ExecuteCommandCreateNewCohortFromFile(_activator, ect);
                yield return new ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(_activator, ect);
                yield return new ExecuteCommandCreateNewCohortFromCatalogue(_activator, ect);
                yield return new ExecuteCommandCreateNewCohortFromTable(_activator,ect);
                yield return new ExecuteCommandImportAlreadyExistingCohort(_activator, ect, null);
            }

            if(Is(o,out ExtractableCohort cohort))
            {
                yield return new ExecuteCommandViewCohortSample(_activator, cohort, 100);
                yield return new ExecuteCommandViewCohortSample(_activator, cohort, int.MaxValue,null,false) 
                {
                    AskForFile = true,
                    OverrideCommandName = "Save Cohort To File...",
                    OverrideIcon = FamFamFamIcons.disk
                };
                yield return new ExecuteCommandDeprecate(_activator, cohort, !cohort.IsDeprecated);
            }

            if (Is(o, out CohortAggregateContainer cohortAggregateContainer))
            {
                yield return new ExecuteCommandSetContainerOperation(_activator, cohortAggregateContainer, SetOperation.EXCEPT) { SuggestedCategory = SetContainerOperation };
                yield return new ExecuteCommandSetContainerOperation(_activator, cohortAggregateContainer, SetOperation.UNION) { SuggestedCategory = SetContainerOperation };
                yield return new ExecuteCommandSetContainerOperation(_activator, cohortAggregateContainer, SetOperation.INTERSECT) { SuggestedCategory = SetContainerOperation };

                yield return new ExecuteCommandAddCohortSubContainer(_activator, cohortAggregateContainer) { SuggestedCategory = Add };

                yield return new ExecuteCommandAddCatalogueToCohortIdentificationSetContainer(_activator, cohortAggregateContainer) { SuggestedCategory = Add };
                yield return new ExecuteCommandAddAggregateConfigurationToCohortIdentificationSetContainer(_activator, cohortAggregateContainer, true) { SuggestedCategory = Add };
                yield return new ExecuteCommandAddAggregateConfigurationToCohortIdentificationSetContainer(_activator, cohortAggregateContainer, false) { SuggestedCategory = Add };

                yield return new ExecuteCommandImportCohortIdentificationConfiguration(_activator, null, cohortAggregateContainer) { SuggestedCategory = Add };
                yield return new ExecuteCommandUnMergeCohortIdentificationConfiguration(_activator, cohortAggregateContainer);

            }


            if (Is(o, out IArgument a))
            {
                yield return new ExecuteCommandSetArgument(_activator,a);

            }

            if(Is(o,out IDisableable disable))
                yield return new ExecuteCommandDisableOrEnable(_activator, disable);

            // If the root object is deletable offer deleting
            if(Is(o,out IDeleteable deletable))
                yield return new ExecuteCommandDelete(_activator,deletable){SuggestedShortcut="Delete" };
                      
            if(Is(o, out ReferenceOtherObjectDatabaseEntity reference))
                yield return new ExecuteCommandShowRelatedObject(_activator,reference);

            if(Is(o, out INamed n))
                yield return new ExecuteCommandRename(_activator, n){SuggestedShortcut = "F2" };

            if(Is(o, out PipelineCompatibleWithUseCaseNode pcu))
            {
                yield return new ExecuteCommandNewObject(_activator,typeof(Pipeline)) { OverrideCommandName = "New Pipeline" };
                yield return new ExecuteCommandClonePipeline(_activator, pcu.Pipeline);
                yield return new ExecuteCommandAddPipelineComponent(_activator, pcu.Pipeline, pcu.UseCase);
            }
            else
            if (Is(o, out Pipeline pipeline))
            {
                yield return new ExecuteCommandNewObject(_activator, typeof(Pipeline)) { OverrideCommandName = "New Pipeline" };
                yield return new ExecuteCommandClonePipeline(_activator, pipeline);
                yield return new ExecuteCommandAddPipelineComponent(_activator, pipeline, null);
            }

            if (Is(o, out StandardPipelineUseCaseNode psu))
            {
                yield return new ExecuteCommandNewObject(_activator, typeof(Pipeline)) { OverrideCommandName = "New Pipeline" };
            }
        }

        public IEnumerable<IAtomicCommand> CreateManyObjectCommands(ICollection many)
        {
            if (many.Cast<object>().All(d => d is IDisableable))
            {
                yield return new ExecuteCommandDisableOrEnable(_activator, many.Cast<IDisableable>().ToArray());
            }

            if(many.Cast<object>().All(t=>t is TableInfo))
            {
                yield return new ExecuteCommandScriptTables(_activator, many.Cast<TableInfo>().ToArray(), null, null, null);
            }

            if (many.Cast<object>().All(d => d is IDeleteable))
            {
                yield return new ExecuteCommandDelete(_activator, many.Cast<IDeleteable>().ToArray()){ SuggestedShortcut = "Delete" };
            }

            if (many.Cast<object>().All(d => d is ExtractionFilterParameterSet))
            {
                yield return new ExecuteCommandAddMissingParameters(_activator, many.Cast<ExtractionFilterParameterSet>().ToArray());
            }
        }
    }
}
