using System;
using System.Collections.Generic;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.EntityNaming;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Revertable;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// See Catalogue
    /// </summary>
    public interface ICatalogue : IRevertable, IHasDependencies, IHasQuerySyntaxHelper
    {
        int? LoadMetadata_ID { get; }
        string LoggingDataTask { get; set; }
        int? LiveLoggingServer_ID { get; set; }
        string Name { get; }
        string ValidatorXML { get; set; }
        int? TimeCoverage_ExtractionInformation_ID { get; set; }
        int? PivotCategory_ExtractionInformation_ID { get; set; }
        bool IsDeprecated { get; set; }
        bool IsInternalDataset { get; set; }
        bool IsColdStorageDataset { get; set; }
        DateTime? DatasetStartDate { get; set; }
        ExtractionInformation TimeCoverage_ExtractionInformation { get; }
        ExtractionInformation PivotCategory_ExtractionInformation { get; }
        LoadMetadata LoadMetadata { get; }
        CatalogueItem[] CatalogueItems { get; }
        AggregateConfiguration[] AggregateConfigurations { get; }
        ExternalDatabaseServer LiveLoggingServer { get; }

        string Acronym { get; set; }

        /// <summary>
        /// In the context of the DLE, you should use current jobs tableinfo list where possible for performance gain etc
        /// </summary>
        /// <param name="includeLookupTables"></param>
        /// <returns></returns>
        TableInfo[] GetTableInfoList(bool includeLookupTables);
        TableInfo[] GetLookupTableInfoList();
        
        Dictionary<string, string> GetListOfTableNameMappings(LoadBubble destination, INameDatabasesAndTablesDuringLoads namer);

        DiscoveredServer GetDistinctLiveDatabaseServer(DataAccessContext context, bool setInitialDatabase, out IDataAccessPoint distinctAccessPoint);
        DiscoveredServer GetDistinctLiveDatabaseServer(DataAccessContext context, bool setInitialDatabase);

        CatalogueItemIssue[] GetAllIssues();
        SupportingSQLTable[] GetAllSupportingSQLTablesForCatalogue(FetchOptions fetch);
        ExtractionInformation[] GetAllExtractionInformation(ExtractionCategory category);
        SupportingDocument[] GetAllSupportingDocuments(FetchOptions fetch);
        void GetTableInfos(out List<TableInfo> normalTables, out List<TableInfo> lookupTables);
        ExtractionFilter[] GetAllMandatoryFilters();
        ExtractionFilter[] GetAllFilters();

        DatabaseType? GetDistinctLiveDatabaseServerType();

        CatalogueExtractabilityStatus GetExtractabilityStatus(IDataExportRepository dataExportRepository);
    }
}