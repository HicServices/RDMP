// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FAnsi.Discovery;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.Logging;

/// <summary>
/// Checks that a logging database is accessible and that the default system statuses and dataset names are present.
/// </summary>
public class LoggingDatabaseChecker : ICheckable
{
    private DiscoveredServer _server;


    public LoggingDatabaseChecker(DiscoveredServer server)
    {
        _server = server;
    }

    public LoggingDatabaseChecker(IDataAccessPoint target)
        : this(DataAccessPortal.ExpectServer(target, DataAccessContext.Logging))
    {
    }

    public void Check(ICheckNotifier notifier)
    {
        try
        {
            CheckDataLoadTaskStatusTable(notifier);
            CheckFatalErrorStatusTable(notifier);
            CheckRowErrorTypeTable(notifier);

            CheckDefaultDatasetsExist(notifier);
            CheckDefaultDataLoadTasksExist(notifier);
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(new CheckEventArgs("Entire checking process crashed", CheckResult.Fail, e));
        }
    }

    private void CheckDataLoadTaskStatusTable(ICheckNotifier notifier)
    {
        var desired = new Dictionary<int, string>
        {
            { 1, "Open" },
            { 2, "Ready" },
            { 3, "Committed" }
        };


        CheckLookupTableIsCorrectlyPopulated(notifier, "status","z_DataLoadTaskStatus",desired);
    }

    private void CheckFatalErrorStatusTable(ICheckNotifier notifier)
    {
        Debug.Assert((int)DataLoadInfo.FatalErrorStates.Outstanding == 1);
        Debug.Assert((int)DataLoadInfo.FatalErrorStates.Resolved == 2);
        Debug.Assert((int)DataLoadInfo.FatalErrorStates.Blocked == 3);

        var expectedFatalErrorStatusRows = new Dictionary<int, string>
        {
            {1, "Outstanding"},
            {2, "Resolved"},
            {3, "Blocked"}
        };
        CheckLookupTableIsCorrectlyPopulated(notifier, "status", "z_FatalErrorStatus", expectedFatalErrorStatusRows);
    }

    private void CheckRowErrorTypeTable(ICheckNotifier notifier)
    {
        var expectedRowErrorTypes = new Dictionary<int, string>
        {
            {1, "LoadRow"},
            {2, "Duplication"},
            {3, "Validation"},
            {4, "DatabaseOperation"},
            {5, "Unknown"}
        };
        CheckLookupTableIsCorrectlyPopulated(notifier, "type", "z_RowErrorType", expectedRowErrorTypes);
    }

    private void CheckDefaultDatasetsExist(ICheckNotifier notifier)
    {
        CheckDatasetExists(notifier, "Internal");
        CheckDatasetExists(notifier, "DataExtraction");
    }

