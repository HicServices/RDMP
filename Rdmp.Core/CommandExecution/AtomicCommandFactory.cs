// Copyright (c) The University of Dundee 2018-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FAnsi;
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
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.Providers.Nodes.CohortNodes;
using Rdmp.Core.Providers.Nodes.LoadMetadataNodes;
using Rdmp.Core.Providers.Nodes.PipelineNodes;
using Rdmp.Core.Providers.Nodes.ProjectCohortNodes;
using Rdmp.Core.Providers.Nodes.SharingNodes;
using Rdmp.Core.Providers.Nodes.UsedByProject;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution;

/// <summary>
/// Builds lists of <see cref="IAtomicCommand"/> for any given RDMP object
/// </summary>
public class AtomicCommandFactory : CommandFactoryBase
{
    private readonly IBasicActivateItems _activator;
    private readonly GoToCommandFactory _goto;
    public const string Add = "Add";
    public const string Batching = "Batching";
    public const string New = "New";
    public const string GoTo = "Go To";
    public const string Extraction = "Extractability";
    public const string Metadata = "Metadata";
    public const string Alter = "Alter";
    public const string SetUsageContext = "Set Context";
    public const string SetContainerOperation = "Set Operation";
    public const string Dimensions = "Dimensions";
    public const string Advanced = "Advanced";
    public const string View = "View";
    public const string Deprecation = "Deprecation";
    public const string ViewParentTree = "View Parent Tree";

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
        foreach (var cmd in _goto.GetCommands(o))
        {
            cmd.SuggestedCategory = GoTo;
            yield return cmd;
        }

        if (_activator.CanActivate(o))
            yield return new ExecuteCommandActivate(_activator, o);

        if (Is(o, out ILoggedActivityRootObject root)) yield return new ExecuteCommandViewLogs(_activator, root);

        if (Is(o, out IArgument a))
            if (!_activator.IsWinForms)
                yield return new ExecuteCommandSetArgument(_activator, a);
        if (Is(o, out Catalogue c))
        {
            var isApiCall = c.IsApiCall();

            if (!isApiCall)
            {
                yield return new ExecuteCommandViewData(_activator, c, ViewType.TOP_100)
                {
                    Weight = -99.2f,
                    OverrideCommandName = "Catalogue SQL/Data",
                    SuggestedCategory = View
                };
                yield return new ExecuteCommandAddNewCatalogueItem(_activator, c)
                { Weight = -99.9f, SuggestedCategory = Add, OverrideCommandName = "New Catalogue Item" };
                yield return new ExecuteCommandAddNewAggregateGraph(_activator, c)
                {
                    Weight = -98.9f,
                    SuggestedCategory = Add,
                    OverrideCommandName = "New Aggregate Graph"
                };
            }

            yield return new ExecuteCommandAddNewSupportingSqlTable(_activator, c)
            {
                Weight = -87.9f,
                SuggestedCategory = Add,
                OverrideCommandName = "New Supporting SQL Table"
            };
            yield return new ExecuteCommandAddNewSupportingDocument(_activator, c)
            {
                Weight = -87.8f,
                SuggestedCategory = Add,
                OverrideCommandName = "New Supporting Document"
            };
            if (!isApiCall)
            {
                yield return new ExecuteCommandChangeExtractability(_activator, c)
                {
                    Weight = -99.0010f,
                    SuggestedCategory = Extraction
                };

                yield return new ExecuteCommandMakeCatalogueProjectSpecific(_activator, c, null, false)
                {
                    Weight = -99.0009f,
                    SuggestedCategory = Extraction
                };
                yield return new ExecuteCommandMakeProjectSpecificCatalogueNormalAgain(_activator, c, null)
                {
                    Weight = -99.0009f,
                    SuggestedCategory = Extraction,
                    OverrideCommandName = "Remove Project Specific Catalogue from a Project"
                };

                yield return new ExecuteCommandSetExtractionIdentifier(_activator, c, null, null)
                {
                    Weight = -99.0008f,
                    SuggestedCategory = Extraction
                };

                yield return new ExecuteCommandSetExtractionPrimaryKeys(_activator, c, null, null)
                {
                    Weight = -99.0007f,
                    SuggestedCategory = Extraction
                };
            }

            yield return new ExecuteCommandExportObjectsToFile(_activator, new[] { c })
            {
                Weight = -95.10f,
                SuggestedCategory = Metadata
            };
            yield return new ExecuteCommandExtractMetadata(_activator, new[] { c }, null, null, null, false, null)
            {
                OverrideCommandName = "Generate Metadata Report (with custom template)",
                Weight = -99.058f,
                SuggestedCategory = Metadata
            };
        }

        if (Is(o, out IHasFolder folderable))
            yield return new ExecuteCommandPutIntoFolder(_activator, new[] { folderable }, null);

        if (Is(o, out FolderNode<Catalogue> cf))
        {
            yield return new ExecuteCommandCreateNewCatalogueByImportingFile(_activator)
            {
                OverrideCommandName = "New Catalogue From File...",
                TargetFolder = cf.FullName,
                SuggestedCategory = Add,
                Weight = -90.9f
            };
            yield return new ExecuteCommandCreateNewCatalogueByImportingExistingDataTable(_activator)
            {
                OverrideCommandName = "New Catalogue From Database...",
                TargetFolder = cf.FullName,
                SuggestedCategory = Add,
                Weight = -90.8f
            };
            yield return new ExecuteCommandCreateNewEmptyCatalogue(_activator)
            {
                OverrideCommandName = "New Empty Catalogue (Advanced)",
                TargetFolder = cf.FullName,
                SuggestedCategory = Add,
                Weight = -90.7f
            };
        }

