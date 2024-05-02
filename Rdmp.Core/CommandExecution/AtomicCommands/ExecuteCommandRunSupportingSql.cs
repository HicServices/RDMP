// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using System.Text.RegularExpressions;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataViewing;
using Rdmp.Core.Repositories.Construction;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
///     Runs the SQL in <see cref="SupportingSQLTable" /> and displays output (if a single table is returned)
/// </summary>
public partial class ExecuteCommandRunSupportingSql : ExecuteCommandViewDataBase
{
    [UseWithObjectConstructor]
    public ExecuteCommandRunSupportingSql(IBasicActivateItems activator,
        [DemandsInitialization("RDMP object storing the sql to run and where to run it (including credentials if any)")]
        SupportingSQLTable supportingSQLTable,
        [DemandsInitialization(ToFileDescription)]
        FileInfo toFile = null)
        : base(activator, toFile)
    {
        SupportingSQLTable = supportingSQLTable;

        if (SupportingSQLTable.ExternalDatabaseServer_ID == null)
        {
            SetImpossible("No server is configured on SupportingSQLTable");
            return;
        }

        if (string.IsNullOrWhiteSpace(SupportingSQLTable.SQL))
        {
            SetImpossible($"No SQL is defined for {SupportingSQLTable}");
        }
    }

    public SupportingSQLTable SupportingSQLTable { get; }

    protected override IViewSQLAndResultsCollection GetCollection()
    {
        // windows GUI client needs to confirm dangerous queries (don't want misclicks to do bad things)
        if (!string.IsNullOrWhiteSpace(SupportingSQLTable.SQL) &&
            BasicActivator.IsWinForms &&
            RiskySqlRegex().IsMatch(SupportingSQLTable.SQL) &&
            !BasicActivator.YesNo("Running this SQL may make changes to your database, really run?", "Run SQL"))
            return null;
        return new ViewSupportingSqlCollection(SupportingSQLTable);
    }

    [GeneratedRegex("\\b(update|delete|drop|truncate)\\b", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex RiskySqlRegex();
}