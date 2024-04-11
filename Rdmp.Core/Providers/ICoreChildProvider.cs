// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Cohort.Joinables;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Governance;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.Providers.Nodes.CohortNodes;
using Rdmp.Core.Providers.Nodes.PipelineNodes;
using Rdmp.Core.Providers.Nodes.SharingNodes;
using Rdmp.Core.Repositories.Managers;

namespace Rdmp.Core.Providers;

/// <summary>
/// Extension of IChildProvider which also lists all the high level cached objects so that if you need to fetch objects from the database to calculate
/// things you don't expect to have been the result of an immediate user change you can access the cached object from one of these arrays instead.  For
/// example if you want to know whether you are within the PermissionWindow of your CacheProgress when picking an icon and you only have the PermissionWindow_ID
/// property you can just look at the array AllPermissionWindows (especially since you might get lots of spam requests for the icon - you don't want to lookup
/// the PermissionWindow from the database every time).
/// </summary>
public interface ICoreChildProvider : IChildProvider
{
    Lazy<JoinInfo[]> AllJoinInfos { get; }
    Lazy<LoadMetadata[]> AllLoadMetadatas { get; }
    Lazy<TableInfoServerNode[]> AllServers { get; }
    Lazy<TableInfo[]> AllTableInfos { get; }
    Lazy<Dictionary<int, List<ColumnInfo>>> TableInfosToColumnInfos { get; }
    Lazy<CohortIdentificationConfiguration[]> AllCohortIdentificationConfigurations { get; }
    Lazy<CohortAggregateContainer[]> AllCohortAggregateContainers { get; set; }
    Lazy<JoinableCohortAggregateConfiguration[]> AllJoinables { get; set; }
    Lazy<JoinableCohortAggregateConfigurationUse[]> AllJoinUses { get; set; }

    Lazy<FolderNode<Catalogue>> CatalogueRootFolder { get; }
    Lazy<FolderNode<Curation.Data.Dataset>> DatasetRootFolder { get; }
    Lazy<FolderNode<LoadMetadata>> LoadMetadataRootFolder { get; }
    Lazy<FolderNode<CohortIdentificationConfiguration>> CohortIdentificationConfigurationRootFolder { get; }

    Lazy<Catalogue[]> AllCatalogues { get; }
    Lazy<Curation.Data.Dataset[]> AllDatasets { get; }
    Lazy<Dictionary<int, Catalogue>> AllCataloguesDictionary { get; }

    Lazy<ExternalDatabaseServer[]> AllExternalServers { get; }

    Lazy<AllANOTablesNode> AllANOTablesNode { get; }
    Lazy<ANOTable[]> AllANOTables { get; }
    Lazy<AllDataAccessCredentialsNode> AllDataAccessCredentialsNode { get; }
    Lazy<AllServersNode> AllServersNode { get; }

    Lazy<ColumnInfo[]> AllColumnInfos { get; }

    Lazy<Lookup[]> AllLookups { get; }
    Lazy<AllExternalServersNode> AllExternalServersNode { get; }
    DescendancyList GetDescendancyListIfAnyFor(object model);

    /// <summary>
    /// Returns the root level object in the descendancy of <paramref name="model"/> or <paramref name="model"/>
    /// if no descendancy is known.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    object GetRootObjectOrSelf(object model);

    Lazy<PermissionWindow[]> AllPermissionWindows { get; }
    Lazy<IEnumerable<CatalogueItem>> AllCatalogueItems { get; }
    Lazy<Dictionary<int, CatalogueItem>> AllCatalogueItemsDictionary { get; }
    Lazy<AggregateConfiguration[]> AllAggregateConfigurations { get; }
    Lazy<AllRDMPRemotesNode> AllRDMPRemotesNode { get; }

    Lazy<AllDashboardsNode> AllDashboardsNode { get; }
    Lazy<DashboardLayout[]> AllDashboards { get; }

