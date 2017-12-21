using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Interfaces.Data.DataTables;
using ReusableLibraryCode.Checks;

namespace DataExportLibrary.Data.DataTables.DataSetPackages
{
    /// <summary>
    /// Helper class for listing, adding and dropping ExtractableDataSets from ExtractableDataSetPackages (See ExtractableDataSetPackage)
    /// </summary>
    public class ExtractableDataSetPackageContents
    {
        private readonly IDataExportRepository _repository;
        Dictionary<int,List<int>> _packageContentsDictionary = new Dictionary<int, List<int>>();

        public ExtractableDataSetPackageContents(IDataExportRepository repository)
        {
            _repository = repository;
            using (var con = repository.GetConnection())
            {
                var r = repository.DiscoveredServer.GetCommand("SELECT * FROM ExtractableDataSetPackage_ExtractableDataSet ORDER BY ExtractableDataSetPackage_ID", con).ExecuteReader();

                var lastPackageId = -1;
                while (r.Read())
                {
                    var packageID = Convert.ToInt32(r["ExtractableDataSetPackage_ID"]);
                    var dataSetID = Convert.ToInt32(r["ExtractableDataSet_ID"]);

                    if (lastPackageId != packageID)
                    {
                        _packageContentsDictionary.Add(packageID,new List<int>());
                        lastPackageId = packageID;
                    }

                    _packageContentsDictionary[packageID].Add(dataSetID);
                }
            }
        }


        public ExtractableDataSet[] GetAllDataSets(ExtractableDataSetPackage package, ExtractableDataSet[] allDataSets)
        {
            //we know of no children
            if (!_packageContentsDictionary.ContainsKey(package.ID))
                return new ExtractableDataSet[0];
            
            return _packageContentsDictionary[package.ID].Select(i => allDataSets.Single(ds => ds.ID == i)).ToArray();
        }

        public void AddDataSetToPackage(ExtractableDataSetPackage package, ExtractableDataSet dataSet)
        {
            if (_packageContentsDictionary.ContainsKey(package.ID) && _packageContentsDictionary[package.ID].Contains(dataSet.ID))
                throw new ArgumentException("dataSet " + dataSet + " is already part of package '" + package + "'", "dataSet");

            using (var con = _repository.GetConnection())
            {
                _repository.DiscoveredServer.GetCommand(
                        "INSERT INTO ExtractableDataSetPackage_ExtractableDataSet(ExtractableDataSetPackage_ID,ExtractableDataSet_ID) VALUES ("+package.ID+"," +dataSet.ID+")",
                        con).ExecuteNonQuery();
            }
            
            if(!_packageContentsDictionary.ContainsKey(package.ID))
                _packageContentsDictionary.Add(package.ID,new List<int>());

            _packageContentsDictionary[package.ID].Add(dataSet.ID);
        }

        public void RemoveDataSetFromPackage(ExtractableDataSetPackage package, ExtractableDataSet dataSet)
        {
            if(!_packageContentsDictionary[package.ID].Contains(dataSet.ID))
                throw new ArgumentException("dataSet " + dataSet +" is not part of package " + package + " so cannot be removed","dataSet");

            using (var con = _repository.GetConnection())
            {
                _repository.DiscoveredServer.GetCommand(
                        "DELETE FROM ExtractableDataSetPackage_ExtractableDataSet WHERE ExtractableDataSetPackage_ID = " + package.ID + " AND ExtractableDataSet_ID ="+ dataSet.ID,
                        con).ExecuteNonQuery();
            }

            _packageContentsDictionary[package.ID].Remove(dataSet.ID);
        }


    }
}
