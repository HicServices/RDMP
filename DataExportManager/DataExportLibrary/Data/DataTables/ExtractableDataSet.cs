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

namespace DataExportLibrary.Data.DataTables
{
    /// <summary>
    /// While the Catalogue Manager database includes support for marking which columns in which Catalogues are extractable (via ExtractionInformation) we need an additional layer
    /// in the Data Export Manager database.  This layer is the ExtractableDataSet object.  An ExtractableDataSet is 'the permission to perform extractions of a given Catalogue'.  We
    /// have this second layer for two main reasons.  The first is so that there is no cross database referential integrity problem for example if you delete a Catalogue 5 years after
    /// performing an extract we can still report to the user the facts in a graceful manner if they clone the old configuration.  The second reason is that you could (if you were crazy)
    /// have multiple DataExportManager databases all feeding off the same Catalogue database - e.g. one that does identifiable extracts and one which does anonymous extracts.  Some
    /// datasets (Catalogues) would therefore be extractable in one DataExportManager database while a different set would be extractable in the other DataExportManager database.
    /// 
    /// </summary>
    public class ExtractableDataSet : VersionedDatabaseEntity, IExtractableDataSet
    {
        #region Database Properties
        private int _catalogue_ID;
        private bool _disableExtraction;

        public int Catalogue_ID
        {
            get { return _catalogue_ID; }
            set { SetField(ref _catalogue_ID, value); }
        }
        public bool DisableExtraction
        {
            get { return _disableExtraction; }
            set { SetField(ref _disableExtraction, value); }
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
        public ICatalogue Catalogue { get
        {
            return ((DataExportRepository) Repository).CatalogueRepository.GetObjectByID<Catalogue>(Catalogue_ID);
        }}
        #endregion

        /// <summary>
        /// Defines a new potentially extractable data set (based on a database query), this is stored in the DataExportManager2 database and the ID of
        /// the new record is returned by this method, use GetExtractableDataSetByID to get the object back from the database
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
        }

        public ExtractableDataSet(IDataExportRepository repository, DbDataReader r)
            : base(repository, r)
        {
            Catalogue_ID = Convert.ToInt32(r["Catalogue_ID"]);
            DisableExtraction = (bool) r["DisableExtraction"];
        }

        private Catalogue _catalogue;

        [NoMappingToDatabase]
        public bool IsCatalogueDeprecated
        {
            get
            {
                return _catalogue != null && _catalogue.IsDeprecated;
            }
        }


        public override string ToString()
        {
            //only bother refreshing Catalogue details if we will be able to get a legit catalogue name
            if (_catalogue == null && !_datasetBroken)
                RefreshCatalogueInfo();

            if (_catalogue == null)
                return "Catalogue Deleted (Catalogue_ID=" + Catalogue_ID+")";

            if (_catalogue.IsDeprecated)
                return "DEPRECATED CATALOGUE " + _catalogue.Name;
            
            return _catalogue.Name;
        }

        private bool _datasetBroken;

        private void RefreshCatalogueInfo()
        {
            if(Catalogue_ID != null)
                try
                {
                    _catalogue = ((DataExportRepository)Repository).CatalogueRepository.GetObjectByID<Catalogue>((int) Catalogue_ID);

                    if (_catalogue == null)
                        _datasetBroken = true;
                }
                catch (Exception e)
                {
                    if (e.Message.ToLower().Contains("could not find"))
                    {
                        _catalogue = null;
                        _datasetBroken = true;
                    }
                    else
                        throw;
                }
        }

        /// <summary>
        /// Only use if you have an existing reference to Catalogue_ID and want to stop the ToString method from popping off to the database
        /// </summary>
        /// <param name="c"></param>
        public void SetKnownCatalogue(Catalogue c)
        {
            if(c.ID != Catalogue_ID)
                throw new ArgumentOutOfRangeException("You told us our Catalogue was '" + c+"' but it's ID didn't match so that is NOT our Catalogue","c");
            
            _catalogue = c;
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
            _catalogue = null;
        }
    }
}