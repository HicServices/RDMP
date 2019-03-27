using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Repositories.Managers;
using DataExportLibrary.Data.DataTables;

namespace DataExportLibrary.Repositories
{
    /// <summary>
    /// Memory only implementation of <see cref="IDataExportRepository"/>.  Also implements <see cref="ICatalogueRepository"/>.  All objects are created in 
    /// dictionaries and arrays in memory instead of the database.
    /// </summary>
    public class MemoryDataExportRepository : MemoryCatalogueRepository,IDataExportRepository, IDataExportPropertyManager, IExtractableDataSetPackageManager
    {
        public ICatalogueRepository CatalogueRepository { get { return this; } }
        public IDataExportPropertyManager DataExportPropertyManager { get { return this; } }
        public IExtractableDataSetPackageManager PackageManager { get { return this; } }
         

        public CatalogueExtractabilityStatus GetExtractabilityStatus(ICatalogue c)
        {
            var eds = GetAllObjectsWithParent<ExtractableDataSet>(c).SingleOrDefault();

            if(eds == null)
                return new CatalogueExtractabilityStatus(false,false);

            return new CatalogueExtractabilityStatus(true, eds.Project_ID != null);
        }

        public ISelectedDataSets[] GetSelectedDatasetsWithNoExtractionIdentifiers()
        {
            var col = GetAllObjects<ExtractableColumn>().Where(ec => ec.IsExtractionIdentifier).ToArray();

            return GetAllObjects<ISelectedDataSets>().Where(sds => col.All(c => c.ExtractableDataSet_ID != sds.ExtractableDataSet_ID)).ToArray();
        }

        IExtractableDataSetPackageManager IDataExportRepository.PackageManager { get; set; }

        #region IDataExportPropertyManager

        Dictionary<DataExportProperty,string>  _propertiesDictionary = new Dictionary<DataExportProperty, string>();
        
        public string GetValue(DataExportProperty property)
        {
            if (_propertiesDictionary.ContainsKey(property))
                return _propertiesDictionary[property];

            return null;
        }

        public void SetValue(DataExportProperty property, string value)
        {
            if (!_propertiesDictionary.ContainsKey(property))
                _propertiesDictionary.Add(property,value);
            else
                _propertiesDictionary[property] = value;
        }
        #endregion

        

        #region IExtractableDataSetPackageManager

        readonly Dictionary<IExtractableDataSetPackage,HashSet<IExtractableDataSet>> _packageDictionary = new Dictionary<IExtractableDataSetPackage, HashSet<IExtractableDataSet>>();

        public IExtractableDataSet[] GetAllDataSets(IExtractableDataSetPackage package, IExtractableDataSet[] allDataSets)
        {
            if (!_packageDictionary.ContainsKey(package))
                _packageDictionary.Add(package, new HashSet<IExtractableDataSet>());

            return _packageDictionary[package].ToArray();
        }

        public void AddDataSetToPackage(IExtractableDataSetPackage package, IExtractableDataSet dataSet)
        {
            if(!_packageDictionary.ContainsKey(package))
                _packageDictionary.Add(package,new HashSet<IExtractableDataSet>());

            _packageDictionary[package].Add(dataSet);
        }

        public void RemoveDataSetFromPackage(IExtractableDataSetPackage package, IExtractableDataSet dataSet)
        {
            if (!_packageDictionary.ContainsKey(package))
                _packageDictionary.Add(package, new HashSet<IExtractableDataSet>());

            _packageDictionary[package].Remove(dataSet);
        }
        #endregion
    }
}