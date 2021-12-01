using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Rdmp.Core.DataExport.Data
{
    /// <summary>
    /// Records how far through a batch extraction a <see cref="SelectedDataSets"/> is.  Also tracks which column is being
    /// used for the batch splitting.
    /// </summary>
    public class ExtractionProgress : DatabaseEntity
    {
        #region Database Properties

        private int _selectedDataSets_ID;
        private DateTime? _progress;
        private int _extractionInformation_ID;
        #endregion

        public int SelectedDataSets_ID
        {
            get { return _selectedDataSets_ID; }
            set { SetField(ref _selectedDataSets_ID, value); }
        }
        public DateTime? ProgressDate
        {
            get { return _progress; }
            set { SetField(ref _progress, value); }
        }
        public int ExtractionInformation_ID
        {
            get { return _extractionInformation_ID; }
            set { SetField(ref _extractionInformation_ID, value); }
        }
        public ExtractionProgress(IDataExportRepository repository, SelectedDataSets sds)
        {
            var cata = sds.GetCatalogue();
            var coverageColId = cata?.TimeCoverage_ExtractionInformation_ID;

            if(!coverageColId.HasValue)
            {
                throw new ArgumentException($"Cannot create ExtractionProgress because Catalogue {cata} does not have a time coverage column");
            }

            repository.InsertAndHydrate(this, new Dictionary<string, object>()
            {
                { "SelectedDataSets_ID",sds.ID},
                { "ExtractionInformation_ID",coverageColId}
            });

            if (ID == 0 || Repository != repository)
                throw new ArgumentException("Repository failed to properly hydrate this class");
        }
        public ExtractionProgress(IDataExportRepository repository, DbDataReader r) : base(repository, r)
        {
            SelectedDataSets_ID = Convert.ToInt32(r["SelectedDataSets_ID"]);
            ProgressDate = ObjectToNullableDateTime(r["ProgressDate"]);
            ExtractionInformation_ID = Convert.ToInt32(r["ExtractionInformation_ID"]);
        }
    }
}
