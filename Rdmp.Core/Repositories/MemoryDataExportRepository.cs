// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Repositories.Managers;

namespace Rdmp.Core.Repositories
{
    /// <summary>
    /// Memory only implementation of <see cref="IDataExportRepository"/>.  Also implements <see cref="ICatalogueRepository"/>.  All objects are created in 
    /// dictionaries and arrays in memory instead of the database.
    /// </summary>
    public class MemoryDataExportRepository : MemoryCatalogueRepository,IDataExportRepository, IDataExportPropertyManager, IExtractableDataSetPackageManager
    {
        public ICatalogueRepository CatalogueRepository { get { return this; } }
        public IDataExportPropertyManager DataExportPropertyManager { get { return this; } }
        public IExtractableDataSetPackageManager PackageManager { get { return this; }}
         

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

            return GetAllObjects<ISelectedDataSets>()
                        .Where(sds => !col.Any(c => c.ExtractableDataSet_ID == sds.ExtractableDataSet_ID
                                               && c.ExtractionConfiguration_ID == sds.ExtractionConfiguration_ID)).ToArray();
        }


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

        public Dictionary<int, List<int>> GetPackageContentsDictionary()
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<ICumulativeExtractionResults> GetAllCumulativeExtractionResultsFor(IExtractionConfiguration configuration, IExtractableDataSet dataset)
        {
            return GetAllObjects<CumulativeExtractionResults>().Where(e=>
            (e.ExtractionConfiguration_ID == configuration.ID) && (e.ExtractableDataSet_ID == dataset.ID));
        }
        #endregion
    }
}