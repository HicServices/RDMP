using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Each Catalogue database can have 0 or 1 TicketingSystemConfiguration, this is a pointer to a plugin that handles communicating with a ticketing/issue system
    /// such as JIRA.  This ticketing system is used to record ticket numbers of a variety of objects (e.g. SupportingDocuments, extraction projects etc) and allows them
    /// to accrue man hours without compromising your current workflow.
    /// 
    /// In addition to tying objects to your ticketing system, the ticketing system will also be consulted about wheter data extraction projects are good to go or should
    /// not be released (e.g. do not release project X until it has been paid for / signed off by the governancer).  The exact implementation of this is mostly left to the
    /// ticketing class you write.
    /// 
    /// The Type field refers to a class that implements PluginTicketingSystem (see LoadModuleAssembly for how to write your own handler or use one of the compatible existing ones).  
    /// this class will handle all communication with the ticketing system/server.
    ///
    /// There is also a reference to DataAccessCredentials record which stores optional username and encrypted password to use in the plugin for communicating with the ticketing system.
    /// 
    /// </summary>
    public class TicketingSystemConfiguration : DatabaseEntity
    {
        #region Database Properties
        private bool _isActive;
        private string _url;
        private string _type;
        private string _name;
        private int? _dataAccessCredentials_ID;

        public bool IsActive
        {
            get { return _isActive; }
            set { SetField(ref _isActive, value); }
        }
        public string Url
        {
            get { return _url; }
            set { SetField(ref _url, value); }
        }
        public string Type
        {
            get { return _type; }
            set { SetField(ref _type, value); }
        }
        public string Name
        {
            get { return _name; }
            set { SetField(ref _name, value); }
        }
        public int? DataAccessCredentials_ID
        {
            get { return _dataAccessCredentials_ID; }
            set { SetField(ref _dataAccessCredentials_ID, value); }
        }

        #endregion

        #region Relationships
        [NoMappingToDatabase]
        public DataAccessCredentials DataAccessCredentials { get
        {
            return DataAccessCredentials_ID == null
                ? null
                : Repository.GetObjectByID<DataAccessCredentials>((int) DataAccessCredentials_ID);
        }}
        #endregion

        public TicketingSystemConfiguration(ICatalogueRepository repository, string name)
        {
            repository.InsertAndHydrate(this,new Dictionary<string, object>
            {
                {"Name", name != null ? (object) name : DBNull.Value},
                {"IsActive", 1}
            });
        }

        internal TicketingSystemConfiguration(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            IsActive = (bool) r["IsActive"];
            Url = r["Url"] as string;
            Type = r["Type"] as string;
            Name = r["Name"] as string;
            DataAccessCredentials_ID = ObjectToNullableInt(r["DataAccessCredentials_ID"]);
        }
    }
}