using System.Collections.Generic;
using System.Data.Common;
using CatalogueLibrary.Checks.SyntaxChecking;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.Checks;

namespace DataExportLibrary.Data.DataTables
{
    /// <summary>
    /// Data Export Manager supports cohort custom data, these are data tables that contain information relevant to a cohort of patients but is not a Catalogue itself.  Usually this
    /// means the data is bespoke project data e.g. questionnaire answers for a cohort etc.  These data tables are extracted along with all the regular data (from Catalogues etc)
    /// in extracts from Data Export Manager.  In addition to this functionality you can use the columns in any of your custom cohort data tables in extracts of any regular Catalogue.
    /// For example imagine you have a custom data set which is 'Patient ID,Date Consented' then you could configure an extraction filters that only extracted records from Prescribing,
    /// Demography, Biochemistry catalogues AFTER each patients consent date.
    /// 
    /// <para>CohortCustomColumns are how Data Export Manager tracks the columns in these data tables for linking in extraction configurations and use in filters.  The columns are synced every
    /// time you edit your configuration.</para>
    /// </summary>
    public class CohortCustomColumn : VersionedDatabaseEntity, IColumn
    {
        #region Database Properties
        private string _selectSQL;
        private int _extractableCohort_ID;

        [Sql]
        public string SelectSQL
        {
            get { return _selectSQL; }
            set { SetField(ref _selectSQL, value); }
        }
        public int ExtractableCohort_ID
        {
            get { return _extractableCohort_ID; }
            set { SetField(ref _extractableCohort_ID, value); }
        }
        #endregion

        #region Relationships
        [NoMappingToDatabase]
        public ColumnInfo ColumnInfo
        {
            get { return null; }
        }
        #endregion

        [NoMappingToDatabase]
        public int Order { get; set; }

        [NoMappingToDatabase]
        public string Alias {
            get { return null; }
        }
        [NoMappingToDatabase]
        public bool HashOnDataRelease {
            get { return false; }
        }
        [NoMappingToDatabase]
        public bool IsExtractionIdentifier 
        {
            get { return false; }
        }
        [NoMappingToDatabase]
        public bool IsPrimaryKey { get { return false; } }

        internal CohortCustomColumn(IDataExportRepository repository, DbDataReader r)
            : base(repository, r)
        {
            SelectSQL = r["SelectSQL"] as string;
            ExtractableCohort_ID = (int)r["ExtractableCohort_ID"];
        }

        public CohortCustomColumn(IDataExportRepository repository, int extractableCohortID, string selectSQL)
        {
            Repository = repository;
            
            Repository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                {"ExtractableCohort_ID", extractableCohortID},
                {"SelectSQL", selectSQL}
            });
        }

        public override string ToString()
        {
            return SelectSQL;
        }

        public void Check(ICheckNotifier notifier)
        {
            new ColumnSyntaxChecker(this).Check(notifier);
        }

        public string GetRuntimeName()
        {
          return RDMPQuerySyntaxHelper.GetRuntimeName(this);
        }
    }
}