    private void CheckDatasetExists(ICheckNotifier notifier, string dataSetID)
    {
        using (var conn = _server.GetConnection())
        {
            conn.Open();

            using (var cmd = _server.GetCommand("SELECT 1 FROM DataSet WHERE dataSetID=@dsID", conn))
            {
                _server.AddParameterWithValueToCommand("@dsID", cmd, dataSetID);

                var found = cmd.ExecuteScalar();

                if (found != null)
                {
                    notifier.OnCheckPerformed(new CheckEventArgs($"Found default dataset: {dataSetID}",
                        CheckResult.Success, null));
                    return;
                }
            }

            if (notifier.OnCheckPerformed(new CheckEventArgs($"Did not find default dataset '{dataSetID}'.", CheckResult.Fail, null,
                    $"Create the dataset '{dataSetID}'")))
            {
                using (var cmd =
                       _server.GetCommand("INSERT INTO DataSet (dataSetID, name) VALUES (@dsID, @dsID)", conn))
                {
                    _server.AddParameterWithValueToCommand("@dsID", cmd, dataSetID);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }

    private void CheckDefaultDataLoadTasksExist(ICheckNotifier notifier)
    {
        CheckDataLoadTaskExists(notifier, 1, "Internal");
        CheckDataLoadTaskExists(notifier, 2, "DataExtraction");
    }

    private void CheckDataLoadTaskExists(ICheckNotifier notifier, int id, string dataSetID)
    {

        var lm = new LogManager(_server);


        if (lm.ListDataTasks().Contains(dataSetID))
            notifier.OnCheckPerformed(new CheckEventArgs($"Found default DataLoadTask for '{dataSetID}'", CheckResult.Success,
                null));
        else
        {
            var shouldCreate =
                notifier.OnCheckPerformed(new CheckEventArgs($"Did not find default DataLoadTask for '{dataSetID}'.",
                    CheckResult.Fail,
                    null, $"Create a DataLoadTask for '{dataSetID}'"));

            if(shouldCreate)
                lm.CreateNewLoggingTask(id, dataSetID);
        }
    }

    private void CheckLookupTableIsCorrectlyPopulated(ICheckNotifier notifier, string valueColumnName, string tableName, Dictionary<int, string> expected)
    {
        //see what is in the database
        var actual = new Dictionary<int, string>();
        using (var conn = _server.GetConnection())
        {
            conn.Open();

            using(var cmd = _server.GetCommand($"SELECT ID, {valueColumnName} FROM {tableName}", conn))
            using(var reader = cmd.ExecuteReader())
                while (reader.Read())
                    actual.Add(Convert.ToInt32(reader["ID"]), reader[valueColumnName].ToString().Trim());
        }

        //now reconcile what is in the database with what we expect

        ExpectedLookupsValuesArePresent(expected, actual, out var missing, out var collisions, out var misnomers);

        if(!missing.Any() && !collisions.Any() && !misnomers.Any())
        {
            notifier.OnCheckPerformed(new CheckEventArgs($"{tableName} contains the correct lookup values", CheckResult.Success, null));
            return;
        }

        //collisions cannot be resolved without manual intervention
        if (collisions.Any())
        {

            notifier.OnCheckPerformed(new CheckEventArgs(
                $"{tableName} there is a key collision between what we require and what is in the database, the mismatches are:{Environment.NewLine}{collisions.Aggregate("", (s, n) => $"{s}Desired:({n.Key},'{n.Value}') VS Found:({n.Key},'{actual[n.Key]}'){Environment.NewLine}")}{collisions}",CheckResult.Fail, null));
            return;
        }

        //misnomers cannot be resolved without manual intervention either
        if (misnomers.Any())
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"{tableName} the following ID conflicts were found:{misnomers.Aggregate("", (s, n) => s + Environment.NewLine + n)}", CheckResult.Fail, null));



        if(missing.Any())
        {

            //add missing values
            if (notifier.OnCheckPerformed(new CheckEventArgs(
                    $"{tableName} does not contain all the required lookup statuses",
                    CheckResult.Fail, null,
                    $"Insert the missing lookups ({missing.Aggregate("", (s, pair) => $"{s}, {pair.Value}")})")))
            {
                using(var c = _server.BeginNewTransactedConnection())
                {
                    _server.GetCommand($"SET IDENTITY_INSERT {tableName} ON ", c).ExecuteNonQuery();

                    foreach (var kvp in missing)
                        _server.GetCommand(
                            $"INSERT INTO {tableName}(ID,{valueColumnName}) VALUES ({kvp.Key},'{kvp.Value}')", c).ExecuteNonQuery();

                    _server.GetCommand($"SET IDENTITY_INSERT {tableName} OFF ", c).ExecuteNonQuery();

                    c.ManagedTransaction.CommitAndCloseConnection();
                }
            }
        }

    }


    private static void ExpectedLookupsValuesArePresent(Dictionary<int, string> expected, Dictionary<int, string> actual, out Dictionary<int, string> missing, out Dictionary<int, string> collisions, out List<string> misnomers)
    {

        collisions = new Dictionary<int, string>();
        missing = new Dictionary<int, string>();
        misnomers = new List<string>();

        //for each desired kvp
        foreach (var kvp in expected)
        {

            //make sure it is not a misnomer
            if (actual.Any(m => m.Value.Equals(kvp.Value) && m.Key != kvp.Key)) //if there are any actuals that have different keys
            {
                //see if it is a misnomer e.g. 1,'MyFunkyStatus' vs 2,'MyFunkyStatus'
                var misnomer = actual.Where(m => m.Value.Equals(kvp.Value)).Select(p => p.Key).ToArray(); //get ALL the keys that correspond to this value including exact matching key=key (to deal with schitzophrenia)

                misnomers.Add(
                    misnomer.Length == 1
                        ? $"{kvp.Value} is known under ID={kvp.Key} in desired but in your live database it is ID={misnomer.Single()}"
                        : $"{kvp.Value} is known under ID={kvp.Key} in desired but in your live database is it is known schizophrenically as ({misnomer.Aggregate("", (s, n) => $"{s}ID={n},").TrimEnd(',')})");
            }


            //there is a required ID that does not exist e.g. 99,'MyFunkyStatus'
            if(!actual.ContainsKey(kvp.Key))
            {

                missing.Add(kvp.Key,kvp.Value);
                continue;
            }

            //there is an ID but they are different values e.g. 1,'Outstanding' in one and 1,'Resolved' in the other
            if(!actual[kvp.Key].Equals(kvp.Value))
                collisions.Add(kvp.Key,kvp.Value);


        }
    }
}