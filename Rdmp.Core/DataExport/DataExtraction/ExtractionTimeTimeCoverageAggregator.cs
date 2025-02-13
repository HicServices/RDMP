// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;

namespace Rdmp.Core.DataExport.DataExtraction;

/// <summary>
/// Counts the number of unique patients / records encountered while executing an ExtractionConfiguration's dataset (linked to the project cohort).  The input
/// to this is a DataRow rather than an SQL query because the class is used at extraction time as we are writing records out to the ExtractionDirectory.
/// </summary>
public partial class ExtractionTimeTimeCoverageAggregator
{
    public int countOfBrokenDates { get; private set; }
    public int countOfNullsSeen { get; private set; }
    public Dictionary<DateTime, ExtractionTimeTimeCoverageAggregatorBucket> Buckets { get; private set; }

    private readonly ICatalogue _catalogue;
    private string _expectedTimeFieldInOutputBuffer;
    private string _expectedExtractionIdentifierInOutputBuffer;

    private bool haveCheckedTimeFieldExists;


    private const ExtractionTimeTimeCoverageAggregatorBucket.BucketSize BucketSize =
        ExtractionTimeTimeCoverageAggregatorBucket.BucketSize.Month;

    public ExtractionTimeTimeCoverageAggregator(ICatalogue catalogue, IExtractableCohort cohort)
    {
        _catalogue = catalogue;

        Buckets = new Dictionary<DateTime, ExtractionTimeTimeCoverageAggregatorBucket>();

        if (!catalogue.TimeCoverage_ExtractionInformation_ID.HasValue)
            return;
        try
        {
            _expectedTimeFieldInOutputBuffer = catalogue.TimeCoverage_ExtractionInformation.GetRuntimeName();
        }
        catch (Exception e)
        {
            throw new Exception(
                $"Could not resolve TimeCoverage_ExtractionInformation for Catalogue '{catalogue}',time coverage ExtractionInformationID was:{catalogue.TimeCoverage_ExtractionInformation_ID}",
                e);
        }

        _expectedExtractionIdentifierInOutputBuffer =
            cohort.GetQuerySyntaxHelper().GetRuntimeName(cohort.GetReleaseIdentifier());
    }

    private bool firstRow = true;

    public void ProcessRow(DataRow row)
    {
        //there is no configured time period field for the dataset, we have already thrown so just ignore calls
        if (string.IsNullOrWhiteSpace(_expectedTimeFieldInOutputBuffer))
            return;

        //check that the listed extraction field that has timeliness information in it is actually included in the column collection
        if (!haveCheckedTimeFieldExists)
            if (!row.Table.Columns.Contains(_expectedTimeFieldInOutputBuffer))
                throw new MissingFieldException(
                    $"Catalogue {_catalogue.Name} specifies that the time periodicity field of the dataset is called {_expectedTimeFieldInOutputBuffer} but the pipeline did not contain a field with this name, the fields in the pipeline are ({string.Join(",", row.Table.Columns.Cast<DataColumn>().Select(c => c.ColumnName))})");
            else
                haveCheckedTimeFieldExists = true;

        var value = row[_expectedTimeFieldInOutputBuffer];

        if (value == DBNull.Value)
        {
            countOfNullsSeen++;
            return;
        }

        DateTime key;

        object identifier = null;

        try
        {
            if (_expectedExtractionIdentifierInOutputBuffer != null)
                identifier = row[_expectedExtractionIdentifierInOutputBuffer];
        }
        catch (Exception)
        {
            //could not find the identifier in the output buffer, could be that there are multiple CHI columns e.g. CHI_Baby1, CHI_Baby2 or something
            //only swallow this exception (and abandon counting of distinct release identifiers) if it is the first output row.
            if (firstRow)
                _expectedExtractionIdentifierInOutputBuffer =
                    null; //give up trying to work out the extraction identifier
            else
                throw;
        }

        firstRow = false;

        try
        {
            if (value is string s)
            {
                if (string.IsNullOrWhiteSpace(s))
                {
                    countOfNullsSeen++;
                    return;
                }

                var valueAsString = s;

                //trim off times
                if (TimeRegex().IsMatch(valueAsString))
                    valueAsString = valueAsString[..^"00:00:00".Length].Trim();

                key = DateTime.ParseExact(valueAsString, "dd/MM/yyyy", null);
            }
            else if (value is DateTime time)
            {
                key = time;
            }
            else
            {
                key = Convert.ToDateTime(value);
            }
        }
        catch (Exception)
        {
            countOfBrokenDates++;
            return;
        }

        //drop the time part
        key = ExtractionTimeTimeCoverageAggregatorBucket.RoundDateTimeDownToNearestBucketFloor(key, BucketSize);

        if (!Buckets.Any())
        {
            //no buckets yet, create a bucket here
            Buckets.Add(key, new ExtractionTimeTimeCoverageAggregatorBucket(key));
            Buckets[key].SawIdentifier(identifier);
        }
        else
        {
            if (!Buckets.ContainsKey(key))
                ExtendBucketsToEncompas(key);

            Buckets[key].SawIdentifier(identifier);
        }
    }

    /// <summary>
    /// we want a consistent line of buckets with difference of BucketSize.  Use this method when you identify a DateTime that is (after
    /// rounding to the BucketSize, not in Buckets.  This method will add additional buckets up until the key has been reached (either
    /// adding new higher buckets if the key is > than the current maximum or adding additional lower buckets if it is lower.
    /// </summary>
    /// <param name="key"></param>
    private void ExtendBucketsToEncompas(DateTime key)
    {
        var addCount = 0;
        const int somethingHasGoneWrongIfAddedThisManyBuckets = 100000;

        if (Buckets.Keys.Max() < key)
        {
            //extend upwards
            var toAdd = Buckets.Keys.Max();

            while (toAdd != key && addCount < somethingHasGoneWrongIfAddedThisManyBuckets)
            {
                toAdd = ExtractionTimeTimeCoverageAggregatorBucket.IncreaseDateTimeBy(toAdd, BucketSize);
                Buckets.Add(toAdd, new ExtractionTimeTimeCoverageAggregatorBucket(toAdd));
                addCount++;
            }
        }
        else if (Buckets.Keys.Min() > key)
        {
            //extend downwards
            var toAdd = Buckets.Keys.Min();

            while (toAdd != key && addCount < somethingHasGoneWrongIfAddedThisManyBuckets)
            {
                toAdd = ExtractionTimeTimeCoverageAggregatorBucket.DecreaseDateTimeBy(toAdd, BucketSize);
                Buckets.Add(toAdd, new ExtractionTimeTimeCoverageAggregatorBucket(toAdd));
                addCount++;
            }
        }
        else
        {
            throw new Exception(
                $"Could not work out how to extend aggregation buckets where dictionary of datetimes had a max of {Buckets.Keys.Max()} and a min of{Buckets.Keys.Min()} which could not be extended in either direction to encompass {key}");
        }

        if (addCount == somethingHasGoneWrongIfAddedThisManyBuckets)
            throw new Exception($"Added {addCount} buckets without reaching the key... oops");
    }

    public static bool HasColumn(DbDataReader Reader, string ColumnName)
    {
        return Reader.GetSchemaTable().Rows.Cast<DataRow>().Any(row => row["ColumnName"].ToString() == ColumnName);
    }

    [GeneratedRegex("[0-2][0-9]:[0-5][0-9]:[0-5][0-9]")]
    private static partial Regex TimeRegex();
}