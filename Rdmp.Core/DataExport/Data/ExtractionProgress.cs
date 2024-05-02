// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataExport.Data;

/// <inheritdoc cref="IExtractionProgress" />
public class ExtractionProgress : DatabaseEntity, IExtractionProgress
{
    #region Database Properties

    private int _selectedDataSets_ID;
    private DateTime? _progress;
    private int _extractionInformation_ID;
    private DateTime? _startDate;
    private DateTime? _endDate;
    private int _numberOfDaysPerBatch;
    private string _name;
    private RetryStrategy _retry;

    #endregion

    /// <inheritdoc />
    public int SelectedDataSets_ID
    {
        get => _selectedDataSets_ID;
        set => SetField(ref _selectedDataSets_ID, value);
    }

    /// <inheritdoc />
    public DateTime? ProgressDate
    {
        get => _progress;
        set => SetField(ref _progress, value);
    }

    /// <inheritdoc />
    public DateTime? StartDate
    {
        get => _startDate;
        set => SetField(ref _startDate, value);
    }

    /// <inheritdoc />
    public DateTime? EndDate
    {
        get => _endDate;
        set => SetField(ref _endDate, value);
    }

    /// <inheritdoc />
    public int NumberOfDaysPerBatch
    {
        get => _numberOfDaysPerBatch;
        set => SetField(ref _numberOfDaysPerBatch, value);
    }

    /// <inheritdoc />
    public int ExtractionInformation_ID
    {
        get => _extractionInformation_ID;
        set => SetField(ref _extractionInformation_ID, value);
    }

    /// <inheritdoc />
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    public RetryStrategy Retry
    {
        get => _retry;
        set => SetField(ref _retry, value);
    }

    #region Relationships

    /// <inheritdoc />
    [NoMappingToDatabase]
    public ISelectedDataSets SelectedDataSets =>
        DataExportRepository.GetObjectByID<SelectedDataSets>(SelectedDataSets_ID);

    public static void ValidateSelectedColumn(ICheckNotifier notifier, ExtractionInformation ei)
    {
        if (ei == null)
        {
            notifier.OnCheckPerformed(new CheckEventArgs("A date column on which to split batches must be selected",
                CheckResult.Fail));
            return;
        }

        var col = ei.ColumnInfo;

        if (col == null)
        {
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"ExtractionInformation '{ei}' is an orphan with no associated ColumnInfo", CheckResult.Fail));
            return;
        }

        try
        {
            if (col.GetQuerySyntaxHelper().TypeTranslater.GetCSharpTypeForSQLDBType(col.Data_type) != typeof(DateTime))
                notifier.OnCheckPerformed(new CheckEventArgs(ErrorCodes.ExtractionProgressColumnProbablyNotADate, ei,
                    col.Data_type));
        }
        catch (Exception ex)
        {
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"Could not determine datatype of ColumnInfo {col} ('{col?.Data_type}')", CheckResult.Fail, ex));
        }
    }

    /// <inheritdoc />
    [NoMappingToDatabase]
    public ExtractionInformation ExtractionInformation =>
        DataExportRepository.CatalogueRepository.GetObjectByID<ExtractionInformation>(ExtractionInformation_ID);

    #endregion

    public ExtractionProgress()
    {
    }

    public ExtractionProgress(IDataExportRepository repository, ISelectedDataSets sds, DateTime? startDate,
        DateTime? endDate, int numberOfDaysPerBatch, string name, int extractionInformation_ID)
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "SelectedDataSets_ID", sds.ID },
            { "ExtractionInformation_ID", extractionInformation_ID },
            { "NumberOfDaysPerBatch", numberOfDaysPerBatch },
            { "StartDate", startDate },
            { "EndDate", endDate },
            { "Name", name },
            { "Retry", RetryStrategy.NoRetry }
        });

        if (ID == 0 || Repository != repository)
            throw new ArgumentException("Repository failed to properly hydrate this class");
    }

    public ExtractionProgress(IDataExportRepository repository, ISelectedDataSets sds)
    {
        var cata = sds.GetCatalogue();
        var coverageColId = cata?.TimeCoverage_ExtractionInformation_ID;

        if (sds.ExtractionProgressIfAny != null)
            throw new Exception($"There is already an ExtractionProgress associated with {sds}");

        if (!coverageColId.HasValue)
            throw new ArgumentException(
                $"Cannot create ExtractionProgress because Catalogue {cata} does not have a time coverage column");

        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "SelectedDataSets_ID", sds.ID },
            { "ExtractionInformation_ID", coverageColId },
            { "NumberOfDaysPerBatch", 365 },
            { "Name", $"ExtractionProgress{Guid.NewGuid()}" },
            { "Retry", RetryStrategy.NoRetry }
        });

        if (ID == 0 || Repository != repository)
            throw new ArgumentException("Repository failed to properly hydrate this class");
    }

    public ExtractionProgress(IDataExportRepository repository, DbDataReader r) : base(repository, r)
    {
        SelectedDataSets_ID = Convert.ToInt32(r["SelectedDataSets_ID"]);
        ProgressDate = ObjectToNullableDateTime(r["ProgressDate"]);
        StartDate = ObjectToNullableDateTime(r["StartDate"]);
        EndDate = ObjectToNullableDateTime(r["EndDate"]);
        ExtractionInformation_ID = Convert.ToInt32(r["ExtractionInformation_ID"]);
        NumberOfDaysPerBatch = Convert.ToInt32(r["NumberOfDaysPerBatch"]);
        Name = r["Name"].ToString();
        Retry = (RetryStrategy)Enum.Parse(typeof(RetryStrategy), r["Retry"].ToString());
    }

    public override string ToString()
    {
        return Name;
    }

    public bool MoreToFetch()
    {
        return ProgressDate < EndDate;
    }

    public bool ApplyRetryWaitStrategy(GracefulCancellationToken token, IDataLoadEventListener listener,
        int totalFailureCount, int consecutiveFailureCount)
    {
        return Retry switch
        {
            RetryStrategy.NoRetry => false,
            RetryStrategy.IterativeBackoff1Hour => IterativeBackoff1Hour(token, listener, totalFailureCount),
            _ => throw new ArgumentOutOfRangeException($"Unknown retry strategy {Retry}")
        };
    }

    private bool IterativeBackoff1Hour(GracefulCancellationToken token, IDataLoadEventListener listener,
        int totalFailureCount)
    {
        token.ThrowIfAbortRequested();

        int[] waitTimes = { 0, 1, 2, 3, 5, 8, 13, 21, 34 };

        if (totalFailureCount > waitTimes.Length) return false;

        // sleep for however many minutes we are up to
        var mins = waitTimes[totalFailureCount];
        listener.OnNotify(this,
            new NotifyEventArgs(ProgressEventType.Information, $"Waiting {mins} mins before retry"));

        // wait for the minutes but cancel if abort is hit
        Task.Delay((int)TimeSpan.FromMinutes(mins).TotalMilliseconds, token.AbortToken).Wait();

        token.ThrowIfAbortRequested();

        // then retry
        return true;
    }
}