// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Text;
using FAnsi.Discovery;
using Microsoft.Data.SqlClient;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.Job.Scheduling;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Modules.LoadProgressUpdating;

/// <summary>
///     Represents a user made descision about how to upload a LoadProgress after a succesful data load.  LoadProgress has
///     a field DataLoadProgress which stores
///     the last date that was loaded.  However you can overreach during a load e.g. run a load for 30 days but find only 5
///     days worth of data streamed through
///     the load, in such cases you might want to update the DataLoadProgress to the 5 day mark on the assumption that
///     there is a delay in data provision and it
///     will arrive later.  There are multiple ways to determine what dates were actually loaded during a data load (See
///     DataLoadProgressUpdateStrategy).
///     <para>
///         You can declare a [DemandsInitialization] decorated property of this Type in a data load component (IAttacher)
///         etc in order to illicit a decision about
///         what to update the DataLoadProgress with from the user at design time.
///     </para>
/// </summary>
public class DataLoadProgressUpdateInfo : ICustomUIDrivenClass, ICheckable
{
    public DataLoadProgressUpdateStrategy Strategy { get; set; }
    public string ExecuteScalarSQL { get; set; }
    public int Timeout { get; set; }

    #region Serialization

    public void RestoreStateFrom(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        var lines = value.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length > 0)
        {
            var fields = lines[0].Split(';');
            if (fields.Length > 0)
                if (Enum.TryParse(fields[0], out DataLoadProgressUpdateStrategy strat))
                    Strategy = strat;

            if (fields.Length > 1)
                Timeout = int.Parse(fields[1]);
        }

        ExecuteScalarSQL = "";

        for (var i = 1; i < lines.Length; i++)
            ExecuteScalarSQL += lines[i] + Environment.NewLine;
    }

    public string SaveStateToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{Strategy};{Timeout}");
        sb.AppendLine(ExecuteScalarSQL ?? "");

        return sb.ToString();
    }

    #endregion


    /// <summary>
    ///     Only call this method when you hav finished populating RAW (since the strategy ExecuteScalarSQLInRAW requires to
    ///     calculate date from populated RAW database right now and it is known that RAW won't even exist post load time)
    /// </summary>
    /// <param name="job"></param>
    /// <param name="rawDatabase"></param>
    public IUpdateLoadProgress AddAppropriateDisposeStep(ScheduledDataLoadJob job, DiscoveredDatabase rawDatabase)
    {
        IUpdateLoadProgress added;
        Check(ThrowImmediatelyCheckNotifier.Quiet);

        switch (Strategy)
        {
            case DataLoadProgressUpdateStrategy.UseMaxRequestedDay:
                added = new UpdateProgressIfLoadsuccessful(job);
                break;
            case DataLoadProgressUpdateStrategy.ExecuteScalarSQLInLIVE:

                added = new UpdateProgressToResultOfDelegate(job, () => GetMaxDate(GetLiveServer(job), job));
                break;
            case DataLoadProgressUpdateStrategy.ExecuteScalarSQLInRAW:
                try
                {
                    var dt = GetMaxDate(rawDatabase.Server, job);
                    added = new UpdateProgressToSpecificValueIfLoadsuccessful(job, dt);
                }
                catch (SqlException e)
                {
                    throw new DataLoadProgressUpdateException(
                        $"Failed to execute the following SQL in the RAW database:{ExecuteScalarSQL}", e);
                }

                break;
            case DataLoadProgressUpdateStrategy.DoNothing:
                //Do not add any post load update i.e. do nothing
                return null;
            default:
                throw new ArgumentOutOfRangeException();
        }

        job.PushForDisposal(added);
        return added;
    }

    private static DiscoveredServer GetLiveServer(ScheduledDataLoadJob job)
    {
        return DataAccessPortal.ExpectDistinctServer(job.RegularTablesToLoad.ToArray(), DataAccessContext.DataLoad,
            false);
    }

    private DateTime GetMaxDate(DiscoveredServer server, IDataLoadEventListener listener)
    {
        using var con = server.GetConnection();
        con.Open();

        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
            $"About to execute SQL to determine the maximum date for data loaded:{ExecuteScalarSQL}"));

        using var cmd = server.GetCommand(ExecuteScalarSQL, con);
        var scalarValue = cmd.ExecuteScalar();

        if (scalarValue == null || scalarValue == DBNull.Value)
            throw new DataLoadProgressUpdateException(
                "ExecuteScalarSQL specified for determining the maximum date of data loaded returned null when executed");

        try
        {
            return Convert.ToDateTime(scalarValue);
        }
        catch (Exception e)
        {
            throw new DataLoadProgressUpdateException(
                $"ExecuteScalarSQL specified for determining the maximum date of data loaded returned a value that was not a Date:{scalarValue}",
                e);
        }
    }

    public void Check(ICheckNotifier notifier)
    {
        if (Strategy == DataLoadProgressUpdateStrategy.ExecuteScalarSQLInLIVE ||
            Strategy == DataLoadProgressUpdateStrategy.ExecuteScalarSQLInRAW)
            if (string.IsNullOrWhiteSpace(ExecuteScalarSQL))
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        $"Strategy is {Strategy} but there is no ExecuteScalarSQL, ExecuteScalarSQL should be a SELECT statement that returns a specific value that reflects the maximum date in the load e.g. Select MAX(MyDate) FROM MyTable",
                        CheckResult.Fail));

        // TODO: These checks are VERY Sql Server specific!

        if (Strategy == DataLoadProgressUpdateStrategy.ExecuteScalarSQLInRAW)
            if (ExecuteScalarSQL.Contains("..") || ExecuteScalarSQL.Contains(".[dbo].") ||
                ExecuteScalarSQL.Contains(".dbo."))
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        $"Strategy is {Strategy} but the SQL looks like it references explicit tables, In general RAW queries should use unqualified table names i.e. 'Select MAX(dt) FROM MyTable' NOT 'Select MAX(dt) FROM [MyLIVEDatabase]..[MyTable]'",
                        CheckResult.Warning));

        if (Strategy == DataLoadProgressUpdateStrategy.ExecuteScalarSQLInLIVE)
            if (!(ExecuteScalarSQL.Contains("..") || ExecuteScalarSQL.Contains(".[dbo].") ||
                  ExecuteScalarSQL.Contains(".dbo.")))
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        $"Strategy is {Strategy} but the SQL does not contain '..' or '.[dbo].' or '.dbo.', LIVE update queries should use fully table names i.e. 'Select MAX(dt) FROM [MyLIVEDatabase]..[MyTable]' NOT 'Select MAX(dt) FROM MyTable'",
                        CheckResult.Warning));
    }
}