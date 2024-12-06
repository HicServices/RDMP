// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.


namespace Rdmp.Core.DataLoad.Modules.Attachers;

/// <summary>
/// Data load component for loading tables with records read from a remote database server.  Runs the specified query (which can include a date parameter)
/// and inserts the results of the query into RAW.
/// This attcher does not create RAW if it does not exist. Another attacher will be required to generate the initial RAW database
/// </summary>
public class RemoteTableWithoutDBCreationAttacher: RemoteTableAttacher
{

    public RemoteTableWithoutDBCreationAttacher() : base(false) { }
}