        if (Is(o, out ExtractionInformation ei))
        {
            yield return new ExecuteCommandCreateNewFilter(_activator, new ExtractionFilterFactory(ei))
            { OverrideCommandName = "Add New Filter" };
            yield return new ExecuteCommandCreateNewCohortFromCatalogue(_activator, ei);
            yield return new ExecuteCommandChangeExtractionCategory(_activator, new[] { ei });

            yield return new ExecuteCommandViewData(_activator, ViewType.TOP_100, ei) { SuggestedCategory = View };
            yield return new ExecuteCommandViewData(_activator, ViewType.Aggregate, ei) { SuggestedCategory = View };
            yield return new ExecuteCommandViewData(_activator, ViewType.Distribution, ei) { SuggestedCategory = View };
        }

        if (Is(o, out ExtractionFilter cataFilter))
            yield return new ExecuteCommandAddNewExtractionFilterParameterSet(_activator, cataFilter);

        if (Is(o, out ExtractionFilterParameterSet efps))
            yield return new ExecuteCommandAddMissingParameters(_activator, efps);

        if (Is(o, out ISqlParameter p) && p is IMapsDirectlyToDatabaseTable m)
        {
            yield return new ExecuteCommandSet(_activator, m, p.GetType().GetProperty(nameof(ISqlParameter.Value)));

            if (p is not ExtractionFilterParameterSetValue)
                yield return new ExecuteCommandSet(_activator, m,
                    p.GetType().GetProperty(nameof(ISqlParameter.ParameterSQL)));
        }

        if (Is(o, out CatalogueItem ci))
        {
            yield return new ExecuteCommandCreateNewFilter(_activator, ci) { OverrideCommandName = "Add New Filter" };
            yield return new ExecuteCommandLinkCatalogueItemToColumnInfo(_activator, ci);
            yield return new ExecuteCommandMakeCatalogueItemExtractable(_activator, ci);
            yield return new ExecuteCommandChangeExtractionCategory(_activator, new[] { ci.ExtractionInformation });
            yield return new ExecuteCommandImportCatalogueItemDescription(_activator, ci)
            { SuggestedShortcut = "I", Ctrl = true };
            var ciExtractionInfo = ci.ExtractionInformation;
            if (ciExtractionInfo != null)
            {
                yield return new ExecuteCommandViewData(_activator, ViewType.TOP_100, ciExtractionInfo)
                { SuggestedCategory = View };
                yield return new ExecuteCommandViewData(_activator, ViewType.Aggregate, ciExtractionInfo)
                { SuggestedCategory = View };
                yield return new ExecuteCommandViewData(_activator, ViewType.Distribution, ciExtractionInfo)
                { SuggestedCategory = View };
            }
        }

        if (Is(o, out SupportingSQLTable sqlTable))
            yield return new ExecuteCommandRunSupportingSql(_activator, sqlTable, null);

        if (Is(o, out AggregateConfiguration ac) && !ac.Catalogue.IsApiCall())
        {
            yield return new ExecuteCommandSet(_activator, ac,
                typeof(AggregateConfiguration).GetProperty(nameof(AggregateConfiguration.Description)));

            yield return new ExecuteCommandCreateNewFilter(_activator, ac)
            { SuggestedCategory = Add, OverrideCommandName = "New Filter" };
            yield return new ExecuteCommandCreateNewFilter(_activator, ac)
            {
                OfferCatalogueFilters = true,
                SuggestedCategory = Add,
                OverrideCommandName = "Existing Filter"
            };

            yield return new ExecuteCommandAddNewFilterContainer(_activator, ac)
            { SuggestedCategory = Add, OverrideCommandName = "New Filter Container" };
            yield return new ExecuteCommandImportFilterContainerTree(_activator, ac)
            { SuggestedCategory = Add, OverrideCommandName = "Existing Filter Container (copy of)" };

            yield return new ExecuteCommandAddParameter(_activator, ac, null, null, null)
            { SuggestedCategory = Add, OverrideCommandName = "New Catalogue Filter Parameter" };

            yield return new ExecuteCommandViewData(_activator, ac) { OverrideCommandName = "View Sample SQL/Data" };

            if (ac.IsCohortIdentificationAggregate)
            {
                yield return new ExecuteCommandSetAggregateDimension(_activator, ac);

                yield return _activator.RepositoryLocator.CatalogueRepository
                    .GetExtendedProperties(ExtendedProperty.IsTemplate, ac)
                    .Any(v => v.Value.Equals("true"))
                    ? new ExecuteCommandSetExtendedProperty(_activator, new[] { ac }, ExtendedProperty.IsTemplate, null)
                    {
                        OverrideCommandName = "Make Non Template"
                    }
                    : (IAtomicCommand)new ExecuteCommandSetExtendedProperty(_activator, new[] { ac },
                        ExtendedProperty.IsTemplate, "true")
                    {
                        OverrideCommandName = "Make Reusable Template"
                    };
            }

            // graph options
            yield return new ExecuteCommandAddDimension(_activator, ac) { SuggestedCategory = Dimensions };
            yield return new ExecuteCommandSetPivot(_activator, ac) { SuggestedCategory = Dimensions };
            yield return new ExecuteCommandSetPivot(_activator, ac, null)
            { OverrideCommandName = "Clear Pivot", SuggestedCategory = Dimensions };
            yield return new ExecuteCommandSetAxis(_activator, ac) { SuggestedCategory = Dimensions };
            yield return new ExecuteCommandSetAxis(_activator, ac, null)
            { OverrideCommandName = "Clear Axis", SuggestedCategory = Dimensions };


            /*if(ac.OverrideFiltersByUsingParentAggregateConfigurationInstead_ID != null)
            {
                yield return new ExecuteCommandSetFilterTreeShortcut(_activator, ac, null) { OverrideCommandName = "Clear Filter Tree Shortcut" };
            }
            else
            {
                yield return new ExecuteCommandSetFilterTreeShortcut(_activator, ac);
            }*/

            //only allow them to execute graph if it is normal aggregate graph
            if (!ac.IsCohortIdentificationAggregate)
                yield return new ExecuteCommandExecuteAggregateGraph(_activator, ac);

            //yield return new ExecuteCommandCreateNewCatalogueByExecutingAnAggregateConfiguration(_activator,ac);
        }

