using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using ADOX;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Injection;

namespace DataExportLibrary.Data.DataTables
{
    /// <summary>
    /// While the Catalogue Manager database includes support for marking which columns in which Catalogues are extractable (via ExtractionInformation) we need an additional layer
    /// in the Data Export Manager database.  This layer is the ExtractableDataSet object.  An ExtractableDataSet is 'the permission to perform extractions of a given Catalogue'.  We
    /// have this second layer for two main reasons.  The first is so that there is no cross database referential integrity problem for example if you delete a Catalogue 5 years after
    /// performing an extract we can still report to the user the facts in a graceful manner if they clone the old configuration.  The second reason is that you could (if you were crazy)
    /// have multiple DataExportManager databases all feeding off the same Catalogue database - e.g. one that does identifiable extracts and one which does anonymous extracts.  Some
    /// datasets (Catalogues) would therefore be extractable in one DataExportManager database while a different set would be extractable in the other DataExportManager database.
    /// </summary>
    public class ExtractableDataSet : VersionedDatabaseEntity, IExtractableDataSet, IInjectKnown<ICatalogue>
    {
        #region Database Properties
        private int _catalogue_ID;
        private bool _disableExtraction;
        private int? _project_ID;

        public int Catalogue_ID
        {
            get { return _catalogue_ID; }
            set
            {
                ClearAllInjections();
                SetField(ref _catalogue_ID, value);
            }
        }
        public bool DisableExtraction
        {
            get { return _disableExtraction; }
            set { SetField(ref _disableExtraction, value); }
        }


        /// <summary>
        /// Indicates that the referenced <see cref="Catalogue_ID"/> is associated only with one <see cref="Project"/> and should not be used outside of that.
        /// 
        /// <para>Data Export Manager supports Project only custom data, these are data tables that contain information relevant to a cohort of patients or specific Project only. 
        /// Usually this means the data is bespoke project data e.g. questionnaire answers for a cohort etc.  These data tables are treated exactly like regular Catalogues and 
        /// extracted in the same way as all the regular data.</para>
        /// 
        /// <para>In addition, you can use the columns in the referenced <see cref="Catalogue_ID"/> by joining them to any regular Catalogue being extracted in the Project.  These
        /// selected columns will be bolted on as additional columns.  You can also reference them in the WhereSQL of filters which will trigger an similar Join></para>.
        /// 
        /// <para>For example imagine you have a custom data set which is 'Patient ID,Date Consented' then you could configure an extraction filters that only extracted records from
        ///  Prescribing, Demography, Biochemistry catalogues AFTER each patients consent date.</para>
        /// </summary>
        public int? Project_ID
        {
            get { return _project_ID; }
            set { SetField(ref _project_ID, value); }
        }

        #endregion
        
        #region Relationships
        [NoMappingToDatabase]
        public IExtractionConfiguration[] ExtractionConfigurations
        {
            get
            {
                return Repository.SelectAllWhere<ExtractionConfiguration>(
                    "SELECT * FROM SelectedDataSets WHERE ExtractableDataSet_ID = @ExtractableDataSet_ID",
                    "ExtractionConfiguration_ID", new Dictionary<string, object>
                    {
                        {"ExtractableDataSet_ID", ID}
                    })
                    .Cast<IExtractionConfiguration>()
                    .ToArray();
            }
        }


        [NoMappingToDatabase]
        public ICatalogue Catalogue { get {return _catalogue.Value;}}
        #endregion

        /// <summary>
        /// Defines that the given Catalogue is extractable to researchers as a data set, this is stored in the DataExport database
        /// </summary>
        /// <returns></returns>
        public ExtractableDataSet(IDataExportRepository repository, Catalogue catalogue, int disableExtraction = 0)
        {
            Repository = repository;
            Repository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                {"DisableExtraction", disableExtraction},
                {"Catalogue_ID",catalogue.ID}
            });

            ClearAllInjections();
            InjectKnown(catalogue);
        }

        internal ExtractableDataSet(IDataExportRepository repository, DbDataReader r)
            : base(repository, r)
        {
            Catalogue_ID = Convert.ToInt32(r["Catalogue_ID"]);
            DisableExtraction = (bool) r["DisableExtraction"];
            Project_ID = ObjectToNullableInt(r["Project_ID"]);

            ClearAllInjections();
        }

        [NoMappingToDatabase]
        public bool IsCatalogueDeprecated
        {
            get
            {
                return Catalogue == null || Catalogue.IsDeprecated;
            }
        }
        
        public override string ToString()
        {
            if (Catalogue == null)
                return "DELETED CATALOGUE " + Catalogue_ID;

            //only bother refreshing Catalogue details if we will be able to get a legit catalogue name
            if (Catalogue.IsDeprecated)
                return "DEPRECATED CATALOGUE " + Catalogue.Name;

            return Catalogue.Name;
        }
        
        #region Stuff for updating our internal database records
        public override void DeleteInDatabase()
        {
            try
            {
                Repository.DeleteFromDatabase(this);
            }
            catch (Exception e)
            {
                if(e.Message.Contains("FK_SelectedDataSets_ExtractableDataSet"))
                    throw new Exception("Cannot delete " + this + " because it is in use by the following configurations :" +
                        Environment.NewLine + 
                        string.Join(Environment.NewLine, ExtractionConfigurations.Select(c=>c.Name +"(" + c.Project +")"), e));
                throw;
            }
        }
        #endregion

        public override void RevertToDatabaseState()
        {
            base.RevertToDatabaseState();
            //clear the cached knowledge
            ClearAllInjections();
        }

        public CatalogueExtractabilityStatus GetCatalogueExtractabilityStatus()
        {
            return new CatalogueExtractabilityStatus(true, Project_ID != null);
        }

        private Lazy<ICatalogue> _catalogue;
        
        public void InjectKnown(ICatalogue instance)
        {
            if(instance.ID != Catalogue_ID)
                throw new ArgumentOutOfRangeException("You told us our Catalogue was '" + instance +"' but it's ID didn't match so that is NOT our Catalogue","c");
            _catalogue = new Lazy<ICatalogue>(() => instance);
        }

        public void ClearAllInjections()
        {
            _catalogue = new Lazy<ICatalogue>(FetchCatalogue);
        }

        private ICatalogue FetchCatalogue()
        {
            try
            {
                var cata =  ((IDataExportRepository) Repository).CatalogueRepository.GetObjectByID<Catalogue>(Catalogue_ID);
                cata.InjectKnown(GetCatalogueExtractabilityStatus());
                return cata;
            }
            catch (KeyNotFoundException)
            {
                //Catalogue has been deleted!
                return null;
            }
        }
    }
}