    Lazy<AllObjectSharingNode> AllObjectSharingNode { get; }
    Lazy<ObjectImport[]> AllImports { get; }
    Lazy<ObjectExport[]> AllExports { get; }

    Lazy<AllPluginsNode> AllPluginsNode { get; }

    Dictionary<IMapsDirectlyToDatabaseTable, DescendancyList> GetAllSearchables();
    IEnumerable<object> GetAllChildrenRecursively(object o);
    Lazy<IEnumerable<ExtractionInformation>> AllExtractionInformations { get; }

    Lazy<Dictionary<int, ExtractionInformation>> AllExtractionInformationsDictionary { get; }

    Lazy<AllPermissionWindowsNode> AllPermissionWindowsNode { get; set; }
    Lazy<AllConnectionStringKeywordsNode> AllConnectionStringKeywordsNode { get; set; }
    Lazy<AllStandardRegexesNode> AllStandardRegexesNode { get; }
    Lazy<AllPipelinesNode> AllPipelinesNode { get; }

    Lazy<AllGovernanceNode> AllGovernanceNode { get; }
    Lazy<GovernancePeriod[]> AllGovernancePeriods { get; }
    Lazy<GovernanceDocument[]> AllGovernanceDocuments { get; }

    Lazy<Dictionary<int, AggregateFilterContainer>> AllAggregateContainersDictionary { get; }
    Lazy<AggregateFilter[]> AllAggregateFilters { get; }

    /// <inheritdoc cref="IGovernanceManager.GetAllGovernedCataloguesForAllGovernancePeriods"/>
    Lazy<Dictionary<int, System.Collections.Generic.HashSet<int>>> GovernanceCoverage { get; }

    Lazy<JoinableCohortAggregateConfigurationUse[]> AllJoinableCohortAggregateConfigurationUse { get; }


    /// <summary>
    /// Copy updated values for all properties from the <paramref name="other"/>
    /// </summary>
    /// <param name="other"></param>
    void UpdateTo(ICoreChildProvider other);

    /// <summary>
    /// Returns all known objects who are masquerading as o
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    IEnumerable<IMasqueradeAs> GetMasqueradersOf(object o);


    Lazy<AllOrphanAggregateConfigurationsNode> OrphanAggregateConfigurationsNode { get; }
    Lazy<AllTemplateAggregateConfigurationsNode> TemplateAggregateConfigurationsNode { get; }

    /// <summary>
    /// All standard (i.e. not plugin) use cases for editting <see cref="IPipeline"/> under.
    /// </summary>
    Lazy<HashSet<StandardPipelineUseCaseNode>> PipelineUseCases { get; }

    /// <summary>
    /// All components within all <see cref="Pipeline"/>
    /// </summary>
    Lazy<PipelineComponent[]> AllPipelineComponents { get; }

    /// <summary>
    /// All arguments for the <see cref="AllPipelineComponents"/>
    /// </summary>
    Lazy<PipelineComponentArgument[]> AllPipelineComponentsArguments { get; }

    /// <summary>
    /// All process
    /// </summary>
    Lazy<ProcessTask[]> AllProcessTasks { get; }

    Lazy<ProcessTaskArgument[]> AllProcessTasksArguments { get; }


    /// <summary>
    /// Returns all objects in the tree hierarchy that are assignable to the supplied <paramref name="type"/>
    /// </summary>
    /// <param name="type"></param>
    /// <param name="unwrapMasqueraders">true to unwrap and return matching underlying objects from <see cref="IMasqueradeAs"/> objects</param>
    /// <returns></returns>
    IEnumerable<IMapsDirectlyToDatabaseTable> GetAllObjects(Type type, bool unwrapMasqueraders);

    /// <summary>
    /// Performs a partial refresh assuming that only the hierarchy of <paramref name="databaseEntity"/> has
    /// changed
    /// </summary>
    /// <returns>True if it was possible to selectively refresh part of the child provider</returns>
    /// <param name="databaseEntity"></param>
    bool SelectiveRefresh(IMapsDirectlyToDatabaseTable databaseEntity);
}