using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using DataExportLibrary.DataRelease.Audit;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Injection;
using ReusableLibraryCode;

namespace DataExportLibrary.Data.DataTables
{
    /// <summary>
    /// Data Export Manager keeps a record of the final SQL generated/used to perform an extraction of any ExtractionConfiguration in records called CumulativeExtractionResults.
    /// These records include the SQL executed, the date, the filters, the number of records extracted etc.  This is in addition to the information logged in the Logging database.
    /// This record is overwritten if you re-extract the ExtractionConfiguration again.  The record is used to ensure that you cannot release an extract if there have been changes
    /// to the configuration subsequent to your last extraction.  This is particularly useful if you have many large datasets that you are extracting over a long period of time either
    /// because they are very large, have complex filters or are liable to crash on a semi regular basis.  Under such circumstances you can extract half of your datasets one day and 
    /// then adjust the others to correct issues and be confident that the system is tracking those changes to ensure that the current state of the system always matches the extracted
    /// files at release time.
    /// </summary>
    public class CumulativeExtractionResults : VersionedDatabaseEntity, ICumulativeExtractionResults, IInjectKnown<IExtractableDataSet>
    {
        #region Database Properties
        private int _extractionConfiguration_ID;
        private int _extractableDataSet_ID;
        private DateTime _dateOfExtraction;
        private string _destinationType;
        private string _destinationDescription;
        private int _recordsExtracted;
        private int _distinctReleaseIdentifiersEncountered;
        private string _filtersUsed;
        private string _exception;
        private string _sQLExecuted;
        private int _cohortExtracted;
        private Lazy<IExtractableDataSet> _knownExtractableDataSet;

        public int ExtractionConfiguration_ID
        {
            get { return _extractionConfiguration_ID; }
            set { SetField(ref _extractionConfiguration_ID, value); }
        }
        public int ExtractableDataSet_ID
        {
            get { return _extractableDataSet_ID; }
            private set { SetField(ref _extractableDataSet_ID, value); }
        }
        public DateTime DateOfExtraction
        {
            get { return _dateOfExtraction; }
            private set { SetField(ref _dateOfExtraction, value); }
        }
        public string DestinationType
        {
            get { return _destinationType; }
            private set { SetField(ref _destinationType, value); }
        }
        public string DestinationDescription
        {
            get { return _destinationDescription; }
            private set { SetField(ref _destinationDescription, value); }
        }
        public int RecordsExtracted
        {
            get { return _recordsExtracted; }
            set { SetField(ref _recordsExtracted, value); }
        }
        public int DistinctReleaseIdentifiersEncountered
        {
            get { return _distinctReleaseIdentifiersEncountered; }
            set { SetField(ref _distinctReleaseIdentifiersEncountered, value); }
        }
        public string FiltersUsed
        {
            get { return _filtersUsed; }
            set { SetField(ref _filtersUsed, value); }
        }
        public string Exception
        {
            get { return _exception; }
            set { SetField(ref _exception, value); }
        }
        public string SQLExecuted
        {
            get { return _sQLExecuted; }
            private set { SetField(ref _sQLExecuted, value); }
        }
        public int CohortExtracted
        {
            get { return _cohortExtracted; }
            private set { SetField(ref _cohortExtracted, value); }
        }

        #endregion

        #region Relationships
        /// <inheritdoc cref="ExtractableDataSet_ID"/>
        [NoMappingToDatabase]
        public IExtractableDataSet ExtractableDataSet
        {
            get
            {
                return _knownExtractableDataSet.Value;
            }
        }

        [NoMappingToDatabase]
        public List<ISupplementalExtractionResults> SupplementalExtractionResults
        {
            get { return new List<ISupplementalExtractionResults>(Repository.GetAllObjectsWithParent<SupplementalExtractionResults>(this)); }
        }

        public ISupplementalExtractionResults AddSupplementalExtractionResult(string sqlExecuted, IMapsDirectlyToDatabaseTable extractedObject)
        {
            var result = new SupplementalExtractionResults(Repository, this, sqlExecuted, extractedObject);
            SupplementalExtractionResults.Add(result);
            return result;
        }

        public bool IsFor(ISelectedDataSets selectedDataSet)
        {
            return selectedDataSet.ExtractableDataSet_ID == ExtractableDataSet_ID &&
                   selectedDataSet.ExtractionConfiguration_ID == ExtractionConfiguration_ID;
        }

        #endregion

        public CumulativeExtractionResults(IDataExportRepository repository, IExtractionConfiguration configuration, IExtractableDataSet dataset, string sql)
        {
            Repository = repository;
            Repository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                {"ExtractionConfiguration_ID", configuration.ID},
                {"ExtractableDataSet_ID", dataset.ID},
                {"SQLExecuted", sql},
                {"CohortExtracted", configuration.Cohort_ID}
            });

            ClearAllInjections();
        }

        internal CumulativeExtractionResults(IDataExportRepository repository, DbDataReader r)
            : base(repository, r)
        {
            ExtractionConfiguration_ID = int.Parse(r["ExtractionConfiguration_ID"].ToString());
            ExtractableDataSet_ID = int.Parse(r["ExtractableDataSet_ID"].ToString());
            DateOfExtraction = (DateTime)r["DateOfExtraction"];
            RecordsExtracted = int.Parse(r["RecordsExtracted"].ToString());
            DistinctReleaseIdentifiersEncountered = int.Parse(r["DistinctReleaseIdentifiersEncountered"].ToString());
            Exception = r["Exception"] as string;
            FiltersUsed = r["FiltersUsed"] as string;
            DestinationType = r["DestinationType"] as string;
            DestinationDescription = r["DestinationDescription"] as string;
            SQLExecuted = r["SQLExecuted"] as string;
            CohortExtracted = int.Parse(r["CohortExtracted"].ToString());

            ClearAllInjections();
        }

        public IReleaseLogEntry GetReleaseLogEntryIfAny()
        {
            var repo = (DataExportRepository)Repository;
            using (var con = repo.GetConnection())
            {
                var cmdselect = DatabaseCommandHelper
                    .GetCommand(@"SELECT *
                                    FROM ReleaseLog
                                    where
                                    CumulativeExtractionResults_ID = " + ID, con.Connection, con.Transaction);

                var r = cmdselect.ExecuteReader();
                if (r.Read())
                    return new ReleaseLogEntry(Repository, r);

                return null;
            }
        }

        public Type GetDestinationType()
        {
            return ((DataExportRepository)Repository).CatalogueRepository.MEF.GetTypeByNameFromAnyLoadedAssembly(_destinationType);
        }
        
        public void CompleteAudit(Type destinationType, string destinationDescription, int recordsExtracted)
        {
            DestinationType = destinationType.FullName;
            DestinationDescription = destinationDescription;
            RecordsExtracted = recordsExtracted;

            SaveToDatabase();
        }

        public override string ToString()
        {
            return ExtractableDataSet.Catalogue.Name;
        }

        public void InjectKnown(IExtractableDataSet instance)
        {
            _knownExtractableDataSet = new Lazy<IExtractableDataSet>(()=>instance);
        }

        public void ClearAllInjections()
        {
            _knownExtractableDataSet = new Lazy<IExtractableDataSet>(() => Repository.GetObjectByID<ExtractableDataSet>(ExtractableDataSet_ID));
        }
    }
}
