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
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Databases;
using Rdmp.Core.DataExport.Data;
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
using System.Text;

namespace Rdmp.Core.CommandExecution
{
    /// <summary>
    /// Builds lists of <see cref="IAtomicCommand"/> for any given RDMP object
    /// </summary>
    public class AtomicCommandFactory
    {
        IBasicActivateItems _activator;

        public const string Add = "Add";
        public const string New = "New";
        public const string Extraction = "Extractability";
        public const string Metadata = "Metadata";
        public const string Alter = "Alter";
        public const string SetUsageContext = "Set Context";

        public AtomicCommandFactory(IBasicActivateItems activator)
        {
            _activator = activator;
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
            if(_activator.CanActivate(o))
                yield return new CommandPresentation(new ExecuteCommandActivate(_activator,o));

            if(o is Catalogue c)
            {
                yield return new CommandPresentation(new ExecuteCommandAddNewSupportingSqlTable(_activator, c),Add);
                yield return new CommandPresentation(new ExecuteCommandAddNewSupportingDocument(_activator, c),Add);
                yield return new CommandPresentation(new ExecuteCommandAddNewAggregateGraph(_activator, c),Add);
                yield return new CommandPresentation(new ExecuteCommandAddNewCatalogueItem(_activator, c),Add);
                                        
                yield return new CommandPresentation(new ExecuteCommandChangeExtractability(_activator, c),Extraction);
                yield return new CommandPresentation(new ExecuteCommandMakeCatalogueProjectSpecific(_activator,c,null),Extraction);
                yield return new CommandPresentation(new ExecuteCommandMakeProjectSpecificCatalogueNormalAgain(_activator, c),Extraction);
                yield return new CommandPresentation(new ExecuteCommandSetExtractionIdentifier(_activator,c),Extraction);
                                        
                yield return new CommandPresentation(new ExecuteCommandExportObjectsToFile(_activator, new[] {c}),Metadata);
                yield return new CommandPresentation(new ExecuteCommandExtractMetadata(_activator, new []{ c},null,null,null,false,null),Metadata);
            }

            if(o is AggregateConfiguration ac)
            {
                yield return new CommandPresentation(new ExecuteCommandViewSample(_activator, ac));
                yield return new CommandPresentation(new ExecuteCommandAddNewFilterContainer(_activator,ac));
                yield return new CommandPresentation(new ExecuteCommandImportFilterContainerTree(_activator,ac));
                yield return new CommandPresentation(new ExecuteCommandCreateNewFilter(_activator,ac));
                yield return new CommandPresentation(new ExecuteCommandCreateNewFilterFromCatalogue(_activator,ac));
                
                yield return new CommandPresentation(new ExecuteCommandSetFilterTreeShortcut(_activator,ac));
                yield return new CommandPresentation(new ExecuteCommandSetFilterTreeShortcut(_activator,ac,null){OverrideCommandName="Clear Filter Tree Shortcut" });
            
                yield return new CommandPresentation(new ExecuteCommandCreateNewCatalogueByExecutingAnAggregateConfiguration(_activator,ac));
            }
            
            if(o is IContainer container)
            {
                
                string targetOperation = container.Operation == FilterContainerOperation.AND ? "OR" : "AND";

                yield return new CommandPresentation(new ExecuteCommandSet(_activator,container,nameof(IContainer.Operation),targetOperation){OverrideCommandName = $"Set Operation to {targetOperation}" });
                
                yield return new CommandPresentation(new ExecuteCommandCreateNewFilter(_activator,container.GetFilterFactory(),container));
                yield return new CommandPresentation(new ExecuteCommandCreateNewFilterFromCatalogue(_activator, container));
                yield return new CommandPresentation(new ExecuteCommandCreateNewFilterFromCatalogue(_activator, container));
                yield return new CommandPresentation(new ExecuteCommandAddNewFilterContainer(_activator,container){OverrideCommandName = "Add SubContainer" });
            }
            
            if(o is AggregatesNode an)
                yield return new CommandPresentation(new ExecuteCommandAddNewAggregateGraph(_activator, an.Catalogue));

            if(o is AllANOTablesNode)
            {
                yield return new CommandPresentation(new ExecuteCommandCreateNewANOTable(_activator));
            
                yield return new CommandPresentation(new ExecuteCommandCreateNewExternalDatabaseServer(_activator,
                new ANOStorePatcher(), PermissableDefaults.ANOStore) 
                { OverrideCommandName = "Create ANOStore Database" });

                yield return new CommandPresentation(new ExecuteCommandExportObjectsToFile(_activator,_activator.CoreChildProvider.AllANOTables));
            }

            if(o is AllCataloguesUsedByLoadMetadataNode aculmd)
            {
                yield return new CommandPresentation(new ExecuteCommandAssociateCatalogueWithLoadMetadata(_activator, aculmd.LoadMetadata));
            }

            if(o is AllDataAccessCredentialsNode)
            {
                yield return new CommandPresentation(new ExecuteCommandNewObject(_activator,
                    ()=>new DataAccessCredentials(_activator.RepositoryLocator.CatalogueRepository, "New Blank Credentials " + Guid.NewGuid()))
                    {
                        OverrideCommandName= "Add New Credentials"
                    });
            }

            if(o is DataAccessCredentialUsageNode usage)
            {
                var existingUsages = _activator.RepositoryLocator.CatalogueRepository.TableInfoCredentialsManager.GetCredentialsIfExistsFor(usage.TableInfo);

                foreach (DataAccessContext context in Enum.GetValues(typeof(DataAccessContext)))
                    yield return new CommandPresentation(new ExecuteCommandSetDataAccessContextForCredentials(_activator, usage, context, existingUsages),SetUsageContext);
            }

            if(o is AllConnectionStringKeywordsNode)
            {
                yield return new CommandPresentation(new ExecuteCommandNewObject(_activator,
                    ()=>new ConnectionStringKeyword(_activator.RepositoryLocator.CatalogueRepository,DatabaseType.MicrosoftSQLServer,"NewKeyword", "v"))
                    {
                        OverrideCommandName= "Add New Connection String Keyword"
                    });
            }

            if(o is AllExternalServersNode)
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

            if(o is AllFreeCohortIdentificationConfigurationsNode || o is AllProjectCohortIdentificationConfigurationsNode)
                yield return new CommandPresentation(new ExecuteCommandCreateNewCohortIdentificationConfiguration(_activator));

            if(o is AllGovernanceNode)
                yield return new CommandPresentation(new ExecuteCommandCreateNewGovernancePeriod(_activator));

            if(o is AllLoadMetadatasNode)
            {
                yield return new CommandPresentation(new ExecuteCommandCreateNewLoadMetadata(_activator));
                yield return new CommandPresentation(new ExecuteCommandImportShareDefinitionList(_activator){OverrideCommandName = "Import Load"});
            }

            if(o is AllObjectImportsNode)
                yield return new CommandPresentation(new ExecuteCommandImportShareDefinitionList(_activator));

            if(o is AllPermissionWindowsNode)
                yield return new CommandPresentation(new ExecuteCommandCreateNewPermissionWindow(_activator));

            if(o is AllPluginsNode)
            {
                yield return new CommandPresentation(new ExecuteCommandAddPlugins(_activator));
                yield return new CommandPresentation(new ExecuteCommandExportPlugins(_activator));
            }

            if(o is AllRDMPRemotesNode)
                yield return new CommandPresentation(new ExecuteCommandCreateNewRemoteRDMP(_activator));

            if(o is AllServersNode)
            {
                yield return new CommandPresentation(new ExecuteCommandImportTableInfo(_activator,null,false));
                yield return new CommandPresentation(new ExecuteCommandBulkImportTableInfos(_activator));
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
                        new CommandPresentation(new ExecuteCommandAlterTableAddArchiveTrigger(_activator,ti), Alter)
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
                yield return new CommandPresentation(new ExecuteCommandNewObject(_activator,()=>new ColumnInfo(_activator.RepositoryLocator.CatalogueRepository, Guid.NewGuid().ToString(), "fish", ti)));
            }
                
            if(o is AllStandardRegexesNode)
                yield return new CommandPresentation(new ExecuteCommandCreateNewStandardRegex(_activator));

            if(o is ArbitraryFolderNode f)
                if(f.CommandGetter != null)
                    foreach(IAtomicCommand cmd in f.CommandGetter())
                        yield return new CommandPresentation(cmd);

            if(o is CacheProgress cp)
                yield return new CommandPresentation(new ExecuteCommandSetPermissionWindow(_activator,cp));

            if(o is SelectedDataSets sds)
            {
                yield return new CommandPresentation(new ExecuteCommandAddNewFilterContainer(_activator,sds));
                yield return new CommandPresentation(new ExecuteCommandImportFilterContainerTree(_activator,sds));
                yield return new CommandPresentation(new ExecuteCommandCreateNewFilter(_activator,sds));
                yield return new CommandPresentation(new ExecuteCommandCreateNewFilterFromCatalogue(_activator,sds));
                yield return new CommandPresentation(new ExecuteCommandViewSelectedDataSetsExtractionSql(_activator,sds));
            }
            
            if(o is ProjectCataloguesNode pcn)
            {
                yield return new CommandPresentation(new ExecuteCommandMakeCatalogueProjectSpecific(_activator).SetTarget(pcn.Project));
                yield return new CommandPresentation(new ExecuteCommandCreateNewCatalogueByImportingFile(_activator).SetTarget(pcn.Project));
                yield return new CommandPresentation(new ExecuteCommandCreateNewCatalogueByImportingExistingDataTable(_activator).SetTarget(pcn.Project));
            }

            if(o is ProjectCohortIdentificationConfigurationAssociationsNode pccan)
            {
                yield return new CommandPresentation(new ExecuteCommandAssociateCohortIdentificationConfigurationWithProject(_activator).SetTarget(pccan.Project));
                yield return new CommandPresentation(new ExecuteCommandCreateNewCohortIdentificationConfiguration(_activator).SetTarget(pccan.Project));
            }

            if(o is ProjectSavedCohortsNode savedCohortsNode )
            {
                yield return new CommandPresentation(new ExecuteCommandCreateNewCohortFromFile(_activator,null).SetTarget(savedCohortsNode.Project));
                yield return new CommandPresentation(new ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(_activator,null).SetTarget(savedCohortsNode.Project));
                yield return new CommandPresentation(new ExecuteCommandCreateNewCohortFromCatalogue(_activator).SetTarget(savedCohortsNode.Project));
            }

            if(o is IArgument a)
            {
                yield return new CommandPresentation(new ExecuteCommandSetArgument(_activator,a));

            }

            if(Is(o,out IDisableable disable))
                yield return new CommandPresentation(new ExecuteCommandDisableOrEnable(_activator, disable));

            // If the root object is deletable offer deleting
			if(Is(o,out IDeleteable deletable))
				yield return new CommandPresentation(new ExecuteCommandDelete(_activator,deletable)){SuggestedShortcut="Delete" };
                      
            if(Is(o, out INamed n))
                yield return new CommandPresentation(new ExecuteCommandRename(_activator, n)){SuggestedShortcut = "F2" };
        }

        /// <summary>
        /// Returns o is <typeparamref name="T"/> but with auto unpacking of <see cref="IMasqueradeAs"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        private bool Is<T>(object o, out T match)
        {
            if(o is T)
            {
                match = (T)o;
                return true;
            }

            if(o is IMasqueradeAs m)
            {
                return Is<T>(m.MasqueradingAs(),out match);
            }

            match = default(T);
            return false;
        }
    }
}
