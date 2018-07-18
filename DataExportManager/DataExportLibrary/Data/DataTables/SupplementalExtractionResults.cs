using System;
using System.Collections.Generic;
using System.Data.Common;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Repositories;
using MapsDirectlyToDatabaseTable;

namespace DataExportLibrary.Data.DataTables
{
    /// <summary>
    /// Describes the extraction status of a supplemental file/table which was bundled along with the normal datasets being extracted.  This could
    /// be lookup tables, pdf documents, etc.
    /// </summary>
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
        private string _repositoryType;
        private string _extractedName;
        private string _destinationType;

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
        public string ExtractedName
        {
            get { return _extractedName; }
            set { SetField(ref _extractedName, value); }
        }

        public string RepositoryType
        {
            get { return _repositoryType; }
            set { SetField(ref _repositoryType, value); }
        }

        public int ExtractedId
        {
            get { return _extractedId; }
            set { SetField(ref _extractedId, value); }
        }

        public string DestinationType
        {
            get { return _destinationType; }
            private set { SetField(ref _destinationType, value); }
        }

        #endregion

        [NoMappingToDatabase]
        public bool IsGlobal { get; private set; }

        public SupplementalExtractionResults(IRepository repository, IExtractionConfiguration configuration, string sql, IMapsDirectlyToDatabaseTable extractedObject)
        {
            Repository = repository;
            string name = extractedObject.GetType().FullName;

            if (extractedObject is INamed)
                name = (extractedObject as INamed).Name;

            Repository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                {"ExtractionConfiguration_ID", configuration.ID},
                {"SQLExecuted", sql},
                {"ExtractedType", extractedObject.GetType().FullName },
                {"ExtractedId", extractedObject.ID },
                {"ExtractedName", name },
                {"RepositoryType", extractedObject.Repository.GetType().FullName }
            });

            IsGlobal = true;
        }

        public SupplementalExtractionResults(IRepository repository, ICumulativeExtractionResults configuration, string sql, IMapsDirectlyToDatabaseTable extractedObject)
        {
            Repository = repository;
            string name = extractedObject.GetType().FullName;

            if (extractedObject is INamed)
                name = (extractedObject as INamed).Name;

            Repository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                {"CumulativeExtractionResults_ID", configuration.ID},
                {"SQLExecuted", sql},
                {"ExtractedType", extractedObject.GetType().FullName },
                {"ExtractedId", extractedObject.ID },
                {"ExtractedName", name },
                {"RepositoryType", extractedObject.Repository.GetType().FullName }
            });

            IsGlobal = false;
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
            ExtractedName = r["ExtractedName"] as string;
            RepositoryType = r["RepositoryType"] as string;
            DestinationType = r["DestinationType"] as string; 

            IsGlobal = CumulativeExtractionResults_ID == null && ExtractionConfiguration_ID != null;
        }

        public Type GetExtractedType()
        {
            return ((DataExportRepository)Repository).CatalogueRepository.MEF.GetTypeByNameFromAnyLoadedAssembly(ExtractedType);
        }

        public Type GetDestinationType()
        {
            return ((DataExportRepository)Repository).CatalogueRepository.MEF.GetTypeByNameFromAnyLoadedAssembly(DestinationType);
        }

        public void CompleteAudit(Type destinationType, string destinationDescription, int distinctIdentifiers)
        {
            DestinationType = destinationType.FullName;
            DestinationDescription = destinationDescription;
            RecordsExtracted = distinctIdentifiers;

            SaveToDatabase();
        }

        

        public override string ToString()
        {
            return ExtractedName;
        }
    }
}