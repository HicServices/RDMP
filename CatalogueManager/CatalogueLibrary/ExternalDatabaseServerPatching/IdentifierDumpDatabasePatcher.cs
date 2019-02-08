// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Reflection;

namespace CatalogueLibrary.ExternalDatabaseServerPatching
{
    /// <summary>
    /// Documents the relationship between the patching and database assemblies of the Identifier Dump Database (this db stores identifiable data that is 
    /// dropped out of loads in the DLE i.e. dilution and column dropping to produce a semi anonymous dataset for data analysts to work on).
    /// </summary>
    public class IdentifierDumpDatabasePatcher : IPatcher
    {
        /// <inheritdoc/>
        public Assembly GetHostAssembly()
        {
            return Assembly.Load("IdentifierDump");
        }

        /// <inheritdoc/>
        public Assembly GetDbAssembly()
        {
            return Assembly.Load("IdentifierDump.Database");
        }
    }
}