// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Rdmp.Core.Caching.Pipeline;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataQualityEngine.Data;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataQualityEngine.Reports;

/// <summary>
///     Generates two DataTable.  One containing the row counts (according to DQE) for every Catalogue in a LoadMetadata.
///     The second containing all cached fetch
///     counts and counts of all files in the Caching directory for the CacheProgress (if any) of the LoadProgress passed
///     into the contructor.  These tables are
///     intended to assist the user in rapidly determining how much of a given dataset collection based on a cache
///     fetch/load DLE job has currently been loaded
///     (according to the DQE).  See LoadProgressDiagram
/// </summary>
public class LoadProgressSummaryReport : ICheckable
{
    private readonly ILoadProgress _loadProgress;
    private readonly ILoadMetadata _loadMetadata;

    public bool DQERepositoryExists => dqeRepository != null;

    private DQERepository dqeRepository;

    public DataTable CataloguesPeriodictiyData;
    public DataTable CachePeriodictiyData;

    public DirectoryInfo ResolvedCachePath;

    public HashSet<Catalogue> CataloguesMissingDQERuns { get; private set; }
    private readonly ICacheProgress _cacheProgress;

    public Dictionary<Catalogue, Evaluation> CataloguesWithDQERuns { get; private set; }

    public LoadProgressSummaryReport(LoadProgress loadProgress)
    {
        _loadProgress = loadProgress;
        _loadMetadata = _loadProgress.LoadMetadata;
        _cacheProgress = _loadProgress.CacheProgress;

        try
        {
            dqeRepository = new DQERepository(loadProgress.CatalogueRepository);
        }
        catch (NotSupportedException)
        {
            dqeRepository = null;
        }
    }

