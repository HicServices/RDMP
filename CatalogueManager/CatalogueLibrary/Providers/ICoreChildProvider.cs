using System.Collections;
using System.Collections.Generic;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.ImportExport;
using CatalogueLibrary.Data.PerformanceImprovement;
using CatalogueLibrary.Nodes;
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

        void GetPluginChildren(HashSet<object> objectsToAskAbout = null);
    }
}