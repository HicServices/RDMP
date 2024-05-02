// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Modules.DataFlowOperations.Aliases.Exceptions;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Modules.DataFlowOperations.Aliases;

/// <summary>
///     Pipeline component for resolving the situation where a given unique patient identifier isn't unique (i.e. a person
///     has aliases) by applying an
///     AliasResolutionStrategy (See AliasHandler.docx)
/// </summary>
public class AliasHandler : IPluginDataFlowComponent<DataTable>
{
    [DemandsInitialization("The server that will be connected to to fetch the alias resolution table",
        Mandatory = true)]
    public ExternalDatabaseServer ServerToExecuteQueryOn { get; set; }

    [DemandsInitialization(
        "The context under which to connect to the server, if unsure just select DataAccessContext.DataLoad (this only matters if you have encrypted logon credentials configured on a per context level)",
        DemandType.Unspecified, DataAccessContext.DataLoad)]
    public DataAccessContext DataAccessContext { get; set; }

    [DemandsInitialization(
        "A fully specified SQL Select query to execute on ServerToExecuteQueryOn, this should result in the alias table.  The alias table must have only 2 columns.  The first column must match a column name in the input DataTable.  The second column must contain a known aliases which is different from the first column value.",
        DemandType.SQL, Mandatory = true)]
    public string AliasTableSQL { get; set; }

    [DemandsInitialization("Maximum amount of time in seconds to let the AliasTableSQL execute for before giving up",
        DemandType.Unspecified, 120)]
    public int TimeoutForAssemblingAliasTable { get; set; }

    [DemandsInitialization("Strategy for dealing with aliases in DataTables")]
    public AliasResolutionStrategy ResolutionStrategy { get; set; }

    [DemandsInitialization(
        "The name of the input column name in pipeline data (which must exist!) which contains the aliasable values.  This is probably your patient identifier column.",
        Mandatory = true)]
    public string AliasColumnInInputDataTables { get; set; }

    public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener,
        GracefulCancellationToken cancellationToken)
    {
        var newRows = new List<object[]>();

        _aliasDictionary ??= GenerateAliasTable(TimeoutForAssemblingAliasTable);

        var idx = toProcess.Columns.IndexOf(AliasColumnInInputDataTables);

        if (idx == -1)
            throw new KeyNotFoundException(
                $"You asked to resolve aliases on a column called '{AliasColumnInInputDataTables}' but no column by that name appeared in the DataTable being processed.  Columns in that table were:{string.Join(",", toProcess.Columns.Cast<DataColumn>().Select(c => c.ColumnName))}");

        var elements = toProcess.Columns.Count;

        var matchesFound = 0;

        foreach (DataRow r in toProcess.Rows)
            if (_aliasDictionary.TryGetValue(r[AliasColumnInInputDataTables], out var aliases))
            {
                matchesFound++;
                switch (ResolutionStrategy)
                {
                    case AliasResolutionStrategy.CrashIfAliasesFound:
                        throw new AliasException(
                            $"Found Alias in input data and ResolutionStrategy is {ResolutionStrategy}, aliased value was {r[AliasColumnInInputDataTables]}");

                    case AliasResolutionStrategy.MultiplyInputDataRowsByAliases:

                        //Get all aliases for the input value
                        foreach (var alias in aliases)
                        {
                            //Create a copy of the input row
                            var newRow = new object[elements];
                            r.ItemArray.CopyTo(newRow, 0);

                            //Set the aliasable element to the alias
                            newRow[idx] = alias;

                            //Add it to our new rows collection
                            newRows.Add(newRow);
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

        if (newRows.Any())
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
                $"Found {matchesFound} aliased input values, resolved by adding {newRows.Count} additional duplicate rows to the dataset"));
        else
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                $"No Aliases found for identifiers in column {AliasColumnInInputDataTables}"));

        foreach (var newRow in newRows)
            toProcess.Rows.Add(newRow);

        return toProcess;
    }

    public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
    {
        _aliasDictionary?.Clear(); //Free up memory
    }

    public void Abort(IDataLoadEventListener listener)
    {
    }

    public void Check(ICheckNotifier notifier)
    {
        var timeout = 5;
        try
        {
            var result = GenerateAliasTable(timeout);
            notifier.OnCheckPerformed(new CheckEventArgs($"Found {result.Count} aliases", CheckResult.Success));
        }
        catch (Exception e)
        {
            var isTimeout = e.Message.ToLower().Contains("timeout");
            notifier.OnCheckPerformed(new CheckEventArgs($"Failed to generate alias table after {timeout}s",
                isTimeout ? CheckResult.Warning : CheckResult.Fail, e));
        }
    }

    private Dictionary<object, List<object>> _aliasDictionary;

    private Dictionary<object, List<object>> GenerateAliasTable(int timeoutInSeconds)
    {
        const string expectation =
            "(expected the query to return 2 columns, the first one being the input column the second being known aliases)";

        var toReturn = new Dictionary<object, List<object>>();

        var server = DataAccessPortal.ExpectServer(ServerToExecuteQueryOn, DataAccessContext);

        using var con = server.GetConnection();
        con.Open();

        using var cmd = server.GetCommand(AliasTableSQL, con);
        cmd.CommandTimeout = timeoutInSeconds;

        using var r = cmd.ExecuteReader();
        var haveCheckedColumns = false;

        while (r.Read())
        {
            if (!haveCheckedColumns)
            {
                int idx;

                try
                {
                    idx = r.GetOrdinal(AliasColumnInInputDataTables);
                }
                catch (IndexOutOfRangeException)
                {
                    throw new AliasTableFetchException(
                        $"Alias table did not contain a column called '{AliasColumnInInputDataTables}' {expectation}");
                }

                if (idx == -1)
                    throw new AliasTableFetchException(
                        $"Alias table did not contain a column called '{AliasColumnInInputDataTables}' {expectation}");

                if (idx != 0)
                    throw new AliasTableFetchException(
                        $"Alias table DID contain column '{AliasColumnInInputDataTables}' but it was not the first column in the result set {expectation}");

                if (r.FieldCount != 2)
                    throw new AliasTableFetchException(
                        $"Alias table SQL resulted in {r.FieldCount} fields being returned, we expect exactly 2 {expectation}");

                haveCheckedColumns = true;
            }

            var input = r[0];
            var alias = r[1];

            if (input == null || input == DBNull.Value || alias == null || alias == DBNull.Value)
                throw new AliasTableFetchException("Alias table contained nulls");

            if (input.Equals(alias))
                throw new AliasTableFetchException(
                    "Alias table SQL should only return aliases not exact matches e.g. in the case of a simple alias X is Y, do not return 4 rows {X=X AND Y=Y AND Y=X AND X=Y}, only return 2 rows {X=Y and Y=X}");

            if (!toReturn.ContainsKey(input))
                toReturn.Add(input, new List<object>());

            toReturn[input].Add(alias);
        }

        return toReturn;
    }
}