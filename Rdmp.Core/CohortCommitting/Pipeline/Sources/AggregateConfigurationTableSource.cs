// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.CohortCommitting.Pipeline.Sources;

/// <summary>
///     Pipeline source component which executes an AggregateConfiguration query (e.g. Aggregate Graph / Joinable patient
///     index table)
/// </summary>
public class AggregateConfigurationTableSource : IPluginDataFlowSource<DataTable>,
    IPipelineRequirement<AggregateConfiguration>
{
    protected AggregateConfiguration AggregateConfiguration;
    protected CohortIdentificationConfiguration CohortIdentificationConfigurationIfAny;

    private bool _haveSentData;

    [DemandsInitialization(
        "The length of time (in seconds) to wait before timing out the SQL command to execute the Aggregate.",
        DemandType.Unspecified, 10000)]
    public int Timeout { get; set; }

    /// <summary>
    ///     The name to give the table produced into the pipeline
    /// </summary>
    public string TableName { get; set; }

    protected virtual string GetSQL()
    {
        if (!AggregateConfiguration.IsCohortIdentificationAggregate)
        {
            var builder = AggregateConfiguration.GetQueryBuilder();
            return builder.SQL;
        }

        var cic = AggregateConfiguration.GetCohortIdentificationConfigurationIfAny() ??
                  throw new Exception(
                      $"There GetCohortIdentificationConfiguration is unknown for '{AggregateConfiguration}'");
        var cohortBuilder = new CohortQueryBuilder(AggregateConfiguration, cic.GetAllParameters(), null);
        return cohortBuilder.SQL;
    }

    public DataTable GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
    {
        if (_haveSentData)
            return null;

        _haveSentData = true;

        return GetDataTable(Timeout, listener);
    }

    private DataTable GetDataTable(int timeout, IDataLoadEventListener listener)
    {
        listener?.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
            $"About to lookup which server to interrogate for AggregateConfiguration '{AggregateConfiguration}'"));

        var server =
            AggregateConfiguration.Catalogue.GetDistinctLiveDatabaseServer(DataAccessContext.DataExport, false);


        using var con = server.GetConnection();
        con.Open();

        var sql = GetSQL();

        listener?.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
            $"Connection opened, ready to send the following SQL (with Timeout {Timeout}s):{Environment.NewLine}{sql}"));

        var dt = new DataTable();
        dt.BeginLoadData();

        using (var cmd = server.GetCommand(sql, con))
        {
            cmd.CommandTimeout = timeout;
            using var da = server.GetDataAdapter(cmd);
            da.Fill(dt);
        }


        dt.TableName = TableName;

        listener?.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
            $"successfully read {dt.Rows.Count} rows from source"));

        dt.EndLoadData();
        return dt;
    }

    public DataTable TryGetPreview()
    {
        return GetDataTable(10, null);
    }


    public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
    {
    }

    public void Abort(IDataLoadEventListener listener)
    {
    }

    public virtual void Check(ICheckNotifier notifier)
    {
        try
        {
            var _sql = GetSQL();
            notifier.OnCheckPerformed(new CheckEventArgs($"successfully built extraction SQL:{_sql}",
                CheckResult.Success));
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"Could not build extraction SQL for '{AggregateConfiguration}' (ID={AggregateConfiguration.ID})",
                CheckResult.Fail, e));
        }
    }

    public virtual void PreInitialize(AggregateConfiguration value, IDataLoadEventListener listener)
    {
        AggregateConfiguration = value;

        CohortIdentificationConfigurationIfAny = value.GetCohortIdentificationConfigurationIfAny();
    }

    [NotNull]
    public override string ToString()
    {
        return GetType().Name;
    }
}