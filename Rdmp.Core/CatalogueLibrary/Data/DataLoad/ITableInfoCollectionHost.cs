// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;

namespace Rdmp.Core.CatalogueLibrary.Data.DataLoad
{
    /// <summary>
    /// Interface for defining that a given class is dependent or operates on one or more TableInfos.  This is used when you declare a [DemandsInitialization] property on
    /// a plugin component of type ITableInfo to determine which instances to offer at design time (i.e. only show TableInfos that are associated with the load you are
    /// editing).
    /// </summary>
    public interface ITableInfoCollectionHost
    {
        /// <summary>
        /// Gets all <see cref="TableInfo"/> that are part of the load
        /// </summary>
        /// <returns></returns>
        IEnumerable<TableInfo> GetTableInfos();
    }
}