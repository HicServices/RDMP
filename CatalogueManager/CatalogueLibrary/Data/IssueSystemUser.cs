// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Attributes;
using ReusableLibraryCode;
using ReusableLibraryCode.Annotations;

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
        [Unique]
        [NotNull]
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
