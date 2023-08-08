// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;

namespace Rdmp.Core.DataExport.DataExtraction;

/// <summary>
/// The number of unique patients and record count on a given day of an ExtractionTimeTimeCoverageAggregator.
/// </summary>
public class ExtractionTimeTimeCoverageAggregatorBucket
{
    public enum BucketSize
    {
        Month,
        Year,
        Day
    }

    public DateTime Time { get; set; }
    public int CountOfTimesSeen { get; set; }
    public int CountOfDistinctIdentifiers => _identifiersSeen.Count;
    private readonly HashSet<object> _identifiersSeen = new();

    public ExtractionTimeTimeCoverageAggregatorBucket(DateTime time)
    {
        Time = time;
    }

    public bool IsTimeInBucket(DateTime toCheck, BucketSize bucketSize)
    {
        var upperLimit = Time;
        if (bucketSize == BucketSize.Day)
            upperLimit = upperLimit.AddDays(1);
        else if (bucketSize == BucketSize.Month)
            upperLimit = upperLimit.AddMonths(1);
        else if (bucketSize == BucketSize.Year)
            upperLimit = upperLimit.AddYears(1);

        return toCheck >= Time && toCheck < upperLimit;
    }

    public void SawIdentifier(object identifier)
    {
        CountOfTimesSeen++;

        if (identifier == DBNull.Value)
            return;

        if (identifier == null)
            return;

        if (!_identifiersSeen.Contains(identifier))
            _identifiersSeen.Add(identifier);
    }

    public static DateTime RoundDateTimeDownToNearestBucketFloor(DateTime toRound, BucketSize bucketSize)
    {
        if (bucketSize == BucketSize.Day)
            return new DateTime(toRound.Year, toRound.Month, toRound.Day);

        if (bucketSize == BucketSize.Month)
            return new DateTime(toRound.Year, toRound.Month, 1);

        if (bucketSize == BucketSize.Year)
            return new DateTime(toRound.Year, 1, 1);

        throw new NotSupportedException($"Unknown bucket size {bucketSize}");
    }

    public static DateTime IncreaseDateTimeBy(DateTime toAdd, BucketSize bucketSize)
    {
        if (bucketSize == BucketSize.Day)
            return toAdd.AddDays(1);

        if (bucketSize == BucketSize.Month)
            return toAdd.AddMonths(1);

        if (bucketSize == BucketSize.Year)
            return toAdd.AddYears(1);

        throw new NotSupportedException($"Unknown bucket size {bucketSize}");
    }

    public static DateTime DecreaseDateTimeBy(DateTime toAdd, BucketSize bucketSize)
    {
        if (bucketSize == BucketSize.Day)
            return toAdd.AddDays(-1);

        if (bucketSize == BucketSize.Month)
            return toAdd.AddMonths(-1);

        if (bucketSize == BucketSize.Year)
            return toAdd.AddYears(-1);

        throw new NotSupportedException($"Unknown bucket size {bucketSize}");
    }
}