        if (Is(o, out IContainer container))
        {
            var targetOperation = container.Operation == FilterContainerOperation.AND ? "OR" : "AND";
            yield return new ExecuteCommandSet(_activator, container, nameof(IContainer.Operation), targetOperation)
            { OverrideCommandName = $"Set Operation to {targetOperation}" };

            yield return new ExecuteCommandCreateNewFilter(_activator, container.GetFilterFactory(), container)
            { SuggestedCategory = Add, OverrideCommandName = "New Filter" };
            yield return new ExecuteCommandCreateNewFilter(_activator, container, null)
            {
                OfferCatalogueFilters = true,
                SuggestedCategory = Add,
                OverrideCommandName = "Existing Filter"
            };
            yield return new ExecuteCommandAddNewFilterContainer(_activator, container)
            { SuggestedCategory = Add, OverrideCommandName = "Sub Container" };

            yield return new ExecuteCommandViewFilterMatchData(_activator, container, ViewType.TOP_100);
            yield return new ExecuteCommandViewFilterMatchData(_activator, container, ViewType.Aggregate);
        }

        if (Is(o, out AggregatesNode an))
            yield return new ExecuteCommandAddNewAggregateGraph(_activator, an.Catalogue);

        if (Is(o, out AllANOTablesNode _))
        {
            yield return new ExecuteCommandCreateNewANOTable(_activator);

            yield return new ExecuteCommandCreateNewExternalDatabaseServer(_activator,
                    new ANOStorePatcher(), PermissableDefaults.ANOStore)
            { OverrideCommandName = "Create ANOStore Database" };

            yield return new ExecuteCommandExportObjectsToFile(_activator, _activator.CoreChildProvider.AllANOTables);
        }

        if (Is(o, out AllCataloguesUsedByLoadMetadataNode aculmd))
            yield return new ExecuteCommandAssociateCatalogueWithLoadMetadata(_activator, aculmd.LoadMetadata);

        if (Is(o, out AllDataAccessCredentialsNode _))
            yield return new ExecuteCommandNewObject(_activator,
                () => new DataAccessCredentials(_activator.RepositoryLocator.CatalogueRepository,
                    $"New Blank Credentials {Guid.NewGuid()}"))
            {
                OverrideCommandName = "Add New Credentials"
            };

        if (Is(o, out DataAccessCredentialUsageNode usage))
        {
            var existingUsages =
                _activator.RepositoryLocator.CatalogueRepository.TableInfoCredentialsManager.GetCredentialsIfExistsFor(
                    usage.TableInfo);

            foreach (DataAccessContext context in Enum.GetValues(typeof(DataAccessContext)))
                yield return new ExecuteCommandSetDataAccessContextForCredentials(_activator, usage, context,
                    existingUsages)
                {
                    SuggestedCategory = SetUsageContext
                };
        }

        if (Is(o, out AllConnectionStringKeywordsNode _))
            yield return new ExecuteCommandNewObject(_activator,
                () => new ConnectionStringKeyword(_activator.RepositoryLocator.CatalogueRepository,
                    DatabaseType.MicrosoftSQLServer, "NewKeyword", "v"))
            {
                OverrideCommandName = "Add New Connection String Keyword"
            };

        if (Is(o, out AllExternalServersNode _))
        {
            yield return new ExecuteCommandCreateNewExternalDatabaseServer(_activator, null, PermissableDefaults.None);

            var assemblyDictionary = new Dictionary<PermissableDefaults, IPatcher>
            {
                { PermissableDefaults.DQE, new DataQualityEnginePatcher() },
                { PermissableDefaults.WebServiceQueryCachingServer_ID, new QueryCachingPatcher() },
                { PermissableDefaults.LiveLoggingServer_ID, new LoggingDatabasePatcher() },
                { PermissableDefaults.IdentifierDumpServer_ID, new IdentifierDumpDatabasePatcher() },
                { PermissableDefaults.ANOStore, new ANOStorePatcher() },
                { PermissableDefaults.CohortIdentificationQueryCachingServer_ID, new QueryCachingPatcher() }
            };

            foreach (var kvp in assemblyDictionary)
                yield return new ExecuteCommandCreateNewExternalDatabaseServer(_activator, kvp.Value, kvp.Key);
        }

        if (Is(o, out ExternalDatabaseServer eds))
        {
            if (eds.WasCreatedBy(new LoggingDatabasePatcher()))
            {
                yield return new ExecuteCommandViewLogs(_activator, eds,
                    new LogViewerFilter(LoggingTables.DataLoadRun));
                yield return new ExecuteCommandViewLogs(_activator, eds, new LogViewerFilter(LoggingTables.FatalError));
                yield return new ExecuteCommandViewLogs(_activator, eds,
                    new LogViewerFilter(LoggingTables.ProgressLog));
                yield return new ExecuteCommandViewLogs(_activator, eds,
                    new LogViewerFilter(LoggingTables.TableLoadRun));
            }

            yield return new ExecuteCommandQueryPlatformDatabase(_activator, eds) { OverrideCommandName = View };
        }

        if (Is(o, out QueryCacheUsedByCohortIdentificationNode cicQueryCache))
            yield return new ExecuteCommandClearQueryCache(_activator, cicQueryCache.User);

        if (Is(o, out FolderNode<CohortIdentificationConfiguration> cicFolder))
            yield return new ExecuteCommandCreateNewCohortIdentificationConfiguration(_activator)
            {
                PromptToPickAProject = true,
                Folder = cicFolder.FullName
            };

