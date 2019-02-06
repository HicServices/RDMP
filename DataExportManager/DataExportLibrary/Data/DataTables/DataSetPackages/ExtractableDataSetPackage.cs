// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Interfaces.Data.DataTables;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Attributes;
using ReusableLibraryCode.Annotations;

namespace DataExportLibrary.Data.DataTables.DataSetPackages
{
    /// <summary>
    /// A collection of ExtractableDataSet which share a common concept e.g. 'Core datasets', 'Supplemental Datasets', 'Diabetes datasets' etc. These allow you to add a collection of 
    /// datasets to a project extraction in one go and to standardise who gets what datasets.
    /// </summary>
    public class ExtractableDataSetPackage:DatabaseEntity,INamed
    {
        #region Database Properties
        private string _name;
        private string _creator;
        private DateTime _creationDate;

        [NotNull]
        [Unique]
        public string Name
        {
            get { return _name; }
            set { SetField(ref _name, value); }
        }
        public string Creator
        {
            get { return _creator; }
            set { SetField(ref _creator, value); }
        }
        public DateTime CreationDate
        {
            get { return _creationDate; }
            set { SetField(ref _creationDate, value); }
        }

        #endregion


        public ExtractableDataSetPackage(IDataExportRepository dataExportRepository, DbDataReader r)
            : base(dataExportRepository, r)
        {
            Name = r["Name"].ToString();
            Creator = r["Creator"].ToString();
            CreationDate = Convert.ToDateTime(r["CreationDate"]);
        }

        public ExtractableDataSetPackage(IDataExportRepository dataExportRepository, string name)
        {
            dataExportRepository.InsertAndHydrate(this,new Dictionary<string, object>()
            {
                {"Name",name},
                {"Creator",Environment.UserName},
                {"CreationDate",DateTime.Now }
            });
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
