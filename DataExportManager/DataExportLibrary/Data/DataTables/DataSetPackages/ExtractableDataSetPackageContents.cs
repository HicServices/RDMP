// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Repositories;

namespace DataExportLibrary.Data.DataTables.DataSetPackages
{
    /// <summary>
    /// Helper class for listing, adding and dropping ExtractableDataSets from ExtractableDataSetPackages (See ExtractableDataSetPackage)
    /// </summary>
    public class ExtractableDataSetPackageContents
    {
        private readonly IDataExportRepository _repository;
        Dictionary<int,List<int>> _packageContentsDictionary = new Dictionary<int, List<int>>();

        /// <summary>
        /// Creates a new <see cref="ExtractableDataSetPackageContents"/> which can read the contents of <see cref="ExtractableDataSetPackage"/>s i.e.
        /// which datasets are part of which packages (many to many relationship).
        /// 
        /// <para>The contents of packages are fetched during construction only and are not sensitive to remote changes during thereafter (e.g. by other users)</para>
        /// </summary>
        /// <param name="repository"></param>
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

        /// <summary>
        /// Returns the subset of <paramref name="allDataSets"/> which are part of the <paramref name="package"/>.  This does
        /// not require going to the database as package contents are cached during construction.
        /// </summary>
        /// <param name="package"></param>
        /// <param name="allDataSets"></param>
        /// <returns></returns>
        public ExtractableDataSet[] GetAllDataSets(ExtractableDataSetPackage package, ExtractableDataSet[] allDataSets)
        {
            //we know of no children
            if (!_packageContentsDictionary.ContainsKey(package.ID))
                return new ExtractableDataSet[0];
            
            return _packageContentsDictionary[package.ID].Select(i => allDataSets.Single(ds => ds.ID == i)).ToArray();
        }

        /// <summary>
        /// Adds the given <paramref name="dataSet"/> to the <paramref name="package"/> and updates the cached package contents 
        /// in memory.  
        /// 
        /// <para>This change is immediately written to the database</para>
        ///
        ///  <para>Throws ArgumentException if the <paramref name="dataSet"/> is already part of the package</para>
        /// </summary>
        /// <param name="package"></param>
        /// <param name="dataSet"></param>
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

        /// <summary>
        /// Removes the given <paramref name="dataSet"/> from the <paramref name="package"/> and updates the cached package contents 
        /// in memory.  
        /// 
        /// <para>This change is immediately written to the database</para>
        ///
        ///  <para>Throws ArgumentException if the <paramref name="dataSet"/> is not part of the package</para>
        /// </summary>
        /// <param name="package"></param>
        /// <param name="dataSet"></param>
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
