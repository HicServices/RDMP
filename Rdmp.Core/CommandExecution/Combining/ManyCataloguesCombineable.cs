// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.CommandExecution.Combining;

/// <summary>
///     <see cref="ICombineToMakeCommand" /> for a collection of type <see cref="Catalogue" />
/// </summary>
public class ManyCataloguesCombineable : ICombineToMakeCommand
{
    /// <summary>
    ///     The <see cref="Catalogue" /> that are being selected for combining (e.g. by dragging)
    /// </summary>
    public Catalogue[] Catalogues { get; set; }

    /// <summary>
    ///     Creates new instance populating <see cref="Catalogues" />
    /// </summary>
    /// <param name="catalogues"></param>
    public ManyCataloguesCombineable(Catalogue[] catalogues)
    {
        Catalogues = catalogues;
    }

    /// <inheritdoc />
    public string GetSqlString()
    {
        return null;
    }
}