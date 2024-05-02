// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.Providers.Nodes;

/// <summary>
///     Collection of all <see cref="ExternalDatabaseServer" /> objects.  These are servers that RDMP knows about and can
///     connect to.  These are
///     distinct from the server attributes of <see cref="TableInfo" /> and may not even be database servers (e.g. they
///     could be FTP server or
///     a ticketing server etc).
/// </summary>
public class AllExternalServersNode : SingletonNode
{
    public AllExternalServersNode() : base("External Servers (Including Platform Databases)")
    {
    }
}