// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CatalogueLibrary.DataHelper;

namespace Rdmp.Core.CatalogueLibrary.Data.EntityNaming
{
    /// <summary>
    /// Used when there is a single staging database used for multiple different Catalogues. The name of the database being loaded is prepended to the staging table name.
    /// </summary>
    public class FixedStagingDatabaseNamer : SuffixBasedNamer
    {
        private readonly string _stagingDatabaseName;
        private readonly string _databaseName;

        /// <summary>
        /// <para>---</para>
        /// <para>For 'Staging', returns the table name prefixed with <paramref name="databaseName"/> and suffixed with _STAGING</para>
        /// <para>---</para>
        /// <para>For others, appends:</para>
        /// <para>_Archive for Archive</para>
        /// </summary>
        public FixedStagingDatabaseNamer(string databaseName, string stagingDatabaseName = "DLE_STAGING")
        {
            _databaseName = RDMPQuerySyntaxHelper.EnsureValueIsNotWrapped(databaseName);
            _stagingDatabaseName = RDMPQuerySyntaxHelper.EnsureValueIsNotWrapped(stagingDatabaseName);
        }

        /// <inheritdoc/>
        public override string GetName(string tableName, LoadBubble convention)
        {
            if (convention == LoadBubble.Staging)
                return _databaseName + "_" + tableName + Suffixes[convention];

            return base.GetName(tableName, convention);
        }
        
        /// <inheritdoc/>
        public override string GetDatabaseName(string rootDatabaseName, LoadBubble stage)
        {
            if (stage == LoadBubble.Staging)
                return _stagingDatabaseName;

            return base.GetDatabaseName(rootDatabaseName, stage);
        }
    }
}