using System.Collections;
using System.Collections.Generic;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.PerformanceImprovement;
using CatalogueLibrary.Nodes;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Providers
{
    public interface ICoreChildProvider:IChildProvider
    {
        Dictionary<int, CatalogueItemClassification> CatalogueItemClassifications { get; }
        LoadMetadata[] AllLoadMetadatas { get; }
        TableInfoServerNode[] AllServers { get; }
        TableInfo[] AllTableInfos { get;}
        CohortIdentificationConfiguration[] AllCohortIdentificationConfigurations { get; }
        Catalogue[] AllCatalogues { get; }
        ANOTablesNode ANOTablesNode { get;}
        ANOTable[] AllANOTables { get; }
        DataAccessCredentialsNode DataAccessCredentialsNode { get; }
        AllServersNode AllServersNode { get;}
        ColumnInfo[] AllColumnInfos { get;}
        AllExternalServersNode AllExternalServersNode { get; }
        DescendancyList GetDescendancyListIfAnyFor(object model);
        PermissionWindow[] AllPermissionWindows { get;}
        AggregateConfiguration[] AllAggregateConfigurations { get;}

        Dictionary<IMapsDirectlyToDatabaseTable, DescendancyList> GetAllSearchables();
    }
}