    public void FetchDataFromDQE(ICheckNotifier notifier)
    {
        CataloguesWithDQERuns = new Dictionary<Catalogue, Evaluation>();
        CataloguesMissingDQERuns = new HashSet<Catalogue>();

        //tell them about the missing evaluations catalogues
        foreach (var catalogue in _loadMetadata.GetAllCatalogues().Cast<Catalogue>())
        {
            var evaluation = dqeRepository.GetMostRecentEvaluationFor(catalogue);

            if (evaluation == null)
            {
                //Catalogue has never been run in the DQE
                CataloguesMissingDQERuns.Add(catalogue);

                notifier?.OnCheckPerformed(
                    new CheckEventArgs(
                        $"Catalogue '{catalogue}' does not have any DQE evaluations on it in the DQE Repository.  You should run the DQE on the dataset",
                        CheckResult.Warning));
            }
            else
            {
                CataloguesWithDQERuns.Add(catalogue, evaluation);
            }
        }
        //The following code uses an epic pivot to produce something like:
        /*YearMonth	Year	Month	 6429	 6430
            1970-1	1970	    1	    4	    0
            1970-2	1970	    2	    2	    0
            1970-3	1970	    3	    6	    1
            1970-4	1970	    4	    4	    0
            1970-6	1970	    6	    2	    0*/


        if (!CataloguesWithDQERuns.Any())
            throw new Exception("There are no Catalogues that have had DQE run on them in this LoadMetadata");

        if (notifier != null)
            foreach (var catalogue in CataloguesWithDQERuns)
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        $"Found DQE Evaluations for Catalogue '{catalogue}'",
                        CheckResult.Success));

        using (var con = dqeRepository.GetConnection())
        {
            CataloguesPeriodictiyData = new DataTable();

            var cmd = dqeRepository.DiscoveredServer.GetCommand(
                GetTotalsByMonthSQL(CataloguesWithDQERuns.Keys.ToArray()), con);
            var da = dqeRepository.DiscoveredServer.GetDataAdapter(cmd);
            da.Fill(CataloguesPeriodictiyData);
        }

        //Now rename the columns from ID to the catalogue name
        foreach (DataColumn col in CataloguesPeriodictiyData.Columns)
            if (int.TryParse(col.ColumnName, out var cataId))
                col.ColumnName = CataloguesWithDQERuns.Keys.Single(c => c.ID == cataId).Name;

        //Now extend the X axis up to the cache fill location
        var cacheProgress = _loadProgress.CacheProgress;
        if (cacheProgress is { CacheFillProgress: not null })
            ExtendXAxisTill(cacheProgress.CacheFillProgress.Value);
    }


    private void ExtendXAxisTill(DateTime value)
    {
        var targetYear = value.Year;
        var targetMonth = value.Month;

        var row = CataloguesPeriodictiyData.Rows.Count - 1;
        var maxYear = Convert.ToInt32(CataloguesPeriodictiyData.Rows[row]["Year"]);
        var maxMonth = Convert.ToInt32(CataloguesPeriodictiyData.Rows[row]["Month"]);

        var currentMax = new DateTime(maxYear, maxMonth, 1);

        var columnCount = CataloguesPeriodictiyData.Columns.Count;

        while (
            //While the cache is years ahead of the end of the axis
            targetYear > currentMax.Year
            ||
            //or the cache is months ahead of the end of the axis
            (targetYear == currentMax.Year && targetMonth >= currentMax.Month))
        {
            currentMax = currentMax.AddMonths(1);

            var newRow = CataloguesPeriodictiyData.Rows.Add();
            newRow["YearMonth"] = $"{currentMax.Year}-{currentMax.Month}";
            newRow["Year"] = currentMax.Year;
            newRow["Month"] = currentMax.Month;

            //set all the catalogue row counts to 0 since we are extending the axis
            for (var i = 3; i < columnCount; i++)
                newRow[i] = 0;
        }
    }

    private void ExtendXAxisBackwardsTill(DateTime value)
    {
        var targetYear = value.Year;
        var targetMonth = value.Month;

        var minYear = Convert.ToInt32(CataloguesPeriodictiyData.Rows[0]["Year"]);
        var minMonth = Convert.ToInt32(CataloguesPeriodictiyData.Rows[0]["Month"]);

        var currentMin = new DateTime(minYear, minMonth, 1);

        var columnCount = CataloguesPeriodictiyData.Columns.Count;

        while (
            //While the cache is years ahead of the end of the axis
            targetYear < currentMin.Year
            ||
            //or the cache is months ahead of the end of the axis
            (targetYear == currentMin.Year && targetMonth <= currentMin.Month))
        {
            currentMin = currentMin.AddMonths(-1);

            var newRow = CataloguesPeriodictiyData.NewRow();

            newRow["YearMonth"] = $"{currentMin.Year}-{currentMin.Month}";
            newRow["Year"] = currentMin.Year;
            newRow["Month"] = currentMin.Month;

            //set all the catalogue row counts to 0 since we are extending the axis
            for (var i = 3; i < columnCount; i++)
                newRow[i] = 0;

            CataloguesPeriodictiyData.Rows.InsertAt(newRow, 0);
        }
    }


    public void Check(ICheckNotifier notifier)
    {
        try
        {
            dqeRepository = new DQERepository((ICatalogueRepository)_loadProgress.Repository);
        }
        catch (Exception ex)
        {
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    "Could not contact DQE server to check the 'real' coverage of the datasets linked to this LoadProgress, possibly because there isn't a DQE server yet.  You should create one in ManageExternalServers",
                    CheckResult.Fail, ex));
            return;
        }

        if (CataloguesPeriodictiyData == null)
            FetchDataFromDQE(notifier);

        if (CachePeriodictiyData == null)
            FetchCacheData(notifier);

        foreach (var cataloguesMissingDQERun in CataloguesMissingDQERuns)
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    $"Catalogue '{cataloguesMissingDQERun}' is associated with the load '{_loadMetadata}' but has never had a DQE run executed on it, you should configure some basic validation on it and choose a time periodicity column and execute a DQE run on it.",
                    CheckResult.Fail));
    }

    private void FetchCacheData(ICheckNotifier notifier)
    {
        if (_cacheProgress != null)
        {
            DateTime[] availableFiles;

            try
            {
                var cacheFileSystem =
                    new CachingPipelineUseCase(_cacheProgress).CreateDestinationOnly(
                        new FromCheckNotifierToDataLoadEventListener(notifier));

                var layout = cacheFileSystem.CreateCacheLayout();
                availableFiles = layout.GetSortedDateQueue(ThrowImmediatelyDataLoadEventListener.Quiet).ToArray();
                ResolvedCachePath =
                    layout.GetLoadCacheDirectory(new FromCheckNotifierToDataLoadEventListener(notifier));
            }
            catch (Exception e)
            {
                throw new Exception(
                    "Failed to generate cache layout/population information because the CacheProgress does not have a stable/working Pipeline Destination.  See Inner Exception for specifics",
                    e);
            }

            CachePeriodictiyData = new DataTable();

            CachePeriodictiyData.Columns.Add("YearMonth");
            CachePeriodictiyData.Columns.Add("Year", typeof(int));
            CachePeriodictiyData.Columns.Add("Month", typeof(int));

            CachePeriodictiyData.Columns.Add("Fetch Failures", typeof(int));
            CachePeriodictiyData.Columns.Add("Files In Cache", typeof(int));

            var allFailures =
                _cacheProgress.CacheFetchFailures
                    .Where(f => f.ResolvedOn == null)
                    .Select(f => f.FetchRequestStart)
                    .ToArray();
            Array.Sort(allFailures);

            var anyFailures = allFailures.Any();
            var anySuccesses = availableFiles.Any();

            //Make sure main data table has room on its X axis for the cache failures and loaded files
            if (anyFailures)
            {
                ExtendXAxisTill(allFailures.Max());
                ExtendXAxisBackwardsTill(allFailures.Min());
            }

            if (anySuccesses)
            {
                ExtendXAxisTill(availableFiles.Max());
                ExtendXAxisBackwardsTill(availableFiles.Min());
            }

            //now clone the data table but populate the axis with available/failures instead of
            foreach (DataRow originRow in CataloguesPeriodictiyData.Rows)
            {
                var year = Convert.ToInt32(originRow["Year"]);
                var month = Convert.ToInt32(originRow["Month"]);

                var newRow = CachePeriodictiyData.Rows.Add();

                newRow["YearMonth"] = originRow["YearMonth"];
                newRow["Year"] = originRow["Year"];
                newRow["Month"] = originRow["Month"];

                var totalFailuresForMonth = anyFailures
                    ? allFailures.Count(f => f.Year == year && f.Month == month)
                    : 0;
                var totalAvailableForMonth = anySuccesses
                    ? availableFiles.Count(f => f.Year == year && f.Month == month)
                    : 0;

                newRow["Fetch Failures"] = totalFailuresForMonth;
                newRow["Files In Cache"] = totalAvailableForMonth;
            }
        }
        else
        {
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    $"There is no Cache configured for LoadProgress '{_loadProgress}' (Not necessarily a problem e.g. if you have a RemoteTableAttacher or some other load module that uses LoadProgress directly, short cutting the need for a cache)",
                    CheckResult.Warning));
        }
    }


    private static string GetTotalsByMonthSQL(Catalogue[] catalogues)
    {
        return string.Format(GetTotalRecordsPerYearCountPivotByCatalogueSQL,
            string.Join(",", catalogues.Select(c => c.ID)));
    }

    private const string GetTotalRecordsPerYearCountPivotByCatalogueSQL =
        @"