        if (Is(o, out IJoin j))
        {
            yield return new ExecuteCommandSetExtendedProperty(_activator, new[] { (IMapsDirectlyToDatabaseTable)j },
                ExtendedProperty.CustomJoinSql, null)
            {
                OverrideCommandName = "Set CustomJoinSql",
                PromptForValue = true,
                PromptForValueTaskDescription = ExtendedProperty.CustomJoinSqlDescription,
                SuggestedCategory = "Custom Join"
            };

            yield return new ExecuteCommandSetExtendedProperty(_activator, new[] { (IMapsDirectlyToDatabaseTable)j },
                ExtendedProperty.CustomJoinSql, null)
            {
                OverrideCommandName = "Clear CustomJoinSql",
                SuggestedCategory = "Custom Join"
            };
        }

        CohortIdentificationConfiguration cic = null;
        if (Is(o, out ProjectCohortIdentificationConfigurationAssociation pcic) || Is(o, out cic))
        {
            if (pcic != null) cic = pcic.CohortIdentificationConfiguration;

            var commit =
                new ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(_activator, null)
                {
                    OverrideCommandName = "Commit Cohort",
                    Weight = -99.8f
                }.SetTarget(cic);
            if (pcic != null) commit.SetTarget((DatabaseEntity)pcic.Project);

            yield return commit;

            yield return new ExecuteCommandViewData(_activator, cic, ViewType.All, null, true) { Weight = -99.7f };
            yield return new ExecuteCommandViewData(_activator, cic, ViewType.All, null, false) { Weight = -99.6f };

            yield return new ExecuteCommandFreezeCohortIdentificationConfiguration(_activator, cic, !cic.Frozen)
            { Weight = -50.5f };
            yield return new ExecuteCommandCreateHoldoutLookup(_activator, cic)
            { Weight = -50.5f };

            var clone = new ExecuteCommandCloneCohortIdentificationConfiguration(_activator)
            { Weight = -50.4f, OverrideCommandName = "Clone" }.SetTarget(cic);
            if (pcic != null) clone.SetTarget((DatabaseEntity)pcic.Project);
            yield return clone;
            //associate with project
            yield return new ExecuteCommandAssociateCohortIdentificationConfigurationWithProject(_activator)
            { Weight = -50.3f, OverrideCommandName = "Associate with Project" }.SetTarget(cic);

            yield return new ExecuteCommandSetQueryCachingDatabase(_activator, cic)
            { Weight = -50.4f, OverrideCommandName = "Change Query Cache" };
        }

        if (Is(o, out AllGovernanceNode _))
        {
            yield return new ExecuteCommandCreateNewGovernancePeriod(_activator)
            { OverrideCommandName = "Add New Governance Period" };
            yield return new ExecuteCommandAddNewGovernanceDocument(_activator, null)
            { OverrideCommandName = "Add New Governance Document" };
        }

        if (Is(o, out FolderNode<LoadMetadata> lmdFolder))
        {
            yield return new ExecuteCommandCreateNewLoadMetadata(_activator) { Folder = lmdFolder.FullName };
            yield return new ExecuteCommandImportShareDefinitionList(_activator)
            { OverrideCommandName = "Import Load" };
        }

        if (Is(o, out LoadMetadata lmd))
        {
            if (lmd.RootLoadMetadata_ID is null)
            {
                yield return new ExecuteCommandExportObjectsToFile(_activator, new IMapsDirectlyToDatabaseTable[] { lmd });

                yield return new ExecuteCommandOverrideRawServer(_activator, lmd);
                yield return new ExecuteCommandCreateNewLoadMetadata(_activator);
                var reservedTest = lmd.AllowReservedPrefix ? "Drop" : "Allow";
                yield return new ExecuteCommandToggleAllowReservedPrefixForLoadMetadata(lmd)
                {
                    OverrideCommandName = $"{reservedTest} Reserved Prefix Columns"
                };
                yield return new ExecuteCommandCreateLoadMetadataVersion(_activator, lmd)
                {
                    OverrideCommandName = "Save Version"
                };
                yield return new ExecuteCommandCloneLoadMetadata(_activator, lmd)
                {
                    OverrideCommandName = "Clone Load Metadata"
                };

                yield return new ExecuteCommandSetGlobalDleIgnorePattern(_activator) { SuggestedCategory = Advanced };
                yield return new ExecuteCommandSetIgnoredColumns(_activator, lmd) { SuggestedCategory = Advanced };
                yield return new ExecuteCommandSetIgnoredColumns(_activator, lmd, null)
                { OverrideCommandName = "Clear Ignored Columns", SuggestedCategory = Advanced };

                yield return new ExecuteCommandSetExtendedProperty(_activator, new[] { lmd },
                    ExtendedProperty.PersistentRaw, null)
                {
                    OverrideCommandName = "Persistent RAW",
                    PromptForValue = true,
                    PromptForValueTaskDescription = ExtendedProperty.PersistentRawDescription,
                    SuggestedCategory = Advanced
                };

                yield return new ExecuteCommandSet(_activator, lmd,
                    typeof(LoadMetadata).GetProperty(nameof(LoadMetadata.IgnoreTrigger)))
                {
                    OverrideCommandName = $"Ignore Trigger (Current value:{lmd.IgnoreTrigger})",
                    SuggestedCategory = Advanced
                };
            }
            else
            {
                yield return new ExecuteCommandRestoreLoadMetadataVersion(_activator, lmd)
                {
                    OverrideCommandName = "Restore Version"
                };
                yield return new ExecuteCommandCloneLoadMetadata(_activator, lmd)
                {
                    OverrideCommandName = "Clone Load Metadata"
                };
            }
        }

        if (Is(o, out LoadMetadataScheduleNode scheduleNode))
            yield return new ExecuteCommandCreateNewLoadProgress(_activator, scheduleNode.LoadMetadata);

        if (Is(o, out LoadProgress loadProgress))
            yield return new ExecuteCommandCreateNewCacheProgress(_activator, loadProgress);

