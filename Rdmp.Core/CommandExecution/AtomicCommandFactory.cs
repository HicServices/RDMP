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
using Rdmp.Core.Curation.Data.Referencing;
using Rdmp.Core.Databases;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataViewing;
using Rdmp.Core.Logging;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.Providers.Nodes.CohortNodes;
using Rdmp.Core.Providers.Nodes.LoadMetadataNodes;
using Rdmp.Core.Providers.Nodes.ProjectCohortNodes;
using Rdmp.Core.Providers.Nodes.SharingNodes;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using System;
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
            return GetCommandsWithPresentation(o).Select(p=>p.Command);
        }

        public IEnumerable<CommandPresentation> GetCommandsWithPresentation(object o)
        {
            foreach(var cmd in _goto.GetCommands(o))
            {
                yield return new CommandPresentation(cmd,GoTo);
            }

            if(_activator.CanActivate(o))
                yield return new CommandPresentation(new ExecuteCommandActivate(_activator,o));

            if(Is(o, out ILoggedActivityRootObject root)){
                yield return new CommandPresentation(new ExecuteCommandViewLogs(_activator,root));
            }

            if(Is(o,out Catalogue c))
            {
                yield return new CommandPresentation(new ExecuteCommandViewCatalogueData(_activator,c,-1));

                yield return new CommandPresentation(new ExecuteCommandAddNewSupportingSqlTable(_activator, c),Add);
                yield return new CommandPresentation(new ExecuteCommandAddNewSupportingDocument(_activator, c),Add);
                yield return new CommandPresentation(new ExecuteCommandAddNewAggregateGraph(_activator, c),Add);
                yield return new CommandPresentation(new ExecuteCommandAddNewCatalogueItem(_activator, c),Add);
                                        
                yield return new CommandPresentation(new ExecuteCommandChangeExtractability(_activator, c),Extraction);
                yield return new CommandPresentation(new ExecuteCommandMakeCatalogueProjectSpecific(_activator,c,null),Extraction);
                yield return new CommandPresentation(new ExecuteCommandMakeProjectSpecificCatalogueNormalAgain(_activator, c),Extraction);
                yield return new CommandPresentation(new ExecuteCommandSetExtractionIdentifier(_activator,c,null,null),Extraction);
                                        
                yield return new CommandPresentation(new ExecuteCommandExportObjectsToFile(_activator, new[] {c}),Metadata);
                yield return new CommandPresentation(new ExecuteCommandExtractMetadata(_activator, new []{ c},null,null,null,false,null),Metadata);
            }

            if(Is(o,out CatalogueFolder cf))
            {
                yield return new CommandPresentation(new ExecuteCommandCreateNewCatalogueByImportingExistingDataTable(_activator)
                {
                    TargetFolder = cf
                });
                yield return new CommandPresentation(new ExecuteCommandCreateNewEmptyCatalogue(_activator){
                    TargetFolder = cf
                });
            }

            if(Is(o,out  CatalogueItem ci))
            {
                yield return new CommandPresentation(new ExecuteCommandLinkCatalogueItemToColumnInfo(_activator, ci));
                yield return new CommandPresentation(new ExecuteCommandMakeCatalogueItemExtractable(_activator, ci));
                yield return new CommandPresentation(new ExecuteCommandChangeExtractionCategory(_activator, ci.ExtractionInformation));
                yield return new CommandPresentation(new ExecuteCommandImportCatalogueItemDescription(_activator, ci)){SuggestedShortcut= "I",Ctrl=true };
            }

            if(Is(o, out SupportingSQLTable sqlTable))
            {
                yield return new CommandPresentation(new ExecuteCommandRunSupportingSql(_activator,sqlTable));
            }

            if(Is(o,out  AggregateConfiguration ac))
            {
                yield return new CommandPresentation(new ExecuteCommandViewSample(_activator, ac));
                yield return new CommandPresentation(new ExecuteCommandAddNewFilterContainer(_activator,ac));
                yield return new CommandPresentation(new ExecuteCommandImportFilterContainerTree(_activator,ac));
                yield return new CommandPresentation(new ExecuteCommandCreateNewFilter(_activator,ac));


                // graph options
                yield return new CommandPresentation(new ExecuteCommandAddDimension(_activator, ac), Dimensions);
                yield return new CommandPresentation(new ExecuteCommandSetPivot(_activator, ac), Dimensions);
                yield return new CommandPresentation(new ExecuteCommandSetPivot(_activator, ac, null) { OverrideCommandName = "Clear Pivot" }, Dimensions);
                yield return new CommandPresentation(new ExecuteCommandSetAxis(_activator, ac), Dimensions);
                yield return new CommandPresentation(new ExecuteCommandSetAxis(_activator, ac, null) { OverrideCommandName = "Clear Axis" }, Dimensions);


                yield return new CommandPresentation(new ExecuteCommandCreateNewFilterFromCatalogue(_activator,ac));
                
                yield return new CommandPresentation(new ExecuteCommandSetFilterTreeShortcut(_activator,ac));
                yield return new CommandPresentation(new ExecuteCommandSetFilterTreeShortcut(_activator,ac,null){OverrideCommandName="Clear Filter Tree Shortcut" });

                //only allow them to execute graph if it is normal aggregate graph
                if (!ac.IsCohortIdentificationAggregate)
                    yield return new CommandPresentation(new ExecuteCommandExecuteAggregateGraph(_activator, ac));

                yield return new CommandPresentation(new ExecuteCommandCreateNewCatalogueByExecutingAnAggregateConfiguration(_activator,ac));
            }
            
            if(Is(o,out  IContainer container))
            {
                string targetOperation = container.Operation == FilterContainerOperation.AND ? "OR" : "AND";

                yield return new CommandPresentation(new ExecuteCommandSet(_activator,container,nameof(IContainer.Operation),targetOperation){OverrideCommandName = $"Set Operation to {targetOperation}" });
                
                yield return new CommandPresentation(new ExecuteCommandCreateNewFilter(_activator,container.GetFilterFactory(),container));
                yield return new CommandPresentation(new ExecuteCommandCreateNewFilterFromCatalogue(_activator, container));
                yield return new CommandPresentation(new ExecuteCommandAddNewFilterContainer(_activator,container){OverrideCommandName = "Add SubContainer" });
               
                yield return new CommandPresentation(new ExecuteCommandViewFilterMatchData(_activator, container, ViewType.TOP_100));
                yield return new CommandPresentation(new ExecuteCommandViewFilterMatchData(_activator, container, ViewType.Aggregate));
            }
            
            if(Is(o,out AggregatesNode an))
                yield return new CommandPresentation(new ExecuteCommandAddNewAggregateGraph(_activator, an.Catalogue));

            if(Is(o,out AllANOTablesNode _))
            {
                yield return new CommandPresentation(new ExecuteCommandCreateNewANOTable(_activator));
            
                yield return new CommandPresentation(new ExecuteCommandCreateNewExternalDatabaseServer(_activator,
                new ANOStorePatcher(), PermissableDefaults.ANOStore) 
                { OverrideCommandName = "Create ANOStore Database" });

                yield return new CommandPresentation(new ExecuteCommandExportObjectsToFile(_activator,_activator.CoreChildProvider.AllANOTables));
            }

            if(Is(o,out AllCataloguesUsedByLoadMetadataNode aculmd))
            {
                yield return new CommandPresentation(new ExecuteCommandAssociateCatalogueWithLoadMetadata(_activator, aculmd.LoadMetadata));
            }

            if(Is(o,out AllDataAccessCredentialsNode _))
            {
                yield return new CommandPresentation(new ExecuteCommandNewObject(_activator,
                    ()=>new DataAccessCredentials(_activator.RepositoryLocator.CatalogueRepository, "New Blank Credentials " + Guid.NewGuid()))
                    {
                        OverrideCommandName= "Add New Credentials"
                    });
            }

            if(Is(o,out DataAccessCredentialUsageNode usage))
            {
                var existingUsages = _activator.RepositoryLocator.CatalogueRepository.TableInfoCredentialsManager.GetCredentialsIfExistsFor(usage.TableInfo);

                foreach (DataAccessContext context in Enum.GetValues(typeof(DataAccessContext)))
                    yield return new CommandPresentation(new ExecuteCommandSetDataAccessContextForCredentials(_activator, usage, context, existingUsages),SetUsageContext);
            }

            if(Is(o,out AllConnectionStringKeywordsNode _))
            {
                yield return new CommandPresentation(new ExecuteCommandNewObject(_activator,
                    ()=>new ConnectionStringKeyword(_activator.RepositoryLocator.CatalogueRepository,DatabaseType.MicrosoftSQLServer,"NewKeyword", "v"))
                    {
                        OverrideCommandName= "Add New Connection String Keyword"
                    });
            }

            if(Is(o,out AllExternalServersNode _))
            {
                yield return new CommandPresentation(new ExecuteCommandCreateNewExternalDatabaseServer(_activator, null,PermissableDefaults.None));
                
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
                    yield return new CommandPresentation(new ExecuteCommandCreateNewExternalDatabaseServer(_activator, kvp.Value, kvp.Key));
            }

            if(Is(o, out ExternalDatabaseServer eds))
            {
                if(eds.WasCreatedBy(new LoggingDatabasePatcher()))
                {
                    yield return new CommandPresentation(new ExecuteCommandViewLogs(_activator,eds,new LogViewerFilter(LoggingTables.DataLoadRun)));
                    yield return new CommandPresentation(new ExecuteCommandViewLogs(_activator,eds,new LogViewerFilter(LoggingTables.FatalError)));
                    yield return new CommandPresentation(new ExecuteCommandViewLogs(_activator,eds,new LogViewerFilter(LoggingTables.ProgressLog)));
                    yield return new CommandPresentation(new ExecuteCommandViewLogs(_activator,eds,new LogViewerFilter(LoggingTables.TableLoadRun)));
                }
            }

            if(Is(o,out AllFreeCohortIdentificationConfigurationsNode _) || Is(o,out AllProjectCohortIdentificationConfigurationsNode _))
                yield return new CommandPresentation(new ExecuteCommandCreateNewCohortIdentificationConfiguration(_activator));


            CohortIdentificationConfiguration cic = null;
            if (Is(o, out ProjectCohortIdentificationConfigurationAssociation pcic) || Is(o,out cic))
            {
                if (pcic != null)
                {
                    cic = pcic.CohortIdentificationConfiguration;
                }

               // Items.Add("View SQL", _activator.CoreIconProvider.GetImage(RDMPConcept.SQL), (s, e) => _activator.Activate<ViewCohortIdentificationConfigurationUI, CohortIdentificationConfiguration>(cic));

                var commit = new ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(_activator, null).SetTarget(cic);
                if (pcic != null)
                {
                    commit.SetTarget((DatabaseEntity)pcic.Project);
                }

                yield return new CommandPresentation(commit);

                //associate with project
                yield return new CommandPresentation(new ExecuteCommandAssociateCohortIdentificationConfigurationWithProject(_activator).SetTarget(cic));
                
                var clone = new ExecuteCommandCloneCohortIdentificationConfiguration(_activator).SetTarget(cic);
                if(pcic != null)
                {
                    clone.SetTarget((DatabaseEntity) pcic.Project);
                }

                yield return new CommandPresentation(clone);

                yield return new CommandPresentation(new ExecuteCommandFreezeCohortIdentificationConfiguration(_activator, cic, !cic.Frozen));

                yield return new CommandPresentation(new ExecuteCommandCreateNewCohortIdentificationConfiguration(_activator));

                yield return new CommandPresentation(new ExecuteCommandSetQueryCachingDatabase(_activator, cic));
            }

            if(Is(o,out AllGovernanceNode _))
                yield return new CommandPresentation(new ExecuteCommandCreateNewGovernancePeriod(_activator));

            if(Is(o,out AllLoadMetadatasNode _))
            {
                yield return new CommandPresentation(new ExecuteCommandCreateNewLoadMetadata(_activator));
                yield return new CommandPresentation(new ExecuteCommandImportShareDefinitionList(_activator){OverrideCommandName = "Import Load"});
            }


            if (Is(o, out LoadMetadataScheduleNode scheduleNode))
            {
                yield return new CommandPresentation(new ExecuteCommandCreateNewLoadProgress(_activator, scheduleNode.LoadMetadata));
            }

            if(Is(o, out LoadProgress loadProgress))
            {
                yield return new CommandPresentation(new ExecuteCommandCreateNewCacheProgress(_activator, loadProgress));
            }

            if (Is(o,out LoadStageNode lsn))
            {
                yield return new CommandPresentation(new ExecuteCommandCreateNewClassBasedProcessTask(_activator,lsn.LoadMetadata,lsn.LoadStage,null));
                yield return new CommandPresentation(new ExecuteCommandCreateNewFileBasedProcessTask(_activator,ProcessTaskType.SQLFile,lsn.LoadMetadata,lsn.LoadStage));
                yield return new CommandPresentation(new ExecuteCommandCreateNewFileBasedProcessTask(_activator,ProcessTaskType.Executable,lsn.LoadMetadata,lsn.LoadStage));
            }

            if(Is(o, out LoadDirectoryNode ldn))
            {
                yield return new CommandPresentation(new ExecuteCommandSet(_activator,ldn.LoadMetadata,typeof(LoadMetadata).GetProperty(nameof(LoadMetadata.LocationOfFlatFiles))));
            }

            if(Is(o,out AllObjectImportsNode _))
                yield return new CommandPresentation(new ExecuteCommandImportShareDefinitionList(_activator));

            if(Is(o,out AllPermissionWindowsNode _))
                yield return new CommandPresentation(new ExecuteCommandCreateNewPermissionWindow(_activator));

            if(Is(o,out AllPluginsNode _))
            {
                yield return new CommandPresentation(new ExecuteCommandAddPlugins(_activator));
                yield return new CommandPresentation(new ExecuteCommandPrunePlugin(_activator));
                yield return new CommandPresentation(new ExecuteCommandExportPlugins(_activator));
            }

            if(Is(o,out AllRDMPRemotesNode _))
                yield return new CommandPresentation(new ExecuteCommandCreateNewRemoteRDMP(_activator));

            if(Is(o,out AllServersNode _))
            {
                yield return new CommandPresentation(new ExecuteCommandImportTableInfo(_activator,null,false));
                yield return new CommandPresentation(new ExecuteCommandBulkImportTableInfos(_activator));
            }

            if(Is(o, out IFilter filter))
            {
                yield return new CommandPresentation(new ExecuteCommandViewFilterMatchData(_activator, filter, ViewType.TOP_100));
                yield return new CommandPresentation(new ExecuteCommandViewFilterMatchData(_activator, filter, ViewType.Aggregate));
            }


            if(Is(o,out TableInfo ti))
            {
                yield return new CommandPresentation(new ExecuteCommandViewData(_activator, ti));
                
                yield return new CommandPresentation(new ExecuteCommandImportTableInfo(_activator,null,false),New);
                yield return new CommandPresentation(new ExecuteCommandCreateNewCatalogueFromTableInfo(_activator, ti),New);
                                    
                yield return new CommandPresentation(new ExecuteCommandUseCredentialsToAccessTableInfoData(_activator,null,ti));
            
                yield return new CommandPresentation(new ExecuteCommandScriptTable(_activator, ti));

                CommandPresentation[] alterCommands = null;
                try
                {
                    alterCommands = new[]
                    {
                        new CommandPresentation(new ExecuteCommandAlterTableName(_activator,ti),Alter),
                        new CommandPresentation(new ExecuteCommandAlterTableCreatePrimaryKey(_activator,ti), Alter),
                        new CommandPresentation(new ExecuteCommandAlterTableAddArchiveTrigger(_activator,ti), Alter),
                        new CommandPresentation(new ExecuteCommandAlterTableMakeDistinct(_activator,ti), Alter)
                    };
                }
                catch(Exception ex)
                {
                    _activator.GlobalErrorCheckNotifier.OnCheckPerformed(new CheckEventArgs("Failed to build Alter commands",CheckResult.Fail,ex));
                }
                
                if(alterCommands  != null)
                    foreach (var item in alterCommands )
                        yield return item;
            
                yield return new CommandPresentation(new ExecuteCommandSyncTableInfo(_activator,ti,false,false));
                yield return new CommandPresentation(new ExecuteCommandSyncTableInfo(_activator,ti,true,false));
                yield return new CommandPresentation(new ExecuteCommandNewObject(_activator,()=>new ColumnInfo(_activator.RepositoryLocator.CatalogueRepository, Guid.NewGuid().ToString(), "fish", ti)){OverrideCommandName = "Add New ColumnInfo" });
            }
                
            if(Is(o,out ColumnInfo colInfo))
            {
                yield return new CommandPresentation(new ExecuteCommandViewData(_activator, ViewType.TOP_100, colInfo),"View Data");
                yield return new CommandPresentation(new ExecuteCommandViewData(_activator, ViewType.Aggregate, colInfo),"View Data");
                yield return new CommandPresentation(new ExecuteCommandViewData(_activator, ViewType.Distribution, colInfo),"View Data");

                yield return new CommandPresentation(new ExecuteCommandAlterColumnType(_activator, colInfo),Alter);

                yield return new CommandPresentation(new ExecuteCommandSet(_activator, colInfo, typeof(ColumnInfo).GetProperty(nameof(ColumnInfo.IgnoreInLoads))) { OverrideCommandName = $"Ignore In Loads ({colInfo.IgnoreInLoads})" });
            }

            if(Is(o, out AllStandardRegexesNode _))
                yield return new CommandPresentation(new ExecuteCommandCreateNewStandardRegex(_activator));

            if(Is(o, out ArbitraryFolderNode f))
                if(f.CommandGetter != null)
                    foreach(IAtomicCommand cmd in f.CommandGetter())
                        yield return new CommandPresentation(cmd);

            if(Is(o, out CacheProgress cp))
                yield return new CommandPresentation(new ExecuteCommandSetPermissionWindow(_activator,cp));

            if(Is(o, out SelectedDataSets sds))
            {
                yield return new CommandPresentation(new ExecuteCommandAddNewFilterContainer(_activator,sds));
                yield return new CommandPresentation(new ExecuteCommandImportFilterContainerTree(_activator,sds));
                yield return new CommandPresentation(new ExecuteCommandCreateNewFilter(_activator,sds));
                yield return new CommandPresentation(new ExecuteCommandCreateNewFilterFromCatalogue(_activator,sds));
                yield return new CommandPresentation(new ExecuteCommandViewExtractionSql(_activator,sds));
                yield return new CommandPresentation(new ExecuteCommandSetExtractionIdentifier(_activator, sds.GetCatalogue(), sds.ExtractionConfiguration,null));
            }
            
            if(Is(o, out ProjectCataloguesNode pcn))
            {
                yield return new CommandPresentation(new ExecuteCommandMakeCatalogueProjectSpecific(_activator).SetTarget(pcn.Project));
                yield return new CommandPresentation(new ExecuteCommandCreateNewCatalogueByImportingFile(_activator).SetTarget(pcn.Project));
                yield return new CommandPresentation(new ExecuteCommandCreateNewCatalogueByImportingExistingDataTable(_activator).SetTarget(pcn.Project));
            }

            if(Is(o, out ProjectCohortIdentificationConfigurationAssociationsNode pccan))
            {
                yield return new CommandPresentation(new ExecuteCommandAssociateCohortIdentificationConfigurationWithProject(_activator).SetTarget(pccan.Project));
                yield return new CommandPresentation(new ExecuteCommandCreateNewCohortIdentificationConfiguration(_activator).SetTarget(pccan.Project));
            }

            if(Is(o, out ProjectSavedCohortsNode savedCohortsNode ))
            {
                yield return new CommandPresentation(new ExecuteCommandCreateNewCohortFromFile(_activator,null).SetTarget(savedCohortsNode.Project));
                yield return new CommandPresentation(new ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(_activator,null).SetTarget(savedCohortsNode.Project));
                yield return new CommandPresentation(new ExecuteCommandCreateNewCohortFromCatalogue(_activator).SetTarget(savedCohortsNode.Project));

                yield return new CommandPresentation(new ExecuteCommandImportAlreadyExistingCohort(_activator,null,savedCohortsNode.Project));
            }

            if(Is(o,out ExternalCohortTable ect))
            {
                yield return new CommandPresentation(new ExecuteCommandCreateNewCohortFromFile(_activator, ect));
                yield return new CommandPresentation(new ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(_activator, ect));
                yield return new CommandPresentation(new ExecuteCommandCreateNewCohortFromCatalogue(_activator, ect));
                yield return new CommandPresentation(new ExecuteCommandImportAlreadyExistingCohort(_activator, ect, null));
            }

            if (Is(o, out CohortAggregateContainer cohortAggregateContainer))
            {
                yield return new CommandPresentation(new ExecuteCommandSetContainerOperation(_activator, cohortAggregateContainer, SetOperation.EXCEPT), SetContainerOperation);
                yield return new CommandPresentation(new ExecuteCommandSetContainerOperation(_activator, cohortAggregateContainer, SetOperation.UNION), SetContainerOperation);
                yield return new CommandPresentation(new ExecuteCommandSetContainerOperation(_activator, cohortAggregateContainer, SetOperation.INTERSECT), SetContainerOperation);

                yield return new CommandPresentation(new ExecuteCommandAddCohortSubContainer(_activator, cohortAggregateContainer), Add);

                yield return new CommandPresentation(new ExecuteCommandAddCatalogueToCohortIdentificationSetContainer(_activator, cohortAggregateContainer), Add);
                yield return new CommandPresentation(new ExecuteCommandAddAggregateConfigurationToCohortIdentificationSetContainer(_activator, cohortAggregateContainer, true),Add);
                yield return new CommandPresentation(new ExecuteCommandAddAggregateConfigurationToCohortIdentificationSetContainer(_activator, cohortAggregateContainer, false), Add);

                yield return new CommandPresentation(new ExecuteCommandImportCohortIdentificationConfiguration(_activator, null, cohortAggregateContainer), Add);
                yield return new CommandPresentation(new ExecuteCommandUnMergeCohortIdentificationConfiguration(_activator, cohortAggregateContainer));

            }


            if (Is(o, out IArgument a))
            {
                yield return new CommandPresentation(new ExecuteCommandSetArgument(_activator,a));

            }

            if(Is(o,out IDisableable disable))
                yield return new CommandPresentation(new ExecuteCommandDisableOrEnable(_activator, disable));

            // If the root object is deletable offer deleting
			if(Is(o,out IDeleteable deletable))
				yield return new CommandPresentation(new ExecuteCommandDelete(_activator,deletable)){SuggestedShortcut="Delete" };
                      
            if(Is(o, out ReferenceOtherObjectDatabaseEntity reference))
                yield return new CommandPresentation(new ExecuteCommandShowRelatedObject(_activator,reference));

            if(Is(o, out INamed n))
                yield return new CommandPresentation(new ExecuteCommandRename(_activator, n)){SuggestedShortcut = "F2" };
        }
    }
}
