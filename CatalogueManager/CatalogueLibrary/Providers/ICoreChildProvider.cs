// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.Cohort.Joinables;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.Governance;
using CatalogueLibrary.Data.ImportExport;
using CatalogueLibrary.Nodes;
using CatalogueLibrary.Nodes.PipelineNodes;
using CatalogueLibrary.Nodes.SharingNodes;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Providers
{
    /// <summary>
    /// Extension of IChildProvider which also lists all the high level cached objects so that if you need to fetch objects from the database to calculate 
    /// things you don't expect to have been the result of an immediate user change you can access the cached object from one of these arrays instead.  For 
    /// example if you want to know whether you are within the PermissionWindow of your CacheProgress when picking an icon and you only have the PermissionWindow_ID
    /// property you can just look at the array AllPermissionWindows (especially since you might get lots of spam requests for the icon - you don't want to lookup
    /// the PermissionWindow from the database every time).
    /// </summary>
    public interface ICoreChildProvider:IChildProvider
    {
        LoadMetadata[] AllLoadMetadatas { get; }
        TableInfoServerNode[] AllServers { get; }
        TableInfo[] AllTableInfos { get;}
        CohortIdentificationConfiguration[] AllCohortIdentificationConfigurations { get; }
        
        Catalogue[] AllCatalogues { get; }
        Dictionary<int, Catalogue> AllCatalogueDictionary { get; }

        ExternalDatabaseServer[] AllExternalServers { get; }

        AllANOTablesNode AllANOTablesNode { get; }
        ANOTable[] AllANOTables { get; }
        AllDataAccessCredentialsNode AllDataAccessCredentialsNode { get; }
        AllServersNode AllServersNode { get;}
        ColumnInfo[] AllColumnInfos { get;}
        AllExternalServersNode AllExternalServersNode { get; }
        DescendancyList GetDescendancyListIfAnyFor(object model);
        PermissionWindow[] AllPermissionWindows { get;}
        IEnumerable<CatalogueItem> AllCatalogueItems { get; }
        AggregateConfiguration[] AllAggregateConfigurations { get;}
        AllRDMPRemotesNode AllRDMPRemotesNode { get; }
        AllObjectSharingNode AllObjectSharingNode { get; }
        ObjectImport[] AllImports { get; }
        ObjectExport[] AllExports { get; }

        Dictionary<IMapsDirectlyToDatabaseTable, DescendancyList> GetAllSearchables();
        IEnumerable<object> GetAllChildrenRecursively(object o);
        IEnumerable<ExtractionInformation> AllExtractionInformations { get; }
        
        AllPermissionWindowsNode AllPermissionWindowsNode { get; set; }
        AllLoadMetadatasNode AllLoadMetadatasNode { get; set; }
        AllConnectionStringKeywordsNode AllConnectionStringKeywordsNode { get; set; }
        AllStandardRegexesNode AllStandardRegexesNode { get;}
        AllPipelinesNode AllPipelinesNode { get; }

        CatalogueItemIssue[] AllCatalogueItemIssues { get; }

        AllGovernanceNode AllGovernanceNode { get; }
        GovernancePeriod[] AllGovernancePeriods { get; }
        GovernanceDocument[] AllGovernanceDocuments { get;}

        /// <inheritdoc cref="GovernancePeriod.GetAllGovernedCataloguesForAllGovernancePeriods"/>
        Dictionary<int, HashSet<int>> GovernanceCoverage { get;}

        JoinableCohortAggregateConfigurationUse[] AllJoinableCohortAggregateConfigurationUse { get; }

        void GetPluginChildren(HashSet<object> objectsToAskAbout = null);

        /// <summary>
        /// Returns all known objects who are masquerading as o
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        IEnumerable<IMasqueradeAs> GetMasqueradersOf(object o);
    }
}