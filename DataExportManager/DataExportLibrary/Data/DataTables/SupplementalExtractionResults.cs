using System;
using System.Collections.Generic;
using System.Data.Common;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Interfaces.Data.DataTables;
using MapsDirectlyToDatabaseTable;

namespace DataExportLibrary.Data.DataTables
{
    public class SupplementalExtractionResults : DatabaseEntity, ISupplementalExtractionResults
    {
        #region Database Properties

        private int? _cumulativeExtractionResults_ID;
        private int? _extractionConfiguration_ID;
        private string _destinationDescription;
        private int _recordsExtracted;
        private string _exception;
        private string _sQLExecuted;
        private string _extractedType;
        private int _extractedId;

        public int? CumulativeExtractionResults_ID
        {
            get { return _cumulativeExtractionResults_ID; }
            set { SetField(ref _cumulativeExtractionResults_ID, value); }
        }
        public int? ExtractionConfiguration_ID
        {
            get { return _extractionConfiguration_ID; }
            set { SetField(ref _extractionConfiguration_ID, value); }
        }
        public string DestinationDescription
        {
            get { return _destinationDescription; }
            set { SetField(ref _destinationDescription, value); }
        }
        public int RecordsExtracted
        {
            get { return _recordsExtracted; }
            set { SetField(ref _recordsExtracted, value); }
        }
        public string Exception
        {
            get { return _exception; }
            set { SetField(ref _exception, value); }
        }
        public string SQLExecuted
        {
            get { return _sQLExecuted; }
            set { SetField(ref _sQLExecuted, value); }
        }
        public string ExtractedType
        {
            get { return _extractedType; }
            set { SetField(ref _extractedType, value); }
        }
        public int ExtractedId
        {
            get { return _extractedId; }
            set { SetField(ref _extractedId, value); }
        }

        #endregion

        [NoMappingToDatabase]
        public bool IsGlobal { get; private set; }

        public SupplementalExtractionResults(IRepository repository, IExtractionConfiguration configuration, string sql, Type getType, int id)
        {
            Repository = repository;
            Repository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                {"ExtractionConfiguration_ID", configuration.ID},
                {"SQLExecuted", sql},
                {"ExtractedType", getType.FullName },
                {"ExtractedId", id }
            });
        }

        public SupplementalExtractionResults(IRepository repository, ICumulativeExtractionResults configuration, string sql, Type getType, int id)
        {
            Repository = repository;
            Repository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                {"CumulativeExtractionResults_ID", configuration.ID},
                {"SQLExecuted", sql},
                {"ExtractedType", getType.FullName },
                {"ExtractedId", id }
            });
        }

        internal SupplementalExtractionResults(IRepository repository, DbDataReader r)
            : base(repository, r)
        {
            CumulativeExtractionResults_ID = ObjectToNullableInt(r["CumulativeExtractionResults_ID"]);
            ExtractionConfiguration_ID = ObjectToNullableInt(r["ExtractionConfiguration_ID"]);
            DestinationDescription = r["DestinationDescription"] as string;
            RecordsExtracted = r["RecordsExtracted"] is DBNull ? 0 : Convert.ToInt32(r["RecordsExtracted"]);
            Exception = r["Exception"] as string;
            SQLExecuted = r["SQLExecuted"] as string;
            ExtractedType = r["ExtractedType"] as string;
            ExtractedId = r["ExtractedId"] is DBNull ? 0 : Convert.ToInt32(r["ExtractedId"]);
        }
        
        public void CompleteAudit(string destinationDescription, int distinctIdentifiers)
        {
            DestinationDescription = destinationDescription;
            RecordsExtracted = distinctIdentifiers;

            SaveToDatabase();
        }
    }
}