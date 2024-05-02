// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.DataExport.Checks;

/// <summary>
///     Checks that a given <see cref="SupportingSQLTable" /> is reachable by the current user and that the
///     <see cref="SupportingSQLTable.SQL" /> can be executed and returns data.
/// </summary>
public class SupportingSQLTableChecker : ICheckable
{
    private readonly SupportingSQLTable _table;

    /// <summary>
    ///     Sets up checking of the supplied
    /// </summary>
    /// <param name="table"></param>
    public SupportingSQLTableChecker(SupportingSQLTable table)
    {
        _table = table;
    }

    /// <summary>
    ///     Checks that the table can be reached on its listed server and that at least one row can be read from it.
    /// </summary>
    /// <param name="notifier"></param>
    public void Check(ICheckNotifier notifier)
    {
        try
        {
            notifier.OnCheckPerformed(new CheckEventArgs($"Found SupportingSQLTable {_table} about to check it",
                CheckResult.Success));

            var supportingSQLServer = _table.GetServer();

            notifier.OnCheckPerformed(supportingSQLServer.Exists()
                ? new CheckEventArgs($"Server {supportingSQLServer} exists", CheckResult.Success)
                : new CheckEventArgs($"Server {supportingSQLServer} does not exist", CheckResult.Fail));

            using var con = _table.GetServer().GetConnection();
            con.Open();

            notifier.OnCheckPerformed(new CheckEventArgs($"About to check Extraction SQL:{_table.SQL}",
                CheckResult.Success));

            using var cmd = supportingSQLServer.GetCommand(_table.SQL, con);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        "SupportingSQLTable table fetched successfully and at least 1 data row was read ",
                        CheckResult.Success));
            else
                notifier.OnCheckPerformed(new CheckEventArgs(
                    $"No data was successfully read from SupportingSQLTable {_table}", CheckResult.Fail));
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"Checking of SupportingSQLTable {_table} failed with Exception", CheckResult.Fail, e));
        }
    }
}