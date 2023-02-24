// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Repositories.Managers;

namespace Rdmp.Core.Providers.Nodes;

/// <summary>
/// Collection of all datasets in a given <see cref="Package"/>.  This lets you define template sets of datasets which all get extracted together
/// e.g. 'Core Datasets'.
/// </summary>
public class PackageContentNode:Node,IDeletableWithCustomMessage, IMasqueradeAs
{
    private readonly IExtractableDataSetPackageManager _contents;
    public IExtractableDataSetPackage Package { get; set; }
    public IExtractableDataSet DataSet { get; set; }

    public PackageContentNode(IExtractableDataSetPackage package, IExtractableDataSet dataSet, IExtractableDataSetPackageManager contents)
    {
        _contents = contents;
        Package = package;
        DataSet = dataSet;
    }

    public override string ToString()
    {
        return DataSet.ToString();
    }
        
    protected bool Equals(PackageContentNode other)
    {
        return Equals(Package, other.Package) && Equals(DataSet, other.DataSet);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((PackageContentNode) obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return ((Package != null ? Package.GetHashCode() : 0)*397) ^ (DataSet != null ? DataSet.GetHashCode() : 0);
        }
    }

    public void DeleteInDatabase()
    {
        _contents.RemoveDataSetFromPackage(Package, DataSet);
    }

    public object MasqueradingAs()
    {
        return DataSet;
    }

    /// <inheritdoc/>
    public string GetDeleteMessage()
    {
        return $"remove '{DataSet}' from Pacakge";
    }

    /// <inheritdoc/>
    public string GetDeleteVerb()
    {
        return "Remove";
    }
}