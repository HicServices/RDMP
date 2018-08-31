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
    public class IssueSystemUser : VersionedDatabaseEntity,INamed
    {
        #region Database Properties
        private string _name;
        private string _emailAddress;

        /// <inheritdoc/>
        public string Name
        {
            get { return _name; }
            set { SetField(ref _name, value); }
        }

        /// <summary>
        /// Users email address
        /// </summary>
        public string EmailAddress
        {
            get { return _emailAddress; }
            set { SetField(ref _emailAddress, value); }
        }

        #endregion

        /// <summary>
        /// Declares a new user of the issue tracking system
        /// </summary>
        /// <param name="repository"></param>
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

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }

    }
}
