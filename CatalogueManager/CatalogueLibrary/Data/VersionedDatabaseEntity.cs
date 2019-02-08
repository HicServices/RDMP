// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Data.Common;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// See DatabaseEntity
    /// </summary>
    public abstract class VersionedDatabaseEntity : DatabaseEntity
    {
        #region Database Properties

        private string _softwareVersion;

        /// <summary>
        /// The version of RDMP that was running when the object was created
        /// </summary>
        [DoNotExtractProperty]
        public string SoftwareVersion
        {
            get { return _softwareVersion; }
            set { SetField(ref  _softwareVersion, value); }
        }

        #endregion
        
        /// <inheritdoc/>
        protected VersionedDatabaseEntity():base()
        {

        }

        /// <inheritdoc/>
        protected VersionedDatabaseEntity(IRepository repository, DbDataReader r):base(repository,r)
        {
            SoftwareVersion = r["SoftwareVersion"].ToString();
        }

    }
}