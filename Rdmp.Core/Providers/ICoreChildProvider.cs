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
    JoinInfo[] AllJoinInfos { get; }
    LoadMetadata[] AllLoadMetadatas { get; }
    LoadMetadataCatalogueLinkage[] AllLoadMetadataCatalogueLinkages { get; }
    TableInfoServerNode[] AllServers { get; }
    TableInfo[] AllTableInfos { get; }
    Dictionary<int, List<ColumnInfo>> TableInfosToColumnInfos { get; }
    CohortIdentificationConfiguration[] AllCohortIdentificationConfigurations { get; }
    CohortAggregateContainer[] AllCohortAggregateContainers { get; set; }
    JoinableCohortAggregateConfiguration[] AllJoinables { get; set; }
    JoinableCohortAggregateConfigurationUse[] AllJoinUses { get; set; }

    FolderNode<Catalogue> CatalogueRootFolder { get; }
    FolderNode<Curation.Data.Dataset> DatasetRootFolder { get; }
    FolderNode<LoadMetadata> LoadMetadataRootFolder { get; }
    FolderNode<CohortIdentificationConfiguration> CohortIdentificationConfigurationRootFolder { get; }
    FolderNode<CohortIdentificationConfiguration> CohortIdentificationConfigurationRootFolderWithoutVersionedConfigurations { get; }
    Catalogue[] AllCatalogues { get; }
    Curation.Data.Dataset[] AllDatasets { get; }
    Dictionary<int, Catalogue> AllCataloguesDictionary { get; }

    ExternalDatabaseServer[] AllExternalServers { get; }

    AllANOTablesNode AllANOTablesNode { get; }
    ANOTable[] AllANOTables { get; }
    AllDataAccessCredentialsNode AllDataAccessCredentialsNode { get; }
    AllServersNode AllServersNode { get; }
    ColumnInfo[] AllColumnInfos { get; }
    Lookup[] AllLookups { get; }
    AllExternalServersNode AllExternalServersNode { get; }
    DescendancyList GetDescendancyListIfAnyFor(object model);

    /// <summary>
    /// Returns the root level object in the descendancy of <paramref name="model"/> or <paramref name="model"/>
    /// if no descendancy is known.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    object GetRootObjectOrSelf(object model);

    PermissionWindow[] AllPermissionWindows { get; }
    IEnumerable<CatalogueItem> AllCatalogueItems { get; }
    Dictionary<int, CatalogueItem> AllCatalogueItemsDictionary { get; }
    AggregateConfiguration[] AllAggregateConfigurations { get; }
    AllRDMPRemotesNode AllRDMPRemotesNode { get; }

    AllDashboardsNode AllDashboardsNode { get; }
    DashboardLayout[] AllDashboards { get; }

    AllDatasetsNode AllDatasetsNode { get; }

    AllRegexRedactionConfigurationsNode AllRegexRedactionConfigurationsNode { get; }

    AllObjectSharingNode AllObjectSharingNode { get; }
    ObjectImport[] AllImports { get; }
    ObjectExport[] AllExports { get; }

    AllPluginsNode AllPluginsNode { get; }

    Dictionary<IMapsDirectlyToDatabaseTable, DescendancyList> GetAllSearchables();
    IEnumerable<object> GetAllChildrenRecursively(object o);
    IEnumerable<ExtractionInformation> AllExtractionInformations { get; }

    Dictionary<int, ExtractionInformation> AllExtractionInformationsDictionary { get; }

    AllPermissionWindowsNode AllPermissionWindowsNode { get; set; }
    AllConnectionStringKeywordsNode AllConnectionStringKeywordsNode { get; set; }
    AllStandardRegexesNode AllStandardRegexesNode { get; }
    AllPipelinesNode AllPipelinesNode { get; }

    AllGovernanceNode AllGovernanceNode { get; }
    GovernancePeriod[] AllGovernancePeriods { get; }
    GovernanceDocument[] AllGovernanceDocuments { get; }

    Dictionary<int, AggregateFilterContainer> AllAggregateContainersDictionary { get; }
    AggregateFilter[] AllAggregateFilters { get; }

    /// <inheritdoc cref="IGovernanceManager.GetAllGovernedCataloguesForAllGovernancePeriods"/>
    Dictionary<int, HashSet<int>> GovernanceCoverage { get; }

    JoinableCohortAggregateConfigurationUse[] AllJoinableCohortAggregateConfigurationUse { get; }


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


    AllOrphanAggregateConfigurationsNode OrphanAggregateConfigurationsNode { get; }
    AllTemplateAggregateConfigurationsNode TemplateAggregateConfigurationsNode { get; }

    /// <summary>
    /// All standard (i.e. not plugin) use cases for editing <see cref="IPipeline"/> under.
    /// </summary>
    HashSet<StandardPipelineUseCaseNode> PipelineUseCases { get; }


    /// <summary>
    /// All components within all <see cref="Pipeline"/>
    /// </summary>
    PipelineComponent[] AllPipelineComponents { get; }

    /// <summary>
    /// All arguments for the <see cref="AllPipelineComponents"/>
    /// </summary>
    PipelineComponentArgument[] AllPipelineComponentsArguments { get; }

    /// <summary>
    /// All process
    /// </summary>
    ProcessTask[] AllProcessTasks { get; }

    ProcessTaskArgument[] AllProcessTasksArguments { get; }


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