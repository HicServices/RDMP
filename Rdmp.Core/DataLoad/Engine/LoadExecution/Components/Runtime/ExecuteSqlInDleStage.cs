// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Runtime;

internal class ExecuteSqlInDleStage
{
    private readonly IDataLoadJob _job;
    private readonly LoadStage _loadStage;
    private readonly Regex _regexEntity = new(@"{([CT]):(\d+)}", RegexOptions.IgnoreCase);

    public ExecuteSqlInDleStage(IDataLoadJob job, LoadStage loadStage)
    {
        _job = job;
        _loadStage = loadStage;
    }

    public ExitCodeType Execute(string commandText, DiscoveredDatabase db)
    {
        var syntax = db.Server.GetQuerySyntaxHelper();
        commandText = _regexEntity.Replace(commandText, m => GetEntityForMatch(m, syntax));

        try
        {
            Dictionary<int, Stopwatch> performance;


            using (var con = db.Server.GetConnection())
            {
                con.Open();
                UsefulStuff.ExecuteBatchNonQuery(commandText, con, null, out performance, 600000);
            }

            foreach (var section in performance)
                _job.OnNotify(this,
                    new NotifyEventArgs(ProgressEventType.Information,
                        $"Batch ending on line  \"{section.Key}\" finished after {section.Value.Elapsed}"));
        }
        catch (Exception e)
        {
            throw new Exception($"Failed to execute the query: {e}");
        }

        return ExitCodeType.Success;
    }

    private string GetEntityForMatch(Match match, IQuerySyntaxHelper syntaxHelper)
    {
        if (match.Groups.Count != 3)
            throw new ExecuteSqlFileRuntimeTaskException(
                $"Regex Match in Sql File had {match.Groups.Count} Groups, expected 3,  Match was:'{match.Value}'");

        char entity;
        int id;
        try
        {
            entity = match.Groups[1].Value.ToUpper()[0];
            id = int.Parse(match.Groups[2].Value);
        }
        catch (Exception e)
        {
            throw new ExecuteSqlFileRuntimeTaskException(
                $"Error performing substitution in Sql File, Failed to replace match {match.Value} due to parse expectations",
                e);
        }

        var tables = _job.RegularTablesToLoad.Union(_job.LookupTablesToLoad);

        var namer = _job.Configuration.DatabaseNamer;

        switch (entity)
        {
            case 'T':
                var toReturnTable = tables.SingleOrDefault(t => t.ID == id) ??
                                    throw new ExecuteSqlFileRuntimeTaskException(
                                        $"Failed to find a TableInfo in the load with ID {id}.  All TableInfo IDs referenced in script must be part of the LoadMetadata");
                return toReturnTable.GetRuntimeName(_loadStage, namer);

            case 'C':

                var toReturnColumn = tables.SelectMany(t => t.ColumnInfos).SingleOrDefault(t => t.ID == id) ??
                                     throw new ExecuteSqlFileRuntimeTaskException(
                                         $"Failed to find a ColumnInfo in the load with ID {id}.  All ColumnInfo IDs referenced in script must be part of the LoadMetadata");
                var db = toReturnColumn.TableInfo.GetDatabaseRuntimeName(_loadStage, namer);
                var tbl = toReturnColumn.TableInfo.GetRuntimeName(_loadStage, namer);
                var col = toReturnColumn.GetRuntimeName(_loadStage);

                return syntaxHelper.EnsureFullyQualified(db, null, tbl, col);

            default:
                throw new ExecuteSqlFileRuntimeTaskException(
                    $"Error performing substitution in Sql File, Unexpected Type char in regex:{entity}");
        }
    }
}