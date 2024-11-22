// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
/// Import all CatalogueItem descriptions from one <see cref="Catalogue"/> into another
/// </summary>
public class ExecuteCommandImportCatalogueItemDescriptions : BasicCommandExecution
{
    public Catalogue ToPopulate { get; }
    public Catalogue Other { get; set; }

    public ExecuteCommandImportCatalogueItemDescriptions(IBasicActivateItems activator, Catalogue toPopulate,
        Catalogue other) : base(activator)
    {
        ToPopulate = toPopulate;
        Other = other;
    }

    public override void Execute()
    {
        base.Execute();

        var other = Other;

        if (other == null)
            if (!SelectOne(BasicActivator.CoreChildProvider.AllCatalogues, out other))
                return;

        foreach (var ci in ToPopulate.CatalogueItems)
        {
            var match = other.CatalogueItems.FirstOrDefault(o =>
                o.Name.Equals(ci.Name, StringComparison.CurrentCultureIgnoreCase) &&
                !string.IsNullOrWhiteSpace(o.Description));

            if (match != null)
                ExecuteCommandImportCatalogueItemDescription.CopyNonIDValuesAcross(match, ci, true);
        }

        Publish(ToPopulate);
    }
}