        if (Is(o, out LoadStageNode lsn))
        {
            yield return new ExecuteCommandCreateNewClassBasedProcessTask(_activator, lsn.LoadMetadata, lsn.LoadStage,
                null);
            yield return new ExecuteCommandCreateNewFileBasedProcessTask(_activator, ProcessTaskType.SQLFile,
                lsn.LoadMetadata, lsn.LoadStage);
            yield return new ExecuteCommandCreateNewFileBasedProcessTask(_activator, ProcessTaskType.Executable,
                lsn.LoadMetadata, lsn.LoadStage);
            yield return new ExecuteCommandCreateNewFileBasedProcessTask(_activator, ProcessTaskType.SQLBakFile,
                lsn.LoadMetadata, lsn.LoadStage);
        }

        if (Is(o, out LoadDirectoryNode ldn))
        {
            yield return new ExecuteCommandCreateNewDataLoadDirectory(_activator, ldn.LoadMetadata, null);
        }

        if (Is(o, out AllObjectImportsNode _))
            yield return new ExecuteCommandImportShareDefinitionList(_activator);

        if (Is(o, out AllPermissionWindowsNode _))
            yield return new ExecuteCommandCreateNewPermissionWindow(_activator);

        if (Is(o, out AllPluginsNode _))
        {
            yield return new ExecuteCommandAddPlugins(_activator);
            yield return new ExecuteCommandPrunePlugin(_activator);
            yield return new ExecuteCommandExportPlugins(_activator);
        }

        if (Is(o, out AllRDMPRemotesNode _))
            yield return new ExecuteCommandCreateNewRemoteRDMP(_activator);

        if (Is(o, out AllServersNode _))
        {
            yield return new ExecuteCommandImportTableInfo(_activator, null, false);
            yield return new ExecuteCommandBulkImportTableInfos(_activator);
        }

        if (Is(o, out IFilter filter))
        {
            if (filter.GetAllParameters().Any())
                yield return new ExecuteCommandSet(_activator,
                        () => _activator.SelectOne("Select Parameter to change Value for...",
                            filter.GetAllParameters().OfType<IMapsDirectlyToDatabaseTable>().ToArray()),
                        typeof(ISqlParameter).GetProperty(nameof(ISqlParameter.Value))
                    )
                { OverrideCommandName = "Set Parameter Value(s)", Weight = -10 };
            yield return new ExecuteCommandViewFilterMatchData(_activator, filter, ViewType.TOP_100);
            yield return new ExecuteCommandViewFilterMatchData(_activator, filter, ViewType.Aggregate);
        }


        if (Is(o, out TableInfo ti))
        {
            yield return new ExecuteCommandViewData(_activator, ti);

            yield return new ExecuteCommandImportTableInfo(_activator, null, false) { SuggestedCategory = New };
            yield return new ExecuteCommandCreateNewCatalogueFromTableInfo(_activator, ti) { SuggestedCategory = New };

            yield return new ExecuteCommandUseCredentialsToAccessTableInfoData(_activator, null, ti);

            yield return new ExecuteCommandScriptTable(_activator, ti);

            IAtomicCommand[] alterCommands = null;
            try
            {
                alterCommands = new IAtomicCommand[]
                {
                    new ExecuteCommandAlterTableName(_activator, ti) { SuggestedCategory = Alter },
                    new ExecuteCommandAlterTableCreatePrimaryKey(_activator, ti) { SuggestedCategory = Alter },
                    new ExecuteCommandAlterTableAddArchiveTrigger(_activator, ti) { SuggestedCategory = Alter },
                    new ExecuteCommandAlterTableMakeDistinct(_activator, ti) { SuggestedCategory = Alter }
                };
            }
            catch (Exception ex)
            {
                _activator.GlobalErrorCheckNotifier.OnCheckPerformed(
                    new CheckEventArgs("Failed to build Alter commands", CheckResult.Fail, ex));
            }

            if (alterCommands != null)
                foreach (var item in alterCommands)
                    yield return item;

            yield return new ExecuteCommandSyncTableInfo(_activator, ti, false, false);
            yield return new ExecuteCommandSyncTableInfo(_activator, ti, true, false);
            yield return new ExecuteCommandNewObject(_activator,
                () => new ColumnInfo(_activator.RepositoryLocator.CatalogueRepository, Guid.NewGuid().ToString(),
                    "fish", ti))
            { OverrideCommandName = "Add New ColumnInfo" };
        }

        if (Is(o, out ColumnInfo colInfo))
        {
            yield return new ExecuteCommandViewData(_activator, ViewType.TOP_100, colInfo) { SuggestedCategory = View };
            yield return new ExecuteCommandViewData(_activator, ViewType.Aggregate, colInfo)
            { SuggestedCategory = View };
            yield return new ExecuteCommandViewData(_activator, ViewType.Distribution, colInfo)
            { SuggestedCategory = View };

            yield return new ExecuteCommandAlterColumnType(_activator, colInfo) { SuggestedCategory = Alter };

            yield return new ExecuteCommandSet(_activator, colInfo,
                    typeof(ColumnInfo).GetProperty(nameof(ColumnInfo.IgnoreInLoads)))
            { OverrideCommandName = $"Ignore In Loads ({colInfo.IgnoreInLoads})" };
        }

        if (Is(o, out AllStandardRegexesNode _))
            yield return new ExecuteCommandCreateNewStandardRegex(_activator);

        if (Is(o, out ArbitraryFolderNode f))
            if (f.CommandGetter != null)
                foreach (var cmd in f.CommandGetter())
                    yield return cmd;

        if (Is(o, out CacheProgress cp))
            yield return new ExecuteCommandSetPermissionWindow(_activator, cp);

