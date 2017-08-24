using System;
using System.Collections.Generic;
using System.Data.Common;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Interfaces.Data.DataTables;
using MapsDirectlyToDatabaseTable;

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