--DYNAMICALLY FETCH COLUMN VALUES FOR USE IN PIVOT
DECLARE @Columns as VARCHAR(MAX)

--Get distinct values of the PIVOT Column if you have columns with values T and F and Z this will produce [T],[F],[Z] and you will end up with a pivot against these values
set @Columns = (
select
 ',' + QUOTENAME([Evaluation].[CatalogueID]) as [text()] 
FROM 
[PeriodicityState] Inner JOIN [Evaluation] ON [PeriodicityState].[Evaluation_ID] = [Evaluation].[ID]

WHERE [Evaluation].[CatalogueID] IS NOT NULL and [Evaluation].[CatalogueID] <> '' 
 AND 
(
--Evaluation ID
[Evaluation_ID] IN (  select ID from Evaluation e where CatalogueID in ({0}) AND DateOfEvaluation = (select max(DateOfEvaluation) from  Evaluation sub where e.CatalogueID = sub.CatalogueID))
)
AND 
PivotCategory = 'ALL'
group by 
[Evaluation].[CatalogueID]
order by 
count(*) desc
FOR XML PATH('') 
)

set @Columns = SUBSTRING(@Columns,2,LEN(@Columns))


DECLARE @FinalSelectList as VARCHAR(MAX)
SET @FinalSelectList ='Year,Month'
--Split up that pesky string in tsql which has the column names up into array elements again
DECLARE @value varchar(8000)
DECLARE @pos INT
DECLARE @len INT
set @pos = 0
set @len = 0

WHILE CHARINDEX(',', @Columns +',', @pos+1)>0
BEGIN
    set @len = CHARINDEX(',', @Columns +',', @pos+1) - @pos
    set @value = SUBSTRING(@Columns +',', @pos, @len)
        
    --We are constructing a version that turns: '[fish],[lama]' into 'ISNULL([fish],0) as [fish], ISNULL([lama],0) as [lama]'
    SET @FinalSelectList = @FinalSelectList + ', ISNULL(' + @value  + ',0) as ' + @value 

    set @pos = CHARINDEX(',', @Columns +',', @pos+@len) +1
END

--DYNAMIC PIVOT
declare @Query varchar(MAX)

SET @Query = '



--Would normally be Select * but must make it IsNull to ensure we see 0s instead of null
select YearMonth,'+@FinalSelectList+'
from
(

--Evaluations By ID
SELECT 
[Evaluation].[CatalogueID],
CONVERT(varchar(4),Year) + ''-'' + CONVERT(varchar(2),Month) YearMonth,
[PeriodicityState].[Year],
[PeriodicityState].[Month],
sum([PeriodicityState].[CountOfRecords]) AS MyCount
FROM 
[PeriodicityState] Inner JOIN [Evaluation] ON [PeriodicityState].[Evaluation_ID] = [Evaluation].[ID]

WHERE
(
--Evaluation ID
[Evaluation_ID] IN (  select ID from Evaluation e where CatalogueID in ({0}) AND DateOfEvaluation = (select max(DateOfEvaluation) from  Evaluation sub where e.CatalogueID = sub.CatalogueID))
)
AND 
PivotCategory = ''ALL''
group by 
[Evaluation].[CatalogueID],
CONVERT(varchar(4),Year) + ''-'' + CONVERT(varchar(2),Month),
[PeriodicityState].[Year],
[PeriodicityState].[Month]) s
PIVOT
(
    sum(MyCount)
    for CatalogueID in ('+@Columns+') --The dynamic Column list we just fetched at top of query
) piv
ORDER BY
Year,
Month'

EXECUTE(@Query)";
}