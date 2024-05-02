// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Rdmp.Core.CohortCommitting.Pipeline;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Modules.DataFlowOperations;

/// <summary>
///     Component for reproducibly pulling a random sample of records from a cohort being committed.  The random number
///     generator
///     is seeded on the Project number such that using the sampler again on the same input will produce the same random
///     selection.
/// </summary>
public class CohortSampler : IPluginDataFlowComponent<DataTable>, IPipelineRequirement<CohortCreationRequest>
{
    private IExternalCohortTable _ect;
    private IProject _project;
    private bool _firstBatch = true;

    [DemandsInitialization("The number of unique patient identifiers you want returned from the input data",
        DefaultValue = 100)]
    public int SampleSize { get; set; } = 100;

    [DemandsInitialization(
        "Determines components behaviour if not enough unique identifiers are being committed.  True to crash.  False to pass on however many records there are.",
        DefaultValue = true)]
    public bool FailIfNotEnoughIdentifiers { get; set; } = true;

    [DemandsInitialization(
        "Optional.  The name of the identifier column that you are submitting.  Set this if it is different than the destination cohort private identifier field")]
    public string PrivateIdentifierColumnName { get; set; }

    public void Abort(IDataLoadEventListener listener)
    {
    }

    public void Check(ICheckNotifier notifier)
    {
    }

    public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
    {
    }

    public void PreInitialize(CohortCreationRequest value, IDataLoadEventListener listener)
    {
        _ect = value.NewCohortDefinition.LocationOfCohort;
        _project = value.Project;
    }

    public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener,
        GracefulCancellationToken cancellationToken)
    {
        if (!_firstBatch)
            throw new Exception(
                "Expected to get the whole cohort at once but got multiple batches.  This component only works if the Source returns all data at once");

        if (_project.ProjectNumber == null)
            throw new Exception(
                "Project must have a ProjectNumber so that it can be used as a seed in random cohort sampling");

        var expectedFieldName = GetPrivateFieldName();

        listener.OnNotify(this,
            new NotifyEventArgs(ProgressEventType.Information,
                $"Looking for column called '{expectedFieldName}' in the data in order to produce a sample"));

        if (!toProcess.Columns.Contains(expectedFieldName))
            throw new Exception(
                $"CohortSampler was unable to find a column called '{expectedFieldName}' in the data passed in.  This is the expected private identifier column name of the cohort you are committing.");

        // get all the unique values
        var uniques = new HashSet<object>();

        foreach (DataRow row in toProcess.Rows)
        {
            var val = row[expectedFieldName];

            if (val != DBNull.Value) uniques.Add(val);
        }

        _firstBatch = false;


        var r = new Random(_project.ProjectNumber.Value);

        // first order the values e.g. alphabetically so that even if the input is in a different order our
        // seeded random picks the same values.  Se test TestCohortSampler_Repeatability_OrderIrrelevant
        var sorted = uniques.OrderBy(u => u);
        var chosen = sorted.OrderBy(v => r.Next()).Take(SampleSize).ToList();
        if (chosen.Count < SampleSize && FailIfNotEnoughIdentifiers)
            throw new Exception(
                $"Cohort only contains {chosen.Count} unique identifiers.  This is less than the requested sample size of {SampleSize} and {nameof(FailIfNotEnoughIdentifiers)} is true");

        var dtToReturn = new DataTable();
        dtToReturn.BeginLoadData();
        dtToReturn.Columns.Add(expectedFieldName);

        foreach (var val in chosen) dtToReturn.Rows.Add(val);

        dtToReturn.EndLoadData();
        return dtToReturn;
    }

    private string GetPrivateFieldName()
    {
        if (!string.IsNullOrWhiteSpace(PrivateIdentifierColumnName))
            return PrivateIdentifierColumnName;

        var syntax = _ect.GetQuerySyntaxHelper();
        return syntax.GetRuntimeName(_ect.PrivateIdentifierField);
    }
}