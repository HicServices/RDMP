// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;

namespace Rdmp.Core.Repositories.Managers;

/// <summary>
///     Helper class for listing, adding and removing <see cref="IExtractableDataSet" /> from
///     <see cref="IExtractableDataSetPackage" />s
/// </summary>
public interface IExtractableDataSetPackageManager
{
    /// <summary>
    ///     Returns the subset of <paramref name="allDataSets" /> which are part of the <paramref name="package" />.
    /// </summary>
    /// <param name="package"></param>
    /// <param name="allDataSets"></param>
    /// <returns></returns>
    IExtractableDataSet[] GetAllDataSets(IExtractableDataSetPackage package, IExtractableDataSet[] allDataSets);

    Dictionary<int, List<int>> GetPackageContentsDictionary();

    /// <summary>
    ///     Adds the given <paramref name="dataSet" /> to the <paramref name="package" />
    /// </summary>
    /// <param name="package"></param>
    /// <param name="dataSet"></param>
    void AddDataSetToPackage(IExtractableDataSetPackage package, IExtractableDataSet dataSet);

    /// <summary>
    ///     Removes the given <paramref name="dataSet" /> from the <paramref name="package" /> and updates the cached package
    ///     contents
    ///     in memory.
    /// </summary>
    /// <param name="package"></param>
    /// <param name="dataSet"></param>
    void RemoveDataSetFromPackage(IExtractableDataSetPackage package, IExtractableDataSet dataSet);
}