using System;
using System.Collections.Generic;
using System.Data.Common;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// A person who can be identified as the reporter, creator or owner of a CatalogueItemIssue.
    /// </summary>
    public class IssueSystemUser : VersionedDatabaseEntity
    {
        #region Database Properties
        private string _name;
        private string _emailAddress;

        public string Name
        {
            get { return _name; }
            set { SetField(ref _name, value); }
        }
        public string EmailAddress
        {
            get { return _emailAddress; }
            set { SetField(ref _emailAddress, value); }
        }

        #endregion

        public IssueSystemUser(ICatalogueRepository repository)
        {
            repository.InsertAndHydrate(this,new Dictionary<string, object>
            {
                {"Name", "NewUser" + Guid.NewGuid()}
            });
        }

        internal IssueSystemUser(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            Name = r["Name"] as string;
            EmailAddress = r["EmailAddress"] as string;
        }

        public override string ToString()
        {
            return Name;
        }

    }
}