        if (Is(o, out SelectedDataSets sds))
        {
            yield return new ExecuteCommandSetExtractionIdentifier(_activator, sds.GetCatalogue(),
                    sds.ExtractionConfiguration, null)
            { Weight = -99.8f };

            ////////////// Add submenu ///////////////

            yield return new ExecuteCommandCreateNewFilter(_activator, sds)
            { OverrideCommandName = "New Filter", SuggestedCategory = Add };
            yield return new ExecuteCommandCreateNewFilter(_activator, sds)
            {
                OfferCatalogueFilters = true,
                OverrideCommandName = "Existing Filter (copy of)",
                SuggestedCategory = Add
            };

            yield return new ExecuteCommandAddNewFilterContainer(_activator, sds)
            { OverrideCommandName = "New Filter Container", SuggestedCategory = Add };
            yield return new ExecuteCommandImportFilterContainerTree(_activator, sds)
            { OverrideCommandName = "Existing Filter Container (copy of)", SuggestedCategory = Add };


            yield return new ExecuteCommandViewExtractionSql(_activator, sds);
            yield return new ExecuteCommandAddExtractionProgress(_activator, sds)
            { SuggestedCategory = Batching, Weight = 1.1f };
            yield return new ExecuteCommandResetExtractionProgress(_activator, sds)
            { SuggestedCategory = Batching, Weight = 1.2f };
        }

        if (Is(o, out ExtractionProgress progress))
            yield return new ExecuteCommandResetExtractionProgress(_activator, progress);

        if (Is(o, out ExtractionConfiguration ec))
        {
            ///////////////////Add//////////////

            yield return new ExecuteCommandChooseCohort(_activator, ec)
            { Weight = -99.8f, SuggestedCategory = Add, OverrideCommandName = "Existing Cohort" };
            yield return new ExecuteCommandAddDatasetsToConfiguration(_activator, ec)
            { Weight = -99.7f, SuggestedCategory = Add, OverrideCommandName = "Existing Datasets" };
            yield return new ExecuteCommandAddPackageToConfiguration(_activator, ec)
            { Weight = -99.6f, SuggestedCategory = Add, OverrideCommandName = "Existing Package" };
            yield return new ExecuteCommandAddParameter(_activator, ec, null, null, null)
            {
                Weight = -99.5f,
                SuggestedCategory = Add,
                OverrideCommandName = "New Extraction Filter Parameter"
            };


            yield return new ExecuteCommandGenerateReleaseDocument(_activator, ec) { Weight = -99.4f };

            yield return ec.IsReleased
                ? new ExecuteCommandUnfreezeExtractionConfiguration(_activator, ec) { Weight = 1.2f }
                : new ExecuteCommandFreezeExtractionConfiguration(_activator, ec) { Weight = 1.2f };

            yield return new ExecuteCommandCloneExtractionConfiguration(_activator, ec) { Weight = 1.3f };

            yield return new ExecuteCommandResetExtractionProgress(_activator, ec, null) { Weight = 1.4f };
        }

        if (Is(o, out Project proj))
        {
            yield return new ExecuteCommandCreateNewCohortIdentificationConfiguration(_activator)
            { OverrideCommandName = "New Cohort Builder Query", SuggestedCategory = Add, Weight = -5f }
                .SetTarget(proj);
            yield return
                new ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(_activator, null)
                {
                    OverrideCommandName = "New Cohort From Cohort Builder Query",
                    SuggestedCategory = Add,
                    Weight = -4.9f
                }.SetTarget(proj);
            yield return new ExecuteCommandCreateNewCohortFromFile(_activator, null)
            { OverrideCommandName = "New Cohort From File", SuggestedCategory = Add, Weight = -4.8f }
                .SetTarget(proj);
            yield return new ExecuteCommandCreateNewCohortFromCatalogue(_activator, (Catalogue)null)
            { OverrideCommandName = "New Cohort From Catalogue", SuggestedCategory = Add, Weight = -4.7f }
                .SetTarget(proj);
            yield return new ExecuteCommandCreateNewCohortFromTable(_activator, null)
            { OverrideCommandName = "New Cohort From Table", SuggestedCategory = Add, Weight = -4.6f }
                .SetTarget(proj);
            yield return new ExecuteCommandCreateNewExtractionConfigurationForProject(_activator, proj)
            {
                OverrideCommandName = "New Extraction Configuration",
                SuggestedCategory = Add,
                Weight = -2f
            };
            yield return new ExecuteCommandCreateNewCatalogueByImportingFile(_activator)
            {
                OverrideCommandName = "New Project Specific Catalogue From File...",
                SuggestedCategory = Add,
                Weight = -1.9f
            }.SetTarget(proj);
            yield return new ExecuteCommandCreateNewCatalogueByImportingExistingDataTable(_activator)
            {
                OverrideCommandName = "New Project Specific Catalogue From Database...",
                SuggestedCategory = Add,
                Weight = -1.8f
            }.SetTarget(proj);
        }

        if (Is(o, out ProjectCataloguesNode pcn))
        {
            yield return new ExecuteCommandMakeCatalogueProjectSpecific(_activator)
            { OverrideCommandName = "Add Existing Catalogue", Weight = -10 }.SetTarget(pcn.Project);
            yield return new ExecuteCommandCreateNewCatalogueByImportingFile(_activator)
            { OverrideCommandName = "Add New Catalogue From File", Weight = -9.5f }.SetTarget(pcn.Project);
            yield return new ExecuteCommandCreateNewCatalogueByImportingExistingDataTable(_activator)
            { OverrideCommandName = "Add New Catalogue From Existing Data Table", Weight = -9.4f }
                .SetTarget(pcn.Project);
        }

        if (Is(o, out ProjectCohortsNode projCohorts))
        {
            yield return new ExecuteCommandCreateNewCohortIdentificationConfiguration(_activator)
            { OverrideCommandName = "Add New Cohort Builder Query", Weight = -5.1f }.SetTarget(projCohorts.Project);
            yield return new ExecuteCommandAssociateCohortIdentificationConfigurationWithProject(_activator)
            { OverrideCommandName = "Add Existing Cohort Builder Query (link to)", Weight = -5f }
                .SetTarget(projCohorts.Project);
            yield return
                new ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(_activator, null)
                { OverrideCommandName = "Add New Cohort From Cohort Builder Query", Weight = -4.9f }
                    .SetTarget(projCohorts.Project);
            yield return new ExecuteCommandCreateNewCohortFromFile(_activator, null)
            { OverrideCommandName = "Add New Cohort From File", Weight = -4.8f }.SetTarget(projCohorts.Project);
            yield return new ExecuteCommandCreateNewCohortFromCatalogue(_activator, (Catalogue)null)
            { OverrideCommandName = "Add New Cohort From Catalogue", Weight = -4.7f }
                .SetTarget(projCohorts.Project);
            yield return new ExecuteCommandCreateNewCohortFromTable(_activator, null)
            { OverrideCommandName = "Add New Cohort From Table", Weight = -4.6f }.SetTarget(projCohorts.Project);
        }

        if (Is(o, out ProjectCohortIdentificationConfigurationAssociationsNode pccan))
        {
            yield return new ExecuteCommandCreateNewCohortIdentificationConfiguration(_activator)
            { OverrideCommandName = "Add New Cohort Builder Query", Weight = -5.1f }.SetTarget(pccan.Project);
            yield return new ExecuteCommandAssociateCohortIdentificationConfigurationWithProject(_activator)
            { OverrideCommandName = "Add Existing Cohort Builder Query (link to)", Weight = -5f }
                .SetTarget(pccan.Project);
        }

        if (Is(o, out ProjectSavedCohortsNode savedCohortsNode))
        {
            yield return
                new ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(_activator, null)
                { OverrideCommandName = "Add New Cohort From Cohort Builder Query", Weight = -4.9f }
                    .SetTarget(savedCohortsNode.Project);
            yield return new ExecuteCommandCreateNewCohortFromFile(_activator, null)
            { OverrideCommandName = "Add New Cohort From File", Weight = -4.8f }
                .SetTarget(savedCohortsNode.Project);
            yield return new ExecuteCommandCreateNewCohortFromCatalogue(_activator, (Catalogue)null)
            { OverrideCommandName = "Add New Cohort From Catalogue", Weight = -4.7f }
                .SetTarget(savedCohortsNode.Project);
            yield return new ExecuteCommandCreateNewCohortFromTable(_activator, null)
            { OverrideCommandName = "Add New Cohort From Table", Weight = -4.6f }
                .SetTarget(savedCohortsNode.Project);
            yield return new ExecuteCommandImportAlreadyExistingCohort(_activator, null, savedCohortsNode.Project);
        }

        if (Is(o, out ExtractionConfigurationsNode ecn))
            yield return new ExecuteCommandCreateNewExtractionConfigurationForProject(_activator, ecn.Project)
            { OverrideCommandName = "Add New Extraction Configuration", Weight = -4.7f };

        if (Is(o, out ExternalCohortTable ect))
        {
            var ectProj = o is CohortSourceUsedByProjectNode csbpn ? csbpn.User : null;

            yield return new ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(_activator,
                    null)
            {
                OverrideCommandName = "New Cohort From Cohort Builder Query",
                Weight = -4.9f,
                SuggestedCategory = "Add"
            }
                .SetTarget(ect)
                .SetTarget(ectProj);
            yield return new ExecuteCommandCreateNewCohortFromFile(_activator, null)
            { OverrideCommandName = "New Cohort From File", Weight = -4.8f, SuggestedCategory = "Add" }
                .SetTarget(ect)
                .SetTarget(ectProj);
            yield return new ExecuteCommandCreateNewCohortFromCatalogue(_activator, (Catalogue)null)
            { OverrideCommandName = "New Cohort From Catalogue", Weight = -4.7f, SuggestedCategory = "Add" }
                .SetTarget(ect)
                .SetTarget(ectProj);

            yield return new ExecuteCommandCreateNewCohortFromTable(_activator, null)
            { OverrideCommandName = "New Cohort From Table", Weight = -4.6f, SuggestedCategory = Add }
                .SetTarget(ect)
                .SetTarget(ectProj);

            yield return new ExecuteCommandImportAlreadyExistingCohort(_activator, ect, null)
            { OverrideCommandName = "Existing Cohort", Weight = -4.6f, SuggestedCategory = "Add" };

            yield return new ExecuteCommandRefreshBrokenCohorts(_activator, ect) { Weight = 1 };
        }

        if (Is(o, out ExtractableCohort cohort))
        {
            yield return new ExecuteCommandViewData(_activator, cohort, ViewType.TOP_100) { Weight = -99.9f };
            yield return new ExecuteCommandViewData(_activator, cohort, ViewType.All)
            {
                AskForFile = true,
                OverrideCommandName = "Save Cohort To File...",
                OverrideIcon = Image.Load<Rgba32>(FamFamFamIcons.disk),
                Weight = -99.8f
            };
            yield return new ExecuteCommandCreateNewExtractionConfigurationForProject(_activator)
            {
                CohortIfAny = cohort,
                OverrideCommandName = "New Extraction Configuration using Cohort"
            };
        }

        if (Is(o, out IMightBeDeprecated d))
        {
            yield return new ExecuteCommandDeprecate(_activator, new[] { d }, !d.IsDeprecated)
            {
                OverrideCommandName = d.IsDeprecated ? "Un Deprecate" : "Deprecate",
                SuggestedCategory = Deprecation,
                Weight = -99.7f
            };
            yield return new ExecuteCommandReplacedBy(_activator, d, null)
            {
                PromptToPickReplacement = true,
                SuggestedCategory = Deprecation,
                Weight = -99.6f,
                OverrideCommandName = "Set Replaced By"
            };
        }

        if (Is(o, out CohortAggregateContainer cohortAggregateContainer))
        {
            yield return new ExecuteCommandAddCatalogueToCohortIdentificationSetContainer(_activator,
                cohortAggregateContainer, null, null)
            { SuggestedCategory = Add, OverrideCommandName = "Catalogue" };
            yield return new ExecuteCommandAddCohortSubContainer(_activator, cohortAggregateContainer)
            { SuggestedCategory = Add, OverrideCommandName = "Sub Container" };
            yield return new ExecuteCommandAddAggregateConfigurationToCohortIdentificationSetContainer(_activator,
                    cohortAggregateContainer, true)
            { SuggestedCategory = Add, OverrideCommandName = "Existing Cohort Set (copy of)" };
            yield return new ExecuteCommandAddAggregateConfigurationToCohortIdentificationSetContainer(_activator,
                cohortAggregateContainer, false)
            { SuggestedCategory = Add, OverrideCommandName = "Aggregate" };
            yield return new ExecuteCommandImportCohortIdentificationConfiguration(_activator, null,
                    cohortAggregateContainer)
            { SuggestedCategory = Add, OverrideCommandName = "Existing Cohort Builder Query (copy of)" };

            //Set Operation
            yield return new ExecuteCommandSetContainerOperation(_activator, cohortAggregateContainer,
                SetOperation.UNION)
            { SuggestedCategory = SetContainerOperation, OverrideCommandName = "UNION" };
            yield return new ExecuteCommandSetContainerOperation(_activator, cohortAggregateContainer,
                SetOperation.EXCEPT)
            { SuggestedCategory = SetContainerOperation, OverrideCommandName = "EXCEPT" };
            yield return new ExecuteCommandSetContainerOperation(_activator, cohortAggregateContainer,
                    SetOperation.INTERSECT)
            { SuggestedCategory = SetContainerOperation, OverrideCommandName = "INTERSECT" };

            yield return new ExecuteCommandUnMergeCohortIdentificationConfiguration(_activator,
                cohortAggregateContainer)
            { OverrideCommandName = "Separate Cohort Builder Query" };
        }

        if (Is(o, out IDisableable disable))
            //todo this calls the db
            yield return new ExecuteCommandDisableOrEnable(_activator, disable);

        // If the root object is deletable offer deleting
        if (Is(o, out IDeleteable deletable))
            //todo this calls the db
            yield return new ExecuteCommandDelete(_activator, deletable) { SuggestedShortcut = "Delete" };

        if (Is(o, out ReferenceOtherObjectDatabaseEntity reference))
            yield return new ExecuteCommandShowRelatedObject(_activator, reference);

        if (Is(o, out INamed n))
            yield return new ExecuteCommandRename(_activator, n) { SuggestedShortcut = "F2" };

        if (Is(o, out PipelineCompatibleWithUseCaseNode pcu))
        {
            yield return new ExecuteCommandNewObject(_activator, typeof(Pipeline))
            { OverrideCommandName = "New Pipeline" };
            yield return new ExecuteCommandClonePipeline(_activator, pcu.Pipeline);
            yield return new ExecuteCommandAddPipelineComponent(_activator, pcu.Pipeline, pcu.UseCase);
        }
        else if (Is(o, out Pipeline pipeline))
        {
            yield return new ExecuteCommandNewObject(_activator, typeof(Pipeline))
            { OverrideCommandName = "New Pipeline" };
            yield return new ExecuteCommandClonePipeline(_activator, pipeline);
            yield return new ExecuteCommandAddPipelineComponent(_activator, pipeline, null);
        }

        if (Is(o, out StandardPipelineUseCaseNode psu))
            yield return new ExecuteCommandNewObject(_activator, typeof(Pipeline))
            { OverrideCommandName = "New Pipeline" };
    }

    public IEnumerable<IAtomicCommand> CreateManyObjectCommands(ICollection many)
    {
        if (many.Cast<object>().All(d => d is IDisableable))
            yield return new ExecuteCommandDisableOrEnable(_activator, many.Cast<IDisableable>().ToArray());
        if (many.Cast<object>().All(d => d is IHasFolder))
            yield return new ExecuteCommandPutIntoFolder(_activator, many.Cast<IHasFolder>().ToArray(), null);

        if (many.Cast<object>().All(t => t is TableInfo))
            yield return new ExecuteCommandScriptTables(_activator, many.Cast<TableInfo>().ToArray(), null, null, null);
        if (many.Cast<object>().All(t => t is CatalogueItem))
            yield return new ExecuteCommandChangeExtractionCategory(_activator,
                many.Cast<CatalogueItem>()
                    .Select(ci => ci.ExtractionInformation)
                    .Where(ei => ei != null).ToArray(), null);

        if (many.Cast<object>().All(d => d is IDeleteable))
            yield return new ExecuteCommandDelete(_activator, many.Cast<IDeleteable>().ToArray())
            { SuggestedShortcut = "Delete" };

        if (many.Cast<object>().All(d => d is ExtractionFilterParameterSet))
            yield return new ExecuteCommandAddMissingParameters(_activator,
                many.Cast<ExtractionFilterParameterSet>().ToArray());

        // Deprecate/UnDeprecate many items at once if all share the same state (all true or all false)
        if (many.Cast<object>().All(d => d is IMightBeDeprecated))
        {
            var dep = many.Cast<IMightBeDeprecated>().ToArray();

            if (dep.All(d => d.IsDeprecated))
                yield return new ExecuteCommandDeprecate(_activator, dep, false)
                {
                    SuggestedShortcut = "UnDeprecate"
                };
            else if (dep.All(d => !d.IsDeprecated))
                yield return new ExecuteCommandDeprecate(_activator, dep, true)
                {
                    SuggestedShortcut = "Deprecate"
                };
        }
    }
}