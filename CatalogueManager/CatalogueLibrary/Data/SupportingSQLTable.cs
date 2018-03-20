using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Revertable;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Describes an SQL query that can be run to generate useful information for the understanding of a given Catalogue (dataset).  If it is marked as 
    /// Extractable then it will be bundled along with the Catalogue every time it is extracted.  This can be used as an alternative to definining Lookups
    /// through the Lookup class or to extract other useful administrative data etc to be provided to researchers
    /// 
    /// It is VITAL that you do not use this as a method of extracting sensitive/patient data as this data is run as is and is not joined against a cohort
    /// or anonymised in anyway.
    /// 
    /// If the Global flag is set then the SQL will be run and the result provided to every researcher regardless of what datasets they have asked for in 
    /// an extraction, this is useful for large lookups like ICD / SNOMED CT which are likely to be used by many datasets. 
    /// </summary>
    public class SupportingSQLTable : VersionedDatabaseEntity,INamed
    {
        public const string ExtractionFolderName = "SupportingDataTables";

        #region Database Properties
        private int _catalogue_ID;
        private string _description;
        private string _name;
        private string _sQL;
        private bool _extractable;
        private int? _externalDatabaseServer_ID;
        private string _ticket;
        private bool _isGlobal;

        public int Catalogue_ID
        {
            get { return _catalogue_ID; }
            set { SetField(ref _catalogue_ID, value); }
        }
        public string Description
        {
            get { return _description; }
            set { SetField(ref _description, value); }
        }
        public string Name
        {
            get { return _name; }
            set { SetField(ref _name, value); }
        }
        public string SQL
        {
            get { return _sQL; }
            set { SetField(ref _sQL, value); }
        }
        public bool Extractable
        {
            get { return _extractable; }
            set { SetField(ref _extractable, value); }
        }
        public int? ExternalDatabaseServer_ID
        {
            get { return _externalDatabaseServer_ID; }
            set { SetField(ref _externalDatabaseServer_ID, value); }
        }
        public string Ticket
        {
            get { return _ticket; }
            set { SetField(ref _ticket, value); }
        }
        public bool IsGlobal
        {
            get { return _isGlobal; }
            set { SetField(ref _isGlobal, value); }
        }

        #endregion

        #region Relationships
        [NoMappingToDatabase]
        public Catalogue Catalogue
        {
            get { return Repository.GetObjectByID<Catalogue>(Catalogue_ID); }
        }

        [NoMappingToDatabase]
        public ExternalDatabaseServer ExternalDatabaseServer {
            get { return ExternalDatabaseServer_ID == null ? null : Repository.GetObjectByID<ExternalDatabaseServer>((int)ExternalDatabaseServer_ID); }
        }

        #endregion

        public SupportingSQLTable(ICatalogueRepository repository, Catalogue parent, string name)
        {
            repository.InsertAndHydrate(this,new Dictionary<string, object>
            {
                {"Name", name},
                {"Catalogue_ID", parent.ID}
            });
        }

        internal SupportingSQLTable(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            Catalogue_ID = int.Parse(r["Catalogue_ID"].ToString());
            Description = r["Description"] as string;
            Name = r["Name"] as string;
            Extractable = (bool) r["Extractable"];
            IsGlobal = (bool) r["IsGlobal"];
            SQL = r["SQL"] as string;

            if(r["ExternalDatabaseServer_ID"] == null || r["ExternalDatabaseServer_ID"] == DBNull.Value)
                ExternalDatabaseServer_ID = null; 
            else
                ExternalDatabaseServer_ID = Convert.ToInt32(r["ExternalDatabaseServer_ID"]); 

            Ticket = r["Ticket"] as string;
        }

        public override string ToString()
        {
            return Name;
        }
        
        /// <summary>
        /// Returns the decrypted connection string you can use to access the data (fetched from ExternalDatabaseServer_ID - which can be null).  If there is no 
        /// ExternalDatabaseServer_ID associated with the SupportingSQLTable then a NotSupportedException will be thrown
        /// </summary>
        /// <returns></returns>
        public DiscoveredServer GetServer()
        {
            if (ExternalDatabaseServer_ID == null)
                throw new NotSupportedException("No external database server has been selected for SupportingSQL table called :" + ToString() + " (ID=" + ID + ").  The SupportingSQLTable currently belongs to Catalogue " + Catalogue.Name);
            
            //do not require an explicit database
            return DataAccessPortal.GetInstance().ExpectServer(ExternalDatabaseServer, DataAccessContext.DataExport, false);
        }
    }

    public enum FetchOptions
    {
        AllGlobalsAndAllLocals,
        AllGlobals,
        AllLocals,

        ExtractableGlobals,
        ExtractableLocals,
        ExtractableGlobalsAndLocals
    }
}