// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Discovery;

namespace Rdmp.UI.DataLoadUIs.LoadMetadataUIs.LoadDiagram.StateDiscovery;

/// <summary>
/// Depicts a table which was found in the loading tables of a DLE load.  These tables are unexpected (i.e. not created by RDMP).  They may be
/// temporary tables created as part of load scripts or they may reflect other ongoing/crashed loads (if in STAGING).
/// </summary>
public class UnplannedTable : IHasLoadDiagramState
{
    public DiscoveredTable Table { get; private set; }
    public readonly DiscoveredColumn[] Columns;
    public LoadDiagramState State => LoadDiagramState.New;

    public UnplannedTable(DiscoveredTable table)
    {
        Table = table;
        Columns = table.DiscoverColumns();
    }

    public override string ToString() => Table.GetRuntimeName();
}