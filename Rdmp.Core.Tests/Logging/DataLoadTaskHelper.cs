// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using FAnsi.Discovery;
using Rdmp.Core.Logging;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.Tests.Logging;

internal class DataLoadTaskHelper
{
    private readonly DiscoveredServer _loggingServer;
    private readonly Stack<string> _sqlToCleanUp = new();

    public DataLoadTaskHelper(DiscoveredServer loggingServer)
    {
        _loggingServer = loggingServer;
    }

    public void SetUp()
    {
        var checker = new LoggingDatabaseChecker(_loggingServer);
        checker.Check(new AcceptAllCheckNotifier());
    }

    public void CreateDataLoadTask(string taskName)
    {
        using var con =_loggingServer.GetConnection();
        con.Open();

        var datasetName = $"Test_{taskName}";
        var datasetCmd = _loggingServer.GetCommand($"INSERT INTO DataSet (dataSetID) VALUES ('{datasetName}')", con);
        datasetCmd.ExecuteNonQuery();
        _sqlToCleanUp.Push($"DELETE FROM DataSet WHERE dataSetID = '{datasetName}'");

        var taskCmd =
            _loggingServer.GetCommand(
                $"INSERT INTO DataLoadTask VALUES (100, '{taskName}', '{taskName}',@date, '{datasetName}', 1, 1, '{datasetName}')",
                con);

        _loggingServer.AddParameterWithValueToCommand("@date", taskCmd, DateTime.Now);

        taskCmd.ExecuteNonQuery();
        _sqlToCleanUp.Push($"DELETE FROM DataLoadTask WHERE dataSetID = '{datasetName}'");
    }

    public void TearDown()
    {
        using var con = _loggingServer.GetConnection();
        con.Open();

        while (_sqlToCleanUp.Any())
            _loggingServer.GetCommand(_sqlToCleanUp.Pop(), con).ExecuteNonQuery();
    }
}