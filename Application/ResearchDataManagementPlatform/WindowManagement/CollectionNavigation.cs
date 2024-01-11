// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Discovery;
using Rdmp.Core.CommandExecution;
using Equ;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace ResearchDataManagementPlatform.WindowManagement;

/// <summary>
/// Records the fact that the user visited a specific object in a tree collection
/// </summary>
public sealed class CollectionNavigation : PropertywiseEquatable<CollectionNavigation>, INavigation
{
    public IMapsDirectlyToDatabaseTable Object { get; }

    [MemberwiseEqualityIgnore] public bool IsAlive => Object is not IMightNotExist o || o.Exists();

    public CollectionNavigation(IMapsDirectlyToDatabaseTable @object)
    {
        Object = @object;
    }

    public void Activate(ActivateItems activateItems)
    {
        activateItems.RequestItemEmphasis(this, new EmphasiseRequest(Object, 0));
    }

    public void Close()
    {
    }

    public override string ToString() => Object.ToString();
}