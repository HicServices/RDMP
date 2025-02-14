// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.CohortCommitting.Pipeline.Sources;

/// <summary>
/// Pipeline source component that generates a DataTable containing all the unique patient identifiers in the column referenced by the <see cref="IPipelineRequirement{T}"/>
/// <see cref="ExtractionInformation"/>.
/// </summary>
public class PatientIdentifierColumnSource : IPluginDataFlowSource<DataTable>,
    IPipelineRequirement<ExtractionInformation>
{
    private ExtractionInformation _extractionInformation;

    private bool _haveSentData;

    [DemandsInitialization("How long to wait for the select query to run before giving up in seconds",
        DemandType.Unspecified, 60)]
    public int Timeout { get; set; }

    public DataTable GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
    {
        if (_haveSentData)
            return null;

        _haveSentData = true;

        return GetDataTable(Timeout, null);
    }

    private DataTable GetDataTable(int timeout, int? topX)
    {
        var qb = new QueryBuilder("distinct", null);
        if (topX != null)
            qb.TopX = topX.Value;
        qb.AddColumn(_extractionInformation);

        var server =
            _extractionInformation.CatalogueItem.Catalogue.GetDistinctLiveDatabaseServer(DataAccessContext.DataExport,
                true);

        var colName = _extractionInformation.GetRuntimeName();

        var dt = new DataTable();
        dt.BeginLoadData();
        dt.Columns.Add(colName);

        using var con = server.GetConnection();
        con.Open();
        using var cmd = server.GetCommand(qb.SQL, con);
        cmd.CommandTimeout = timeout;

        using var r = cmd.ExecuteReader();
        while (r.Read())
            dt.Rows.Add(r[colName]);

        dt.EndLoadData();
        return dt;
    }

    public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
    {
    }

    public void Abort(IDataLoadEventListener listener)
    {
    }

    public DataTable TryGetPreview() => GetDataTable(10, 1000);

    public void Check(ICheckNotifier notifier)
    {
        if (!_extractionInformation.IsExtractionIdentifier)
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"Column '{_extractionInformation}' is not marked IsExtractionIdentifier, are you sure it contains patient identifiers?",
                CheckResult.Fail));

        try
        {
            var dt = GetDataTable(5, 1000);

            if (dt.Rows.Count == 0)
                notifier.OnCheckPerformed(new CheckEventArgs("The table is empty!", CheckResult.Fail));
        }
        catch (Exception ex)
        {
            notifier.OnCheckPerformed(new CheckEventArgs("Failed to get DataTable/build query", CheckResult.Fail, ex));
        }
    }

    public void PreInitialize(ExtractionInformation value, IDataLoadEventListener listener)
    {
        _extractionInformation = value;
    }
}