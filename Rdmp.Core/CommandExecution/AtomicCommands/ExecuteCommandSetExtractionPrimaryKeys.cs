// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandSetExtractionPrimaryKeys : ExecuteCommandSetColumnSettingBase, IAtomicCommand
{
    /// <summary>
    ///     Change which column(s) should be marked as primary key on extraction to a destination that supports this feature
    ///     (e.g. to database).
    ///     Operation can be applied either at global <see cref="Catalogue" /> level or for a specific
    ///     <paramref name="inConfiguration" />
    /// </summary>
    /// <param name="activator"></param>
    /// <param name="catalogue"></param>
    /// <param name="inConfiguration"></param>
    /// <param name="column"></param>
    public ExecuteCommandSetExtractionPrimaryKeys(IBasicActivateItems activator,
            [DemandsInitialization("The dataset you want to change the extraction primary keys for")]
            ICatalogue catalogue,
            [DemandsInitialization(
                "Optional - The specific extraction you want the change made in or Null for the Catalogue itself (will affect all future extractions)")]
            IExtractionConfiguration inConfiguration,
            [DemandsInitialization(
                "Optional - The Column name(s) you want to select as the new extraction primary keys.  Comma separate multiple entries if needed")]
            string column)
        // base class args
        : base(activator, catalogue, inConfiguration, column,
            "Set Extraction Primary Key",
            "Extraction Primary Key")
    {
    }

    public override string GetCommandHelp()
    {
        return "Change which column(s) should be marked as primary key if extracting to database";
    }

    protected override bool IsValidSelection(ConcreteColumn[] selected)
    {
        return true;
    }

    protected override bool Getter(ConcreteColumn c)
    {
        return c.IsPrimaryKey;
    }

    protected override void Setter(ConcreteColumn ec, bool newValue)
    {
        ec.IsPrimaryKey = newValue;
    }
}