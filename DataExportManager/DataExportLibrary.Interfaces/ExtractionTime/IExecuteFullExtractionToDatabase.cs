using System.Data.Common;
using DataExportLibrary.Interfaces.Data;
using DataExportLibrary.Interfaces.Data.DataTables;
using HIC.Logging;
using ReusableLibraryCode;

namespace DataExportLibrary.Interfaces.ExtractionTime
{
    public interface IExecuteFullExtractionToDatabase
    {
        bool Abort { get; set; }

        string TargetServer { get; }
        string DatabaseNameForDatasetData { get; }
        string DatabaseNameForCatalogue { get;}

        DatabaseType TargetDatabaseType { get; }

        /// <summary>
        /// Do not specify any specific database and you should have CREATE/DROP rights, this connection string will be used for drop and/or create the required data tables on the target database server to receive the extracted dataset
        /// </summary>
        string TargetAdministratorUserConnectionString { get; }

        /// <summary>
        /// Should point at the database which will store the extracted project data.  This connection string should be the same as TargetAdministratorUserConnectionString except that it only needs read/write privelleges.
        /// </summary>
        string TargetDatasetConnectionString { get;}

        /// <summary>
        /// Should point at the Catalogue database which will store metadata for the extracted project data.  This connection string should be the same as TargetAdministratorUserConnectionString except that it only needs read/write privelleges.
        /// </summary>
        string TargetCatalogueConnectionString { get; }

        /// <summary>
        /// A connection string to the server that the data is being read from i.e. if this is an extraction for project A then the SourceDataConnectionString will point at the live server with all the unanonymised data of which A is only a subset
        /// </summary>
        string SourceDataConnectionString { get; }

        /// <summary>
        /// Call this method to populate the target server with the extraction, feel free to make use of ExecuteFullExtractionToDatabaseHelper to help achieve this
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="extractableCohort"></param>
        /// <param name="dataset"></param>
        /// <param name="dataLoadInfo"></param>
        /// <param name="salt"></param>
        void ExecuteDataset(IExtractionConfiguration configuration, IExtractableCohort extractableCohort, IExtractableDataSet dataset, DataLoadInfo dataLoadInfo, IHICProjectSalt salt);

        /// <summary>
        /// Should create a new connection of the appropriate type and return it, connectionString should be either TargetAdministratorUserConnectionString,TargetCatalogueConnectionString or TargetCatalogueConnectionString but
        /// it could have been modified e.g. to add support for MARS, Async Queries etc.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        DbConnection GetNewConnection(string connectionString);
    }